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

namespace NadekoBot.Modules.Bartender
{
    class Bartender : DiscordModule
    {
        public override string Prefix { get; } = NadekoBot.Config.CommandPrefixes.Bartender;

        public Bartender()
        {
            commands.Add(new Menu(this));
            commands.Add(new Inventory(this));
            commands.Add(new State(this));
        }

        public override void Install(ModuleManager manager)
        {
            manager.CreateCommands("", cgb =>
            {
                cgb.AddCheck(PermissionChecker.Instance);

                commands.ForEach(cmd => cmd.Init(cgb));

                cgb.CreateCommand(Prefix + "donate")
                    .Description($"Contribute {NadekoBot.Config.CurrencyName} towards a pool which will be used for drink purchases. | `{Prefix}donate 100`")
                    .Parameter("amount", ParameterType.Required)
                    .Do(async e =>
                    {
                    });

                cgb.CreateCommand(Prefix + "pool")
                    .Description($"Check how many {NadekoBot.Config.CurrencyName} are in the pool. | `{Prefix}pool`")
                    .Do(async e =>
                    {
                    });

            });
        }
    }
}
