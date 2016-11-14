using Discord;
using Discord.Commands;
using NadekoBot.Classes;
using NadekoBot.Extensions;
using NadekoBot.DataModels;
using NadekoBot.Modules.Permissions.Classes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Games.Commands
{
    /// <summary>
    /// Flower picking/planting idea is given to me by its
    /// inceptor Violent Crumble from Game Developers League discord server
    /// (he has !cookie and !nom) Thanks a lot Violent!
    /// Check out GDL (its a growing gamedev community):
    /// https://discord.gg/0TYNJfCU4De7YIk8
    /// </summary>
    class Festival : DiscordCommand
    {

        private Random rng;
        public Festival(DiscordModule module) : base(module)
        {
            NadekoBot.Client.MessageReceived += PotentialFestivalItem;
            rng = new Random();
        }

        // from https://stackoverflow.com/a/2730393
        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }
            return words;
        }

        private static readonly ConcurrentDictionary<ulong, DateTime> festivalCooldowns = new ConcurrentDictionary<ulong, DateTime>();
        private static readonly ConcurrentDictionary<ulong, String> festivalDrops = new ConcurrentDictionary<ulong, String>();

        private async void PotentialFestivalItem(object sender, Discord.MessageEventArgs e)
        {
            try
            {
                if (e.Server == null || e.Channel.IsPrivate || e.Message.IsAuthor)
                    return;
                var config = Classes.SpecificConfigurations.Default.Of(e.Server.Id);
                var now = DateTime.Now;
                int cd;
                DateTime lastSpawned;
                try
                {
                    if (config.FestivalChannels.TryGetValue(e.Channel.Id, out cd))
                        if (!festivalCooldowns.TryGetValue(e.Channel.Id, out lastSpawned) || (lastSpawned + new TimeSpan(0, cd, 0)) < now)
                        {
                            var rnd = Math.Abs(rng.Next(0, 21));
                            if (rnd == 0)
                            {
                                rnd = Math.Abs(rng.Next(0, 9));
                                string message = "";
                                string file = "";
                                string droptype = "";
                                switch (rnd)
                                {
                                    case 0: //mysterious gem
                                        message = "A mysterious gem has fallen from the sky!";
                                        file = "data/festival_images/rock.jpg";
                                        droptype = "gem";
                                        break;
                                    case 1:
                                    case 2: //focusing lens
                                        message = "A focusing lens has fallen from the sky!";
                                        file = "data/festival_images/lens.jpg";
                                        droptype = "lens";
                                        break;
                                    case 3: //phlebotinum
                                        message = "A bar of phlebotinum has fallen from the sky!";
                                        file = "data/festival_images/bar.jpg";
                                        droptype = "bar";
                                        break;
                                    case 4:
                                    case 5: //perplexing mechanism
                                        message = "Part of a perplexing mechanism has fallen from the sky!";
                                        file = "data/festival_images/clockwork.jpg";
                                        droptype = "gear";
                                        break;
                                    default: //curved metal plate
                                        message = "A curved metal plate has fallen from the sky!";
                                        file = "data/festival_images/plate.png";
                                        droptype = "plate";
                                        break;
                                }
                                festivalDrops.AddOrUpdate(e.Channel.Id, droptype, (i, d) => droptype);

                                var msgs = new[] { await e.Channel.SendFile(file), await e.Channel.SendMessage(message + $" Take it by typing `{Prefix}take`.") };
                                fallenFestivalChannels.AddOrUpdate(e.Channel.Id, msgs, (u, m) => { m.ForEach(async msgToDelete => { try { await msgToDelete.Delete(); } catch { } }); return msgs; });
                                festivalCooldowns.AddOrUpdate(e.Channel.Id, now, (i, d) => now);
                            }
                        }
                        else { config.FestivalChannels.TryAdd(e.Channel.Id, cd); }
                }
                catch (Exception ex) { Console.WriteLine(ex); }
            }
            catch { }
        }
        //channelid/messageid pair
        ConcurrentDictionary<ulong, IEnumerable<Message>> fallenFestivalChannels = new ConcurrentDictionary<ulong, IEnumerable<Message>>();

        private SemaphoreSlim locker = new SemaphoreSlim(1, 1);

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "take")
                .Description($"Take an item dropped in this channel | `{Prefix}take`")
                .Do(async e =>
                {
                    IEnumerable<Message> msgs;

                    await e.Message.Delete().ConfigureAwait(false);
                    if (!fallenFestivalChannels.TryRemove(e.Channel.Id, out msgs))
                        return;

                    foreach (var msgToDelete in msgs)
                        await msgToDelete.Delete().ConfigureAwait(false);

                    var db = DbHandler.Instance.GetAllRows<FestivalStats>();
                    Dictionary<long, FestivalStats> setUsers = db.ToDictionary(x => x.UserID, y => y);
                    FestivalStats user;

                    if (setUsers.ContainsKey((long)e.User.Id))
                    {
                        user = setUsers[(long)e.User.Id];
                    }
                    else
                    {
                        user = new FestivalStats { UserID = (long)e.User.Id, Lens = 0, Bars = 0, Gears = 0, Gems = 0, Plates = 0, Assembled = 0 };
                    }

                    var config = SpecificConfigurations.Default.Of(e.Server.Id);
                    string message = "";
                    int throwaway;
                    switch (festivalDrops[e.Channel.Id])
                    {
                        case "gem":
                            user.Gems += 1;
                            message = " picked up a mysterious gem. It's cool to the touch.";
                            break;
                        case "lens":
                            user.Lens += 1;
                            message = " carefully picked up a focusing lens. They're being careful not to smudge it.";
                            break;
                        case "bar":
                            user.Bars += 1;
                            message = " picked up a bar of phlebotinum. They're not stupid, so they used tongs.";
                            break;
                        case "gear":
                            user.Gears += 1;
                            message = " picked up a perplexing mechanism. Its gears seem jammed.";
                            break;
                        case "plate":
                            user.Plates += 1;
                            message = " picked up a curved metal plate. It looks like there might be mounting points on the inside.";
                            break;
                        default:
                            message = " has broken something ...";
                            break;
                    }
                    DbHandler.Instance.Save(user);

                    if (user.Gems >= 1 && user.Lens >= 2 && user.Bars >= 1 && user.Gears >= 2 && user.Plates >= 3)
                    {
                        message += $" They might have enough pieces to `{Prefix}assemble` something interesting ...";
                    }

                    var msg = await e.Channel.SendMessage($"**{e.User.Name}** " + message).ConfigureAwait(false);

                    ThreadPool.QueueUserWorkItem(async (state) =>
                    {
                        try
                        {
                            await Task.Delay(10000).ConfigureAwait(false);
                            await msg.Delete().ConfigureAwait(false);
                            await e.Message.Delete().ConfigureAwait(false);
                        }
                        catch { }
                    });
                });

            cgb.CreateCommand(Prefix + "point")
                .Description($"Do ... something. | `{Prefix}point`")
                .Do(async e =>
                {
                    var db = DbHandler.Instance.GetAllRows<FestivalStats>();
                    Dictionary<long, FestivalStats> setUsers = db.ToDictionary(x => x.UserID, y => y);
                    FestivalStats user;

                    if (setUsers.ContainsKey((long)e.User.Id))
                    {
                        user = setUsers[(long)e.User.Id];
                        if (user.Assembled > 0)
                        {
                            await e.Channel.SendMessage(
                                $"{e.User.Mention} points their focusing mechanism up towards the swollen supermoon. For a moment a bright beam of light connects them," +
                                $" the mechanism, and the moon. When it fades the moon looms slightly larger in the sky and {e.User.Mention} seems a bit **more** than they" +
                                " were before. The mechanism crumbles away into dust."
                                ).ConfigureAwait(false);
                            user.Assembled -= 1;

                            // await e.User.SendMessage
                            await FlowersHandler.AddFlowersAsync(e.User, "a reward for your accomplishment", 250).ConfigureAwait(false);

                            DbHandler.Instance.Save(user);
                            return;
                        }
                    }

                    var msg = await e.Channel.SendMessage($"You don't have a focusing mechanism, {e.User.Mention}.").ConfigureAwait(false);

                    ThreadPool.QueueUserWorkItem(async (state) =>
                    {
                        try
                        {
                            await Task.Delay(30000).ConfigureAwait(false);
                            await msg.Delete().ConfigureAwait(false);
                            await e.Message.Delete().ConfigureAwait(false);
                        }
                        catch { }
                    });
                });

            cgb.CreateCommand(Prefix + "assemble")
        .Description($"Put everything together. | `{Prefix}assemble`")
        .Parameter("target", ParameterType.Unparsed)
        .Do(async e =>
        {
            var db = DbHandler.Instance.GetAllRows<FestivalStats>();
            Dictionary<long, FestivalStats> setUsers = db.ToDictionary(x => x.UserID, y => y);
            FestivalStats user;

            string message = "";

            if (setUsers.ContainsKey((long)e.User.Id))
            {
                user = setUsers[(long)e.User.Id];

                if (user.Gems >= 1 && user.Lens >= 2 && user.Bars >= 1 && user.Gears >= 2 && user.Plates >= 3)
                {
                    message = $"{e.User.Mention} has assembled a focusing mechanism. It seems to want to be `{Prefix}point`ed at something ...";

                    user.Bars -= 1;
                    user.Gears -= 2;
                    user.Gems -= 1;
                    user.Lens -= 2;
                    user.Plates -= 3;

                    user.Assembled += 1;

                    DbHandler.Instance.Save(user);
                }
                else
                {
                    while (message == "")
                    {
                        switch (Math.Abs(rng.Next(0, 5)))
                        {
                            case 0:
                                if (user.Gems > 0) { message = $"{e.User.Mention} is playing with a mysterious gem. It glitters when the moonlight hits it."; }
                                break;
                            case 1:
                                if (user.Gears >= 2) { message = $"{e.User.Mention} has found that two of their perplexing mechanisms fit together perfectly."; }
                                else if (user.Gears == 1) { message = $"{e.User.Mention} is playing with a perplexing mechanism. Something seems to be missing ..."; }
                                break;
                            case 2:
                                if (user.Lens > 0) { message = $"{e.User.Mention} is using a focusing lens to burn patterns into the ground. It twitches against their hand, almost as if it had a mind of its own."; }
                                break;
                            case 3:
                                if (user.Plates >= 3) { message = $"{e.User.Mention} has found a way to fit three of their curved metal plates together into a casing. If only they had something to put inside it."; }
                                else if (user.Plates == 2) { message = $"{e.User.Mention} has found a way to fit two of their curved metal platestogether, but something seems to be missing ..."; }
                                else if (user.Plates == 1) { message = $"{e.User.Mention} is inspecting a curved metal plate. Something seems to be missing ..."; }
                                break;
                            case 4:
                                if (user.Bars > 0) { message = $"{e.User.Mention} is playing with a bar of phlebotinum. That was probably a bad idea."; }
                                break;
                        }
                    }
                }
            }
            else
            {
                message = $"You don't have anything yet, {e.User.Mention}.";
            }

            var msg = await e.Channel.SendMessage(message).ConfigureAwait(false);


            ThreadPool.QueueUserWorkItem(async (state) =>
            {
                try
                {
                    await Task.Delay(30000).ConfigureAwait(false);
                    await msg.Delete().ConfigureAwait(false);
                    await e.Message.Delete().ConfigureAwait(false);
                }
                catch { }
            });

        });

            cgb.CreateCommand(Prefix + "check")
                .Description($"Check how many things you've collected for the festival. | `{Prefix}check`")
                .Do(async e =>
                {
                    var db = DbHandler.Instance.GetAllRows<FestivalStats>();
                    Dictionary<long, FestivalStats> setUsers = db.ToDictionary(x => x.UserID, y => y);
                    FestivalStats user;

                    string message = "";

                    if (setUsers.ContainsKey((long)e.User.Id))
                    {
                        user = setUsers[(long)e.User.Id];

                        message = $"You have {NumberToWords(user.Gems)} mysterious gem(s), {NumberToWords(user.Gears)} perplexing mechanism(s)," +
                            $" {NumberToWords(user.Lens)} focusing lens(es), {NumberToWords(user.Bars)} phlebotinum bar(s), and" +
                            $" {NumberToWords(user.Plates)} curved metal plate(s), {e.User.Mention}.";
                        if (user.Assembled > 0) { message += $" You also have {NumberToWords(user.Assembled)} assembled focusing mechanism(s). You should `{Prefix}point` them at something!"; }

                        if (user.Gems >= 1 && user.Lens >= 2 && user.Bars >= 1 && user.Gears >= 2 && user.Plates >= 3)
                        {
                            message += $" That might be enough to `{Prefix}assemble` something interesting ...";
                        }
                    }
                    else
                    {
                        message = $"You don't have anything yet, {e.User.Mention}.";
                    }

                    var msg = await e.Channel.SendMessage(message).ConfigureAwait(false);

                    ThreadPool.QueueUserWorkItem(async (state) =>
                    {
                        try
                        {
                            await Task.Delay(10000).ConfigureAwait(false);
                            await msg.Delete().ConfigureAwait(false);
                            await e.Message.Delete().ConfigureAwait(false);
                        }
                        catch { }
                    });
                });

            cgb.CreateCommand(Prefix + "festival")
                .Alias(Prefix + "fe")
                .Description($"Toggles the festival on this channel. Every posted message will have 5% chance to spawn an item." +
                    " Optional parameter cooldown time in minutes, 2 minutes by default. Requires Manage Messages permission. | `{Prefix}gc` or `{Prefix}gc 60`")
                .AddCheck(SimpleCheckers.ManageMessages())
                .Parameter("cd", ParameterType.Unparsed)
                .Do(async e =>
                {
                    var cdStr = e.GetArg("cd");
                    int cd = 2;
                    if (!int.TryParse(cdStr, out cd) || cd < 0)
                    {
                        cd = 2;
                    }
                    var config = SpecificConfigurations.Default.Of(e.Server.Id);
                    int throwaway;
                    if (config.FestivalChannels.TryRemove(e.Channel.Id, out throwaway))
                    {
                        await e.Channel.SendMessage("`The lunar festival has ended.`").ConfigureAwait(false);
                    }
                    else
                    {
                        if (config.FestivalChannels.TryAdd(e.Channel.Id, cd))
                            await e.Channel.SendMessage($"`The lunar festival has begun!`").ConfigureAwait(false);
                    }
                });

            cgb.CreateCommand(Prefix + "gift")
                    .Description($"Give a part to another user. Use `{Prefix}check` to see which parts you have. | `{Prefix}gift \"part\" @someguy`")
                    .Parameter("part", ParameterType.Required)
                    .Parameter("target", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        var move = e.GetArg("part");
                        var targetStr = e.GetArg("target")?.Trim();
                        if (string.IsNullOrWhiteSpace(targetStr))
                            return;
                        var target = e.Server.FindUsers(targetStr).FirstOrDefault();
                        string message = "";

                        if (target == null)
                        {
                            message = "No such person.";
                        }
                        else if (target == e.User)
                        {
                            message = "You can't give yourself a gift.";
                        }
                        else
                        {
                            List<String> parts = new List<String> { "gem", "gear", "bar", "plate", "lens" };
                            if (parts.Contains(e.GetArg("part").ToLowerInvariant()))
                            {
                                var db = DbHandler.Instance.GetAllRows<FestivalStats>();
                                Dictionary<long, FestivalStats> setUsers = db.ToDictionary(x => x.UserID, y => y);
                                FestivalStats user;
                                FestivalStats targetf;

                                if (setUsers.ContainsKey((long)target.Id))
                                {
                                    targetf = setUsers[(long)target.Id];
                                }
                                else
                                {
                                    targetf = new FestivalStats { UserID = (long)target.Id, Lens = 0, Bars = 0, Gears = 0, Gems = 0, Plates = 0, Assembled = 0 };
                                }

                                if (setUsers.ContainsKey((long)e.User.Id))
                                {
                                    user = setUsers[(long)e.User.Id];

                                    switch (e.GetArg("part").ToLowerInvariant())
                                    {
                                        case "gem":
                                            if (user.Gems > 0)
                                            {
                                                user.Gems -= 1;
                                                targetf.Gems += 1;
                                                message = $"{e.User.Mention} gave {target.Mention} a mysterious gem.";
                                            }
                                            else { message = "You don't have that part.";  }
                                            break;
                                        case "gear":
                                            if (user.Gears > 0)
                                            {
                                                user.Gears -= 1;
                                                targetf.Gears += 1;
                                                message = $"{e.User.Mention} gave {target.Mention} a perplexing mechanism.";
                                            }
                                            else { message = "You don't have that part."; }
                                            break;
                                        case "bar":
                                            if (user.Bars > 0)
                                            {
                                                user.Bars -= 1;
                                                targetf.Bars += 1;
                                                message = $"{e.User.Mention} gave {target.Mention} a bar of phlebotinum.";
                                            }
                                            else { message = "You don't have that part."; }
                                            break;
                                        case "plate":
                                            if (user.Plates > 0)
                                            {
                                                user.Plates -= 1;
                                                targetf.Plates += 1;
                                                message = $"{e.User.Mention} gave {target.Mention} a curved metal plate.";
                                            }
                                            else { message = "You don't have that part."; }
                                            break;
                                        case "lens":
                                            if (user.Lens > 0)
                                            {
                                                user.Lens -= 1;
                                                targetf.Lens += 1;
                                                message = $"{e.User.Mention} gave {target.Mention} a focusing lens.";
                                            }
                                            else { message = "You don't have that part."; }
                                            break;
                                    }

                                    DbHandler.Instance.Save(user);
                                    DbHandler.Instance.Save(targetf);
                                }
                                else
                                {
                                    message = "You don't have that part.";
                                }
                            }
                            else
                            {
                                message = "Part must be one of `gem`, `gear`, `bar`, `plate`, or `lens`.";
                            }
                        }

                        var msg = await e.Channel.SendMessage(message).ConfigureAwait(false);

                        ThreadPool.QueueUserWorkItem(async (state) =>
                        {
                            try
                            {
                                await Task.Delay(10000).ConfigureAwait(false);
                                await msg.Delete().ConfigureAwait(false);
                                await e.Message.Delete().ConfigureAwait(false);
                            }
                            catch { }
                        });
                    });
        }

        int GetRandomNumber()
        {
            using (RNGCryptoServiceProvider rg = new RNGCryptoServiceProvider())
            {
                byte[] rno = new byte[4];
                rg.GetBytes(rno);
                int randomvalue = BitConverter.ToInt32(rno, 0);
                return randomvalue;
            }
        }
    }
}