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
    class Menu : DiscordCommand
    {
        public Menu(DiscordModule module) : base(module)
        {
        }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Prefix + "menu")
                    .Description($"List items in one of the drink menu's categories. | `{Prefix}menu \"beer\"`")
                    .Parameter("category", ParameterType.Required)
                    .Do(async e =>
                    {
                        try
                        {
                            Dictionary<String, Drink> drink_cat = _.Drinks.Where(t => t.Category == e.GetArg("category").Trim().ToLowerInvariant()).ToDictionary(x => x.Code, y => y);

                            if (drink_cat.Count > 0)
                            {
                                StringBuilder str = new StringBuilder(2000);
                                foreach (KeyValuePair<string, Drink> d in drink_cat)
                                {
                                    if (!d.Value.Dragon)
                                    {
                                        if (d.Value.Name != null)
                                        {
                                            str.Append(" - " + d.Value.Name + " (" + d.Value.Code + ", " + d.Value.Cost + NadekoBot.Config.CurrencySign + ").\n");
                                        }
                                        else
                                        {
                                            str.Append(" - " + d.Value.Code + " (" + d.Value.Cost + NadekoBot.Config.CurrencySign + ").\n");
                                        }
                                    }
                                }
                                await e.Channel.SendMessage($"Items in the **{e.GetArg("category").ToUpper()}** category:\n{str.ToString().CapitalizeFirst()}").ConfigureAwait(false);
                            }
                            else
                            { await e.Channel.SendMessage($"We don't have any drinks in that category, {e.User.Mention}.").ConfigureAwait(false); }
                        }
                        catch (Exception ex)
                        { Console.WriteLine(ex); }
                    });

            cgb.CreateCommand(Prefix + "info")
                .Description($"Get additional information on a specific drink. | `{Prefix}info \"mead\"`")
                .Parameter("drink", ParameterType.Required)
                .Do(async e =>
                {
                    Drink drink;

                    drink = _.Drinks.Find(t => t.Code.Equals(e.GetArg("target")?.Trim().ToLowerInvariant()));

                    if (drink == null)
                    {
                        await e.Channel.SendMessage($"Sorry, {e.User.Mention}, that's not on the menu.").ConfigureAwait(false);
                        return;
                    }

                    if (drink.Dragon)
                    {
                        //await e.Message.Delete().ConfigureAwait(false);
                        await e.Channel.SendMessage($"Trying to order off-menu, dear?").ConfigureAwait(false);
                        return;
                    }

                    StringBuilder str = new StringBuilder(2000);
                    if (drink.Name != null)
                    {
                        str.Append(drink.Name + " (" + drink.Code + ", " + drink.Cost + NadekoBot.Config.CurrencySign + ").\n");
                    }
                    else
                    {
                        str.Append(drink.Code + " (" + drink.Cost + NadekoBot.Config.CurrencySign + ").\n");
                    }

                    str.Append(drink.Description);

                    await e.Channel.SendMessage(str.ToString().CapitalizeFirst()).ConfigureAwait(false);
                });

            cgb.CreateCommand(Prefix + "buy")
                    .Description($"Purchase a drink for yourself or someone else. | `{Prefix}buy \"sex on the beach\"` or `{Prefix}buy \"beer\" @someone`")
                    .Parameter("drink", ParameterType.Required)
                    .Parameter("target", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        var targetStr = e.GetArg("target")?.Trim();
                        var target = (string.IsNullOrWhiteSpace(targetStr)) ? e.User : e.Server.FindUsers(targetStr).FirstOrDefault();

                        Drink drink = _.Drinks.Find(t => t.Code.Equals(e.GetArg("drink").ToLowerInvariant()));

                        if (drink == null)
                        {
                            await e.Channel.SendMessage($"Sorry, {e.User.Mention}, that's not on the menu.").ConfigureAwait(false);
                            return;
                        }

                        if (drink.Dragon)
                        {
                            //await e.Message.Delete().ConfigureAwait(false);
                        }

                        //Payment~
                        var amount = drink.Cost;
                        var pts = DbHandler.Instance.GetStateByUserId((long)e.User.Id)?.Value ?? 0;
                        if (pts < amount)
                        {
                            await e.Channel.SendMessage($"{e.User.Mention} you don't have enough {NadekoBot.Config.CurrencyName}s! \nYou still need {amount - pts} {NadekoBot.Config.CurrencySign} to be able to do this!").ConfigureAwait(false);
                            return;
                        }
                        await FlowersHandler.RemoveFlowers(e.User, $"bought a {drink.Code}", amount).ConfigureAwait(false);

                        String msg = null;
                        if (target != e.User)
                        {
                            if (drink.Name != null)
                            { msg = $"{e.User.Mention} bought {target.Mention} {(Desc.vowelFirst(drink.Name) ? "an" : "a")} {drink.Name}."; }
                            else
                            { msg = $"{e.User.Mention} bought {target.Mention} {(Desc.vowelFirst(drink.Code) ? "an" : "a")} {drink.Code}."; }
                        }
                        else
                        {
                            if (drink.Name != null)
                            { msg = $"Enjoy your {drink.Name}, **{e.User.Name}**. "; }
                            else
                            { msg = $"Enjoy your {drink.Code}, **{e.User.Name}**. "; }
                        }

                        Inventory.addIntoInventory(drink, target);

                        await e.Channel.SendMessage(msg);
                    });
        }
    }
}
