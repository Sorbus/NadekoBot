using Discord.Commands;
using Discord.Modules;
using NadekoBot.Classes;
using NadekoBot.Classes.JSONModels;
using NadekoBot.DataModels.Bartender;
using NadekoBot.Extensions;
using NadekoBot.Modules.Permissions.Classes;
using NadekoBot.Modules.Bartender.Commands;
using NadekoBot.Modules.Bartender.Prose;
using NadekoBot.Modules.Bartender.Helpers;
using System;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace NadekoBot.Modules.Bartender.Commands
{
    class Inventory : DiscordCommand
    {

        public static void addIntoInventory(Drink drink, Discord.User user)
        {
            addIntoInventory(drink, user, 1);
        }

        public static void addIntoInventory(Drink drink, Discord.User user, int count)
        {
            var db = DbHandler.Instance.GetAllRows<InventoryModel>();
            List<InventoryModel> inv = db.Where(t => t.UserId.Equals((long)user.Id)).Where(t => t.DrinkCode.Equals(drink.Code)).ToList();

            if (inv.Count > 0)
            {
                DbHandler.Instance.Delete<InventoryModel>((int)inv.First().Id);

                DbHandler.Instance.Save(new InventoryModel
                {
                    UserId = (long)user.Id,
                    Count = inv.First().Count + count,
                    DrinkCode = drink.Code
                });
            }
            else
            {
                DbHandler.Instance.Save(new InventoryModel
                {
                    UserId = (long)user.Id,
                    Count = 1,

                    DrinkCode = drink.Code
                });
            }
        }

        public static Boolean removeFromInventory(Drink drink, Discord.User user)
        {
            return removeFromInventory(drink, user, 1);
        }

        public static Boolean removeFromInventory(Drink drink, Discord.User user, int count)
        {
            var db = DbHandler.Instance.GetAllRows<InventoryModel>();
            List<InventoryModel> inv = db.Where(t => t.UserId.Equals((long)user.Id)).Where(t => t.DrinkCode.Equals(drink.Code)).ToList();

            if (inv.Count > 0)
            {
                DbHandler.Instance.Delete<InventoryModel>((int)inv.First().Id);

                if (inv.First().Count > count)
                {
                    DbHandler.Instance.Save(new InventoryModel
                    {
                        UserId = (long)user.Id,
                        Count = inv.First().Count - count,
                        DrinkCode = drink.Code
                    });
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public Inventory(DiscordModule module) : base(module)
        {
        }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Prefix + "inventory")
                .Alias(Module.Prefix + "inv")
                .Description($"Check on what's in your inventory. | `{Prefix}inventory' or `{Prefix}inv`")
                .Do(async e =>
                {
                    var db = DbHandler.Instance.GetAllRows<InventoryModel>();
                    List<InventoryModel> inv = db.Where(t => t.UserId.Equals((long)e.User.Id)).OrderBy(t => t.Id).ToList();

                    if (inv.Count == 0)
                    {
                        await e.Channel.SendMessage($"Your inventory is empty, **{e.User.Name}**.").ConfigureAwait(false);
                        return;
                    }

                    StringBuilder str = new StringBuilder(2000);
                    Drink d = null;
                    foreach (InventoryModel item in inv)
                    {
                        try
                        {
                            d = _.Drinks.Find(t => t.Code.Equals(item.DrinkCode));
                            if (d.Name != null)
                            {
                                str.Append("- " + Bar.NumberToWords(item.Count) + " " + d.Name + ((item.Count > 1) ? "s" : null) +
                                    ((d.Dragon) ? " (Key: " + item.GetHashCode().ToString() + ")" : " (" + item.DrinkCode + ")") + ".\n");
                            }
                            else
                            {
                                str.Append("- " + Bar.NumberToWords(item.Count) + " " + d.Code + ((item.Count > 1) ? "s.\n" : ".\n"));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Invalid drink code {d.Code} in {e.User.Name}'s inventory. Please correct.");
                        }
                    }
                    await e.Channel.SendMessage($"Here are the contents of your inventory, **{e.User.Name}**:\n{str.ToString().CapitalizeFirst()}").ConfigureAwait(false);
                });

            cgb.CreateCommand(Prefix + "drink")
                .Description($"Drink an item from your inventory. | `{Prefix}drink \"hard cider\"'")
                .Parameter("drink", ParameterType.Required)
                .Do(async e =>
                {
                    try
                    {
                        Drink drink = _.Drinks.Find(t => (t.Code.Equals(e.GetArg("drink")) || t.Name == e.GetArg("drink")));
                        if (drink == null && Regex.IsMatch(e.GetArg("drink"), @"^[0-9]+$"))
                        {
                            var db = DbHandler.Instance.GetAllRows<InventoryModel>();
                            var cd = db.Where(t => t.UserId.Equals((long)e.User.Id) && t.Id.Equals(e.GetArg("drink"))).FirstOrDefault();
                            if (cd != null)
                            {
                                drink = _.Drinks.Find(x => x.Code.Equals(cd.DrinkCode));
                            }
                        }
                        if (drink == null)
                        {
                            await e.Channel.SendMessage($"That's not a valid drink, **{e.User.Name}**.").ConfigureAwait(false);
                            return;
                        }
                        else if (!removeFromInventory(drink, e.User))
                        {
                            await e.Channel.SendMessage($"You don't have any of those, **{e.User.Name}**.").ConfigureAwait(false);
                            return;
                        }

                        await e.Channel.SendMessage($"**{e.User.Name}** drank " + ((drink.Name == null) ? (Desc.vowelFirst(drink.Code) ? "an " : "a ") +
                        drink.Code : (Desc.vowelFirst(drink.Name) ? "an " : "a ") +  drink.Name) + ".").ConfigureAwait(false);

                    }
                    catch (Exception ex) { Console.WriteLine(ex); }

                });

            cgb.CreateCommand(Prefix + "give")
                .Alias(Module.Prefix + "gift")
                .Description($"Give an item to someone else. | `{Prefix}give \"beer\" @someone' or `{Prefix}gift \"mead\" @someone'")
                .Parameter("drink", ParameterType.Required)
                .Parameter("target", ParameterType.Unparsed)
                .Do(async e =>
                {
                    var targetStr = e.GetArg("target")?.Trim();
                    if (string.IsNullOrWhiteSpace(targetStr))
                        return;
                    var target = e.Server.FindUsers(targetStr).FirstOrDefault();
                    if (target == null)
                    {
                        await e.Channel.SendMessage("No such person.").ConfigureAwait(false);
                        return;
                    }

                    Drink drink = _.Drinks.Find(t => (t.Code.Equals(e.GetArg("drink")) || t.Name == e.GetArg("drink")));
                    if (drink == null && Regex.IsMatch(e.GetArg("drink"), @"^[0-9]+$"))
                    {
                        var db = DbHandler.Instance.GetAllRows<InventoryModel>();
                        var cd = db.Where(t => t.UserId.Equals((long)e.User.Id) && t.Id.Equals(e.GetArg("drink"))).FirstOrDefault();
                        if (cd != null)
                        {
                            drink = _.Drinks.Find(x => x.Code.Equals(cd.DrinkCode));
                        }
                    }
                    if (drink == null)
                    {
                        await e.Channel.SendMessage($"That's not a valid drink, **{e.User.Name}**.").ConfigureAwait(false);
                        return;
                    }
                    else if (!removeFromInventory(drink, e.User))
                    {
                        await e.Channel.SendMessage($"You don't have any of those, **{e.User.Name}**.").ConfigureAwait(false);
                        return;
                    }

                    String msg = null;
                    if (drink.Name != null)
                    { msg = $"{e.User.Mention} sent {target.Mention} {(Desc.vowelFirst(drink.Name) ? "an" : "a")} {drink.Name}."; }
                    else
                    { msg = $"{e.User.Mention} sent {target.Mention} {(Desc.vowelFirst(drink.Code) ? "an" : "a")} {drink.Code}."; }

                    Inventory.addIntoInventory(drink, target);

                    await e.Channel.SendMessage(msg);
                });
        }
    }
}
