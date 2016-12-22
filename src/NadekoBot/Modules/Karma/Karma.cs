using Discord.Commands;
using NadekoBot.Extensions;
using System.Linq;
using Discord;
using NadekoBot.Services;
using System.Threading.Tasks;
using NadekoBot.Attributes;
using System;
using System.IO;
using System.Text;
using Discord.WebSocket;
using System.Collections;
using System.Collections.Generic;
using NadekoBot.Services.Database;
using System.Threading;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using NadekoBot.Services.Database.Models;
using NLog;

namespace NadekoBot.Modules.Karma
{
    [NadekoModule("Karma", ".")]
    public partial class Karma : DiscordModule
    {
        private ConcurrentDictionary<ulong, Boolean> karmaCooldowns = new ConcurrentDictionary<ulong, Boolean>();
        private String toMatch = @"(?:[Tt](?:hank(?:s*)(?:,*)(?: +you)*|h(?:n*)x|a(?:nk|ck))|[Ss]hot|[Dd]anke|[Pp]raise(?: +be +to)*|[Kk]udos) +(?:([\w]+)|<@!([0-9]{18})>)";

        public Karma(ILocalization loc, CommandService cmds, ShardedDiscordClient client) : base(loc, cmds, client)
        {
            NadekoBot.Client.MessageReceived += CheckForKarma;
        }

        [NadekoCommand, Usage, Description, Aliases]
        [Priority(0)]
        public async Task KarmaStatus(IUserMessage umsg, [Remainder] IUser user = null)
        {
            var channel = umsg.Channel;

            user = user ?? umsg.Author;

            await channel.SendMessageAsync($"{user.Mention} has {GetKarma(user.Id).ToWords()} karma.").ConfigureAwait(false);
        }

        [NadekoCommand, Usage, Description, Aliases]
        [Priority(1)]
        public async Task KarmaStatus(IUserMessage umsg, ulong userId)
        {
            var channel = umsg.Channel;

            await channel.SendMessageAsync($"You have {GetKarma(userId).ToWords()} karma, **{umsg.Author.Username}**.").ConfigureAwait(false);
        }

        private async Task CheckForKarma(IMessage imsg)
        {
            var msg = imsg as IUserMessage;
            if (msg == null || msg.IsAuthor() || msg.Author.IsBot)
                return;

            var channel = imsg.Channel as ITextChannel;
            if (channel == null)
                return;

            if (karmaCooldowns.Keys.Contains(msg.Author.Id))
            {
                //return Task.CompletedTask;
            }
            
            MatchCollection matches = Regex.Matches(msg.Content.ToLowerInvariant(), toMatch, RegexOptions.IgnoreCase);

            if (matches.Count > 0)
            {
                IGuildUser target = null;

                foreach (Match match in matches)
                {

                    if (match.Groups[1].Value.Length > 0)
                    {
                        target = channel.Guild.GetUsers().FirstOrDefault(x => (x.Username.ToLowerInvariant() == match.Groups[1].Value.Trim()));
                        if (target == null)
                        {
                            break;
                        }
                    }
                    else if (match.Groups[2].Value.Length > 0)
                    {
                        try
                        {
                            target = channel.Guild.GetUser(ulong.Parse(match.Groups[2].Value));
                        }
                        catch
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                    if (target.Id.Equals(imsg.Author.Id))
                    {
                        break;
                    }

                    AddKarmaCooldown(imsg.Author.Id);
                    await KarmaHandler.AddKarmaAsync((target.Id), 1).ConfigureAwait(true);

                    StringBuilder message = new StringBuilder(2000);
                    message.Append($"**{msg.Author.Username}** gave karma to {target.Mention}!");
                    long karma = GetKarma(target.Id);

                    if (karma == 1) { message.Append($" The first but not the last."); }
                    else if (karma % 10 == 0) {
                        if (karma % 5000 == 0) { message.Append($" This calls for a celebration!"); await CurrencyHandler.AddCurrencyAsync(target, "Karma.", 500, true).ConfigureAwait(false); }
                        else if (karma % 1000 == 0) { message.Append($" This calls for a present!"); await CurrencyHandler.AddCurrencyAsync(target, "Karma.", 250, true).ConfigureAwait(false); }
                        else if (karma % 500 == 0) { message.Append($" This calls for a gift!"); await CurrencyHandler.AddCurrencyAsync(target, "Karma.", 100, true).ConfigureAwait(false); }
                        else if (karma % 100 == 0) { message.Append($" What a wonderful person."); await CurrencyHandler.AddCurrencyAsync(target, "Karma.", 50, true).ConfigureAwait(false); }
                        else if (karma % 50 == 0) { message.Append($" What a nice person."); await CurrencyHandler.AddCurrencyAsync(target, "Karma.", 25, true).ConfigureAwait(false); }
                        else if (karma % 10 == 0) { message.Append($" What a swell person."); await CurrencyHandler.AddCurrencyAsync(target, "Karma.", 5, true).ConfigureAwait(false); }
                    }

                    var toDelete = await channel.SendMessageAsync(message.ToString()).ConfigureAwait(false);

                    var t = Task.Run(async () =>
                    {
                        await Task.Delay(30 * 1000).ConfigureAwait(false);
                        try { await toDelete.DeleteAsync().ConfigureAwait(false); } catch { }
                    });

                    return; ;
                }
            }

            return;
        }

        private void AddKarmaCooldown(ulong userId)
        {
            karmaCooldowns.TryAdd(userId, true);
            Task.Run(async () =>
            {
                Boolean cd;
                if (!karmaCooldowns.TryGetValue(userId, out cd))
                {
                    return;
                }
                if (karmaCooldowns.TryAdd(userId, true))
                {
                    await Task.Delay(30 * 1000);
                    Boolean throwaway;
                    karmaCooldowns.TryRemove(userId, out throwaway);
                }

            });
        }

        private static long GetKarma(ulong id)
        {
            using (var uow = DbHandler.UnitOfWork())
            {
                return uow.Karma.GetUserKarma(id);
            }
        }
    }
}