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
    class State : DiscordCommand
    {
        public State(DiscordModule module) : base(module)
        {
        }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Prefix + "state")
                    .Description($"Get information about a user's current state. | `{Prefix}state @somegal`")
                    .Parameter("target", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        var targetStr = e.GetArg("target")?.Trim();
                        if (string.IsNullOrWhiteSpace(targetStr))
                            return;
                        var target = e.Server.FindUsers(targetStr).FirstOrDefault();
                        if (target == null)
                        {
                            await e.Channel.SendMessage("No such person.").ConfigureAwait(false);
                            return;
                        }

                        try
                        {
                            var db = DbHandler.Instance.GetAllRows<MorphModel>();
                            Dictionary<long, MorphModel> morphs = db.Where(t => t.UserId.Equals((long)target.Id)).ToDictionary(x => x.UserId, y => y);

                            await e.Channel.SendIsTyping();

                            if (morphs.ContainsKey((long)target.Id))
                            {
                                MorphModel morph = morphs[(long)target.Id];

                                await e.Channel.SendMessage(Gen.GetState(morph, target)).ConfigureAwait(false);
                            }
                            else
                            {
                                await e.Channel.SendMessage($"{target.Mention} is a baseline human.").ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    });

            cgb.CreateCommand(Prefix + "set")
                .Description($"Set your gender. | `{Prefix}set female`")
                .Parameter("gender", ParameterType.Required)
                .Do(async e =>
                {
                    int gender_marker;
                    if (e.GetArg("gender").ToLowerInvariant() == "female")
                    {
                        gender_marker = 1;
                    }
                    else if (e.GetArg("gender").ToLowerInvariant() == "male")
                    {
                        gender_marker = 2;
                    }
                    else if (e.GetArg("gender").ToLowerInvariant() == "neutral")
                    {
                        gender_marker = 0;
                    }
                    else
                    {
                        await e.Channel.SendMessage($"Sorry, {e.User.Mention}, we don't currently support that gender. Supported markers are male, female, and neutral.").ConfigureAwait(false);
                        return;
                    }

                    try
                    {
                        var db = DbHandler.Instance.GetAllRows<MorphModel>();
                        Dictionary<long, MorphModel> morphs = db.Where(t => t.UserId.Equals((long)e.User.Id)).ToDictionary(x => x.UserId, y => y);

                        if (morphs.ContainsKey((long)e.User.Id))
                        {
                            MorphModel morph = morphs[(long)e.User.Id];
                            if (morph.Gender == gender_marker)
                            {
                                await e.Channel.SendMessage($"Your gender is already {e.GetArg("gender").ToUpperInvariant()}, {e.User.Mention}.").ConfigureAwait(false);
                            }
                            else
                            {
                                morph.Gender = gender_marker;

                                DbHandler.Instance.Save(morph);

                                await e.Channel.SendMessage($"Set your gender to {e.GetArg("gender").ToUpperInvariant()}, {e.User.Mention}.").ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            MorphModel morph = Helpers.Morphs.buildMorph((long)e.User.Id, _.Morphs.FirstOrDefault(x => x.Value.Code == "human"));

                            morph.Gender = gender_marker;

                            DbHandler.Instance.Save(morph);

                            await e.Channel.SendMessage($"Set your gender to {e.GetArg("gender").ToUpperInvariant()}, {e.User.Mention}.").ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });

            cgb.CreateCommand(Prefix + "change")
                .Description($"Change a user to a specific morph. **Bot Owner Only!** | `{Prefix}set spider @somegal`")
                .Parameter("morph_type", ParameterType.Required)
                .Parameter("target", ParameterType.Unparsed)
                .AddCheck(SimpleCheckers.OwnerOnly())
                .Do(async e =>
                {
                    var targetStr = e.GetArg("target")?.Trim();
                    if (string.IsNullOrWhiteSpace(targetStr))
                        return;
                    var target = e.Server.FindUsers(targetStr).FirstOrDefault();
                    if (target == null)
                    {
                        await e.Channel.SendMessage("No such person.").ConfigureAwait(false);
                        return;
                    }

                    KeyValuePair<int, TFMorph> target_morph;

                    target_morph = _.Morphs.FirstOrDefault(x => x.Value.Code == e.GetArg("morph_type").ToLowerInvariant());

                    if (target_morph.Value == null)
                    {
                        await e.Channel.SendMessage($"That's not a valid morph, {e.User.Mention}.").ConfigureAwait(false);
                        return;
                    }

                    try
                    {
                        DbHandler.Instance.Save(Helpers.Morphs.buildMorph((long)target.Id, target_morph));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    await e.Channel.SendMessage($"All will be well when you wake, {target.Mention}. Relax and embrace the void.").ConfigureAwait(false);
                });

            cgb.CreateCommand(Prefix + "delete")
                .Description($"Delete a user's morph. **Bot Owner Only!** | `{Prefix}delete @somegal`")
                .Parameter("target", ParameterType.Unparsed)
                .AddCheck(SimpleCheckers.OwnerOnly())
                .Do(async e =>
                {
                    try
                    {
                        var targetStr = e.GetArg("target")?.Trim();
                        if (string.IsNullOrWhiteSpace(targetStr))
                            return;
                        var target = e.Server.FindUsers(targetStr).FirstOrDefault();
                        if (target == null)
                        {
                            await e.Channel.SendMessage("No such person.").ConfigureAwait(false);
                            return;
                        }

                        var db = DbHandler.Instance.GetAllRows<MorphModel>();
                        Dictionary<long, MorphModel> morphs = db.Where(t => t.UserId.Equals((long)target.Id)).ToDictionary(x => x.UserId, y => y);

                        if (morphs.ContainsKey((long)target.Id))
                        {
                            DbHandler.Instance.Delete<MorphModel>(morphs[(long)target.Id].Id.Value);

                            await e.Channel.SendMessage($"**{target.Name}** has been purged from the annals of history.").ConfigureAwait(false);
                        }
                        else
                        {
                            await e.Channel.SendMessage($"**{target.Name}** is unchanged and so remains.").ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex) { Console.WriteLine(ex); }
                });
        }
    }
}
