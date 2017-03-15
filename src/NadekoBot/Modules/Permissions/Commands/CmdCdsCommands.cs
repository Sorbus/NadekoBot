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
        public class CmdCdsCommands : NadekoSubmodule
        {
            public static ConcurrentDictionary<ulong, ConcurrentHashSet<CommandCooldown>> CommandCooldowns { get; }
            public static ConcurrentDictionary<ulong, ConcurrentHashSet<ModuleCooldown>> ModuleCooldowns { get; }

            private static ConcurrentDictionary<ulong, ConcurrentHashSet<ActiveCooldown>> activeCooldowns { get; } = new ConcurrentDictionary<ulong, ConcurrentHashSet<ActiveCooldown>>();

            static CmdCdsCommands()
            {
                var configs = NadekoBot.AllGuildConfigs;
                CommandCooldowns = new ConcurrentDictionary<ulong, ConcurrentHashSet<CommandCooldown>>(configs.ToDictionary(k => k.GuildId, v => new ConcurrentHashSet<CommandCooldown>(v.CommandCooldowns)));
                ModuleCooldowns = new ConcurrentDictionary<ulong, ConcurrentHashSet<ModuleCooldown>>(configs.ToDictionary(k => k.GuildId, v => new ConcurrentHashSet<ModuleCooldown>(v.ModuleCooldowns)));
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            public async Task CmdCooldown(CommandInfo command, int secs)
            {
                var channel = (ITextChannel)Context.Channel;
                if (secs < 0 || secs > 3600)
                {
                    await ReplyErrorLocalized("invalid_second_param_between", 0, 3600).ConfigureAwait(false);
                    return;
                }

                using (var uow = DbHandler.UnitOfWork())
                {
                    var config = uow.GuildConfigs.For(channel.Guild.Id, set => set.Include(gc => gc.CommandCooldowns));
                    var localSet = CommandCooldowns.GetOrAdd(channel.Guild.Id, new ConcurrentHashSet<CommandCooldown>());

                    config.CommandCooldowns.RemoveWhere(cc => cc.CommandName == command.Aliases.First().ToLowerInvariant());
                    localSet.RemoveWhere(cc => cc.CommandName == command.Aliases.First().ToLowerInvariant());
                    if (secs != 0)
                    {
                        var cc = new CommandCooldown()
                        {
                            CommandName = command.Aliases.First().ToLowerInvariant(),
                            Seconds = secs
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
                    await ReplyConfirmLocalized("cmdcd_cleared", 
                        Format.Bold(command.Aliases.First())).ConfigureAwait(false);
                }
                else
                {
                    await ReplyConfirmLocalized("cmdcd_add", 
                        Format.Bold(command.Aliases.First()), 
                        Format.Bold(secs.ToString())).ConfigureAwait(false);
                }
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            public async Task ModCooldown(ModuleInfo module, int secs, [Remainder] ITextChannel chnl)
            {
                var channel = (ITextChannel)Context.Channel;
                if (secs < 0 || secs > 3600)
                {
                    await channel.SendMessageAsync("Invalid second parameter. (Must be a number between 0 and 3600)").ConfigureAwait(false);
                    return;
                }

                using (var uow = DbHandler.UnitOfWork())
                {
                    var config = uow.GuildConfigs.For(channel.Guild.Id, set => set.Include(gc => gc.ModuleCooldowns));
                    var localSet = ModuleCooldowns.GetOrAdd(channel.Guild.Id, new ConcurrentHashSet<ModuleCooldown>());

                    config.ModuleCooldowns.RemoveWhere(cc => (cc.ModuleName == module.Name.ToLowerInvariant() && cc.ChannelId == (long)chnl.Id));
                    localSet.RemoveWhere(cc => (cc.ModuleName == module.Name.ToLowerInvariant() && cc.ChannelId == (long)chnl.Id));
                    if (secs != 0)
                    {
                        var cc = new ModuleCooldown()
                        {
                            ModuleName = module.Name.ToLowerInvariant(),
                            Seconds = secs,
                            ChannelId = (long)chnl.Id
                        };
                        config.ModuleCooldowns.Add(cc);
                        localSet.Add(cc);
                    }
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                if (secs == 0)
                {
                    var activeCds = activeCooldowns.GetOrAdd(channel.Guild.Id, new ConcurrentHashSet<ActiveCooldown>());
                    activeCds.RemoveWhere(ac => (ac.Command == module.Name.ToLowerInvariant() && ac.UserId == chnl.Id));
                    await channel.SendMessageAsync($"Command **{module.Name}** has no coooldown now and all existing cooldowns have been cleared.").ConfigureAwait(false);
                }
                else
                    await channel.SendMessageAsync($"Command **{module.Name}** now has a **{secs} {(secs == 1 ? "second" : "seconds")}** cooldown.").ConfigureAwait(false);
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            public async Task AllCmdCooldowns()
            {
                var channel = (ITextChannel)Context.Channel;
                var localSet = CommandCooldowns.GetOrAdd(channel.Guild.Id, new ConcurrentHashSet<CommandCooldown>());

                if (!localSet.Any())
                    await ReplyConfirmLocalized("cmdcd_none").ConfigureAwait(false);
                else
                    await channel.SendTableAsync("", localSet.Select(c => c.CommandName + ": " + c.Seconds + GetText("sec")), s => $"{s,-30}", 2).ConfigureAwait(false);
            }

            public static bool HasCooldown(CommandInfo cmd, IGuild guild, IUser user, IChannel channel)
            {
                if (guild == null)
                    return false;

                var cmdcds = CmdCdsCommands.CommandCooldowns.GetOrAdd(guild.Id, new ConcurrentHashSet<CommandCooldown>());
                var modcds = CmdCdsCommands.ModuleCooldowns.GetOrAdd(guild.Id, new ConcurrentHashSet<ModuleCooldown>());
                
                CommandCooldown cdRule;
                ModuleCooldown mdRule;

                if ((cdRule = cmdcds.FirstOrDefault(cc => cc.CommandName == cmd.Aliases.First().ToLowerInvariant())) != null)
                {
                    var activeCdsForGuild = activeCooldowns.GetOrAdd(guild.Id, new ConcurrentHashSet<ActiveCooldown>());
                    if (activeCdsForGuild.FirstOrDefault(ac => ac.UserId == user.Id && ac.Command == cmd.Aliases.First().ToLowerInvariant()) != null)
                    {
                        return true;
                    }
                    activeCdsForGuild.Add(new ActiveCooldown()
                    {
                        UserId = user.Id,
                        Command = cmd.Aliases.First().ToLowerInvariant(),
                    });
                    var _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(cdRule.Seconds * 1000);
                            activeCdsForGuild.RemoveWhere(ac => ac.Command == cmd.Aliases.First().ToLowerInvariant() && ac.UserId == user.Id);
                        }
                        catch
                        {
                            // ignored
                        }
                    });
                }
                mdRule = modcds.FirstOrDefault(mc => mc.ModuleName == cmd.Module.Name.ToLowerInvariant() && mc.ChannelId == (long)channel.Id);
                if (mdRule == null && cmd.Module.IsSubmodule)
                {
                    mdRule = modcds.FirstOrDefault(mc => mc.ModuleName == cmd.Module.Parent.Name.ToLowerInvariant() && mc.ChannelId == (long)channel.Id);
                    if (mdRule == null && cmd.Module.Parent.IsSubmodule)
                    {
                        mdRule = modcds.FirstOrDefault(mc => mc.ModuleName == cmd.Module.Parent.Parent.Name.ToLowerInvariant() && mc.ChannelId == (long)channel.Id);
                    }
                }

                if (mdRule != null)
                {
                    System.Console.WriteLine("found cooldown");
                    var activeCdsForGuild = activeCooldowns.GetOrAdd(guild.Id, new ConcurrentHashSet<ActiveCooldown>());
                    if (activeCdsForGuild.FirstOrDefault(ac => ac.UserId == channel.Id && ac.Command == mdRule.ModuleName) != null)
                    {
                        return true;
                    }
                    else
                    {
                        activeCdsForGuild.Add(new ActiveCooldown()
                        {
                            UserId = channel.Id,
                            Command = mdRule.ModuleName
                        });
                        var t = Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Delay(mdRule.Seconds * 1000);
                                activeCdsForGuild.RemoveWhere(ac => ac.Command == mdRule.ModuleName && ac.UserId == channel.Id);
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
