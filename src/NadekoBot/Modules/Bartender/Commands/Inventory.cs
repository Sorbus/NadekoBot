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
        public class InventoryCommands
        {
            private Logger _log;

            public InventoryCommands()
            {
                _log = LogManager.GetCurrentClassLogger();
            }
        }
    }
}
