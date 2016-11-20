using Discord.Commands;
using Discord.Modules;
using NadekoBot.Classes;
using NadekoBot.Classes.JSONModels;
using NadekoBot.DataModels;
using NadekoBot.Extensions;
using NadekoBot.Modules.Permissions.Classes;
using System;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace NadekoBot.Modules.Karma
{
    class KarmaModule : DiscordModule
    {
        public KarmaModule()
        {
            NadekoBot.Client.MessageReceived += CheckForKarma;
        }

        public override string Prefix { get; } = NadekoBot.Config.CommandPrefixes.Karma;

        private static List<String> listOfThanks = new List<string> { "thank", "thanks", "kudos" };

        // from https://stackoverflow.com/a/2730393
        private static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

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

        private ConcurrentDictionary<ulong, int> karmaCooldowns = new ConcurrentDictionary<ulong, int>();

        private Tuple<Boolean, String, int> GiveKarma(ulong targetUID, ulong userUID)
        {
            Boolean isGiven = false;
            String message = null;
            int award = 0;

            var db = DbHandler.Instance.GetAllRows<UserKarma>();
            Dictionary<long, UserKarma> karma = db.ToDictionary(x => x.UserID, y => y);
            UserKarma target;
            UserKarma user;

            // find or create the userkarma objects
            if (karma.ContainsKey((long)targetUID))
            { target = karma[(long)targetUID]; }
            else
            { target = new UserKarma { UserID = (long)targetUID, Karma = 0 }; }

            if (karma.ContainsKey((long)userUID))
            { user = karma[(long)userUID]; }
            else
            { user = new UserKarma { UserID = (long)userUID, Karma = 0 }; }


            if (!karmaCooldowns.Keys.Contains(userUID))
            {
                AddKarmaCooldown(userUID);

                target.Karma += 1;

                // delete old
                if (karma.ContainsKey(target.UserID))
                { DbHandler.Instance.Delete<UserKarma>(karma[(long)target.UserID].Id.Value); }
                if (karma.ContainsKey(user.UserID))
                { DbHandler.Instance.Delete<UserKarma>(karma[(long)user.UserID].Id.Value); }

                // store new
                DbHandler.Instance.Connection.Insert(target, typeof(UserKarma));
                DbHandler.Instance.Connection.Insert(user, typeof(UserKarma));

                // process message and award

                isGiven = true;
                if (target.Karma == 1) { message = "[target] has recieved their first [type]!"; }
                else if (target.Karma % 5000 == 0) { message = "Thanks to [user], [target] has reached a enormous milestone!"; award = 500; }
                else if (target.Karma % 1000 == 0) { message = "Thanks to [user], [target] has reached a huge milestone!"; award = 250; }
                else if (target.Karma % 500 == 0) { message = "Thanks to [user], [target] has reached a major milestone!"; award = 100; }
                else if (target.Karma % 100 == 0) { message = "Thanks to [user], [target] has reached a moderate milestone!"; award = 50; }
                else if (target.Karma % 50 == 0) { message = "Thanks to [user], [target] has reached a minor milestone!"; award = 25; }
                else if (target.Karma % 10 == 0) { message = "Thanks to [user], [target] has reached a tiny milestone!"; award = 5; }
                else { message = "[user] has given [target] [type]."; }

                return Tuple.Create(isGiven, message, award);
            }
            else { return Tuple.Create(false, "", 0); }
        }

        private async void CheckForKarma(object sender, Discord.MessageEventArgs e)
        {
            try
            {
                if (e.Server == null || e.Channel.IsPrivate || e.Message.IsAuthor)
                    return;

                var toMatch = @"(thank you|thanks|thank|thnx|thnx|tank|thx|shot|danke|praise be to|praise|kudos|tack) ([\w@#]+|""[\w@# ]+"")";

                if (Regex.IsMatch(e.Message.Text.ToLowerInvariant(), toMatch, RegexOptions.IgnoreCase))
                {
                    MatchCollection matches = Regex.Matches(e.Message.Text.ToLowerInvariant(), toMatch);

                    String targetStr = "";
                    foreach (Match match in matches)
                    {
                        targetStr = match.Groups[2].Value;

                        if (string.IsNullOrWhiteSpace(targetStr))
                            return;
                        var target = e.Server.FindUsers(targetStr).FirstOrDefault();
                        if (target == null)
                        { return; }
                        else if (target == e.User)
                        {
                            return;
                        }

                        Tuple<Boolean, String, int> message = GiveKarma(target.Id, e.User.Id);

                        if (message.Item1)
                        {
                            if (message.Item3 != 0)
                            {
                                await FlowersHandler.AddFlowersAsync(target, "Reached a milestone", message.Item3, true).ConfigureAwait(false);
                                await target.SendMessage($":crown:Congratulations!:crown:\nYou received: {message.Item3} {NadekoBot.Config.CurrencySign} for being a swell person.").ConfigureAwait(false);
                            }

                            if (message.Item2 != null)
                            {
                                String str = message.Item2.Replace("[user]", e.User.Name).Replace("[type]", "kudos").Replace("[target]", target.Name);
                                if (message.Item3 > 0)
                                { str += $" They have been awarded {NumberToWords(message.Item3)}{NadekoBot.Config.CurrencySign}!"; }
                                var msg = await e.Channel.SendMessage(str).ConfigureAwait(false);

                                ThreadPool.QueueUserWorkItem(async (state) =>
                                {
                                    try
                                    {
                                        await Task.Delay(6000).ConfigureAwait(false);
                                        await msg.Delete().ConfigureAwait(false);
                                    }
                                    catch { }
                                });
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public override void Install(ModuleManager manager)
        {
            manager.CreateCommands("", cgb =>
            {
                cgb.AddCheck(PermissionChecker.Instance);

                commands.ForEach(cmd => cmd.Init(cgb));

                cgb.CreateCommand(Prefix + "?")
                    .Description($"Check your own karma. | `{Prefix}?`")
                    .Do(async e =>
                    {
                        var db = DbHandler.Instance.GetAllRows<UserKarma>();
                        Dictionary<long, UserKarma> karma = db.ToDictionary(x => x.UserID, y => y);
                        UserKarma user;

                        // find or create the userkarma objects
                        if (karma.ContainsKey((long)e.User.Id))
                        { user = karma[(long)e.User.Id]; }
                        else
                        {
                            await e.Channel.SendMessage("You have 0 karma.").ConfigureAwait(false);
                            return;
                        }

                        String str = $"You have {user.Karma} karma.";

                        // if (user.Karma < 10) { str += $"You have {NumberToWords(user.Karma)} karma."; }
                        // else if (user.Karma < 100) {
                        //     str += $"You have between {NumberToWords(user.Karma - (user.Karma % 10)).Trim()} and {NumberToWords(10 + user.Karma - (user.Karma % 10)).Trim()} karma.";
                        // }
                        // else if (user.Karma < 1000) {
                        // str += $"You have between {NumberToWords(user.Karma - (user.Karma % 50)).Trim()} and {NumberToWords(50 + user.Karma - (user.Karma % 50)).Trim()} karma.";
                        // }
                        // else {
                        // str += $"You have between {NumberToWords(user.Karma - (user.Karma % 100)).Trim()} and {NumberToWords(100 + user.Karma - (user.Karma % 100)).Trim()} karma.";
                        // }

                        await e.Channel.SendMessage(str).ConfigureAwait(false);
                    });

            });
        }

        public void AddKarmaCooldown(ulong userId)
        {
            karmaCooldowns.TryAdd(userId, 0);
            Task.Run(async () =>
            {
                int cd;
                if (!karmaCooldowns.TryGetValue(userId, out cd))
                {
                    return;
                }
                if (karmaCooldowns.TryAdd(userId, 0))
                {
                    await Task.Delay(30 * 1000);
                    int throwaway;
                    karmaCooldowns.TryRemove(userId, out throwaway);
                }

            });
        }
    }
}
