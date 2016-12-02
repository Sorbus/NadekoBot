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
    [NadekoModule("Bartender", "?")]
    public partial class Bartender : DiscordModule
    {
        public Bartender(ILocalization loc, CommandService cmds, ShardedDiscordClient client, IGoogleApiService youtube) : base(loc, cmds, client)
        {
        }
    }
}
