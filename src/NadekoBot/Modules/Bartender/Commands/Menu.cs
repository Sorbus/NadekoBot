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
using NadekoBot.Modules.Bartender.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace NadekoBot.Modules.Bartender
{
    public partial class Bartender
    {
        [Group]
        public class MenuCommands
        {
            private List<Drink> drinks = new List<Drink>();
            private Logger _log;

            public MenuCommands()
            {
                _log = LogManager.GetCurrentClassLogger();
                if (File.Exists("data/bartender/drinks.json"))
                {
                    drinks = JsonConvert.DeserializeObject<List<Drink>>(File.ReadAllText("data/bartender/drinks.json"));
                }
                else
                    _log.Warn("data/bartender/drinks.json is missing. Bartemder is not functional.");
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task MenuCategory(IUserMessage umsg, [Remainder] string category = null)
            {
                Dictionary<String, Drink> drink_cat = drinks.Where(t => t.Category == category.Trim().ToLowerInvariant()).ToDictionary(x => x.Code, y => y);
                var channel = umsg.Channel;

                if (drink_cat.Count > 0)
                {
                    StringBuilder str = new StringBuilder(2000);
                    foreach (KeyValuePair<string, Drink> d in drink_cat)
                    {
                        if (!d.Value.Dragon)
                        {
                            if (d.Value.Name != null)
                            {
                                str.Append(" - " + d.Value.Name + " (" + d.Value.Code + ", " + d.Value.Cost + Gambling.Gambling.CurrencySign + ").\n");
                            }
                            else
                            {
                                str.Append(" - " + d.Value.Code + " (" + d.Value.Cost + Gambling.Gambling.CurrencySign + ").\n");
                            }
                        }
                    }
                    await channel.SendMessageAsync($"Items in the **{category.ToUpper()}** category:\n{str.ToString().CapitalizeFirst()}").ConfigureAwait(false);
                }
                else
                { await channel.SendMessageAsync($"We don't have any drinks in that category, {umsg.Author.Mention}.").ConfigureAwait(false); }
            }
        }
    }
}
