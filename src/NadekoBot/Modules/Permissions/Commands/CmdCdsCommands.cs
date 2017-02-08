using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using NadekoBot.Attributes;
using NadekoBot.Extensions;
using NadekoBot.Services;
using NadekoBot.Services.Database.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Permissions
{
    public partial class Permissions
    {

        public class ActiveCooldown
        {
            public string Command { get; set; }
            public ulong UserId { get; set; }
        }

        [Group]
        public class CmdCdsCommands : ModuleBase
        {
            public static ConcurrentDictionary<ulong, ConcurrentHashSet<CommandCooldown>> commandCooldowns { get; }
            public static ConcurrentDictionary<ulong, ConcurrentHashSet<ModuleCooldown>> moduleCooldowns { get; }
            private static ConcurrentDictionary<ulong, ConcurrentHashSet<ActiveCooldown>> activeCooldowns { get; } = new ConcurrentDictionary<ulong, ConcurrentHashSet<ActiveCooldown>>();

            static CmdCdsCommands()
            {
                var configs = NadekoBot.AllGuildConfigs;
                commandCooldowns = new ConcurrentDictionary<ulong, ConcurrentHashSet<CommandCooldown>>(configs.ToDictionary(k => k.GuildId, v => new ConcurrentHashSet<CommandCooldown>(v.CommandCooldowns)));
                moduleCooldowns = new ConcurrentDictionary<ulong, ConcurrentHashSet<ModuleCooldown>>(configs.ToDictionary(k => k.GuildId, v => new ConcurrentHashSet<ModuleCooldown>(v.ModuleCooldowns)));
            }
            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            public async Task CmdCooldown(CommandInfo command, int secs)
            {
                var channel = (ITextChannel)Context.Channel;
                if (secs < 0 || secs > 3600)
                {
                    await channel.SendErrorAsync("Invalid second parameter. (Must be a number between 0 and 3600)").ConfigureAwait(false);
                    return;
                }

                using (var uow = DbHandler.UnitOfWork())
                {
                    var config = uow.GuildConfigs.For(channel.Guild.Id, set => set.Include(gc => gc.CommandCooldowns));
                    var localSet = commandCooldowns.GetOrAdd(channel.Guild.Id, new ConcurrentHashSet<CommandCooldown>());

                    config.CommandCooldowns.RemoveWhere(cc => cc.CommandName == command.Aliases.First().ToLowerInvariant());
                    localSet.RemoveWhere(cc => cc.CommandName == command.Aliases.First().ToLowerInvariant());
                    if (secs != 0)
                    {
                        var cc = new CommandCooldown()
                        {
                            CommandName = command.Aliases.First().ToLowerInvariant(),
                            Seconds = secs,
                        };
                        config.CommandCooldowns.Add(cc);
                        localSet.Add(cc);
                    }
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                if (secs == 0)
                {
                    var activeCds = activeCooldowns.GetOrAdd(channel.Guild.Id, new ConcurrentHashSet<ActiveCooldown>());
                    activeCds.RemoveWhere(ac => ac.Command == command.Aliases.First().ToLowerInvariant());
                    await channel.SendConfirmAsync($"🚮 Command **{command.Aliases.First()}** has no coooldown now and all existing cooldowns have been cleared.")
                                 .ConfigureAwait(false);
                }
                else
                {
                    await channel.SendConfirmAsync($"✅ Command **{command.Aliases.First()}** now has a **{secs} {"seconds".SnPl(secs)}** cooldown.")
                                 .ConfigureAwait(false);
                }
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]

            public async Task ModCooldown(IUserMessage imsg, Module module, int secs)
            {
                var channel = (ITextChannel)imsg.Channel;
                if (secs < 0 || secs > 3600)
                {
                    await channel.SendMessageAsync("Invalid second parameter. (Must be a number between 0 and 3600)").ConfigureAwait(false);
                    return;
                }

                using (var uow = DbHandler.UnitOfWork())
                {
                    var config = uow.GuildConfigs.For(channel.Guild.Id);
                    var localSet = moduleCooldowns.GetOrAdd(channel.Guild.Id, new ConcurrentHashSet<ModuleCooldown>());

                    config.ModuleCooldowns.RemoveWhere(cc => cc.ModuleName == module.Name.ToLowerInvariant());
                    localSet.RemoveWhere(cc => cc.ModuleName == module.Name.ToLowerInvariant());
                    if (secs != 0)
                    {
                        var cc = new ModuleCooldown()
                        {
                            ModuleName = module.Name.ToLowerInvariant(),
                            Seconds = secs,
                            ChannelId = (long)imsg.Channel.Id
                        };
                        config.ModuleCooldowns.Add(cc);
                        localSet.Add(cc);
                    }
                    Console.WriteLine("1");
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                if (secs == 0)
                {
                    var activeCds = activeCooldowns.GetOrAdd(channel.Guild.Id, new ConcurrentHashSet<ActiveCooldown>());
                    activeCds.RemoveWhere(ac => ac.Command == module.Name.ToLowerInvariant());
                    await channel.SendMessageAsync($"Command **{module}** has no coooldown now and all existing cooldowns have been cleared.").ConfigureAwait(false);
                }
                else
                    await channel.SendMessageAsync($"Command **{module}** now has a **{secs} {(secs == 1 ? "second" : "seconds")}** cooldown.").ConfigureAwait(false);
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            public async Task AllCmdCooldowns()
            {
                var channel = (ITextChannel)Context.Channel;
                var localSet = commandCooldowns.GetOrAdd(channel.Guild.Id, new ConcurrentHashSet<CommandCooldown>());

                if (!localSet.Any())
                    await channel.SendConfirmAsync("ℹ️ `No command cooldowns set.`").ConfigureAwait(false);
                else
                    await channel.SendTableAsync("", localSet.Select(c => c.CommandName + ": " + c.Seconds + " secs"), s => $"{s,-30}", 2).ConfigureAwait(false);
            }

            public static bool HasCooldown(CommandInfo cmd, IGuild guild, IUser user)
            {
                if (guild == null)
                    return false;
                var cmdcds = CmdCdsCommands.commandCooldowns.GetOrAdd(guild.Id, new ConcurrentHashSet<CommandCooldown>());
                var modcds = CmdCdsCommands.moduleCooldowns.GetOrAdd(guild.Id, new ConcurrentHashSet<ModuleCooldown>());
                CommandCooldown cdRule;
                ModuleCooldown mdRule;

                if ((cdRule = cmdcds.FirstOrDefault(cc => cc.CommandName == cmd.Aliases.First().ToLowerInvariant())) != null)
                {
                    var activeCdsForGuild = activeCooldowns.GetOrAdd(guild.Id, new ConcurrentHashSet<ActiveCooldown>());
                    if (activeCdsForGuild.FirstOrDefault(ac => ac.UserId == user.Id && ac.Command == cmd.Aliases.First().ToLowerInvariant()) != null)
                    {
                        return true;
                    }
                    else
                    {
                        activeCdsForGuild.Add(new ActiveCooldown()
                        {
                            UserId = user.Id,
                            Command = cmd.Aliases.First().ToLowerInvariant(),
                        });
                        var t = Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Delay(cdRule.Seconds * 1000);
                                activeCdsForGuild.RemoveWhere(ac => ac.Command == cmd.Aliases.First().ToLowerInvariant() && ac.UserId == user.Id);
                            }
                            catch { }
                        });
                    }
                }
                if ((mdRule = modcds.FirstOrDefault(mc => (mc.ModuleName == cmd.Module.Name.ToLowerInvariant() && mc.ChannelId == (long)channel.Id))) != null)
                {
                    var activeCdsForGuild = activeCooldowns.GetOrAdd(guild.Id, new ConcurrentHashSet<ActiveCooldown>());
                    if (activeCdsForGuild.FirstOrDefault(ac => ac.UserId == channel.Id && ac.Command == cmd.Module.Name.ToLowerInvariant()) != null)
                    {
                        return true;
                    }
                    else
                    {
                        activeCdsForGuild.Add(new ActiveCooldown()
                        {
                            UserId = channel.Id,
                            Command = cmd.Module.Name.ToLowerInvariant(),
                        });
                        var t = Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Delay(mdRule.Seconds * 1000);
                                activeCdsForGuild.RemoveWhere(ac => ac.Command == cmd.Module.Name.ToLowerInvariant() && ac.UserId == channel.Id);
                            }
                            catch { }
                        });
                    }
                }
                return false;
            }
        }
    }
}
