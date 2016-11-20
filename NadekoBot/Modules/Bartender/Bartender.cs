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
using System.Text.RegularExpressions;

namespace NadekoBot.Modules.Bartender
{
    class Bartender : DiscordModule
    {
        public override string Prefix { get; } = NadekoBot.Config.CommandPrefixes.Bartender;

        public Random rng = new Random();

        private List<Drink> Drinks = NadekoBot.Config.Drinks;

        private Dictionary<int, TFMorph> Morphs = NadekoBot.Config.Morphs;
        private Dictionary<int, TFColor> Colors = NadekoBot.Config.Colors;
        private Dictionary<int, TFOrnament> Ornament = NadekoBot.Config.Ornament;
        private Dictionary<int, TFSkin> Skin = NadekoBot.Config.Skin;

        private static String[] PronounSelf = new String[3] { "themself", "herself", "himself" };

        private ProseGen Prose = new ProseGen();

        private int getRandItem(int[] a)
        { return a[rng.Next(0, a.Length)]; }

        private MorphModel buildMorph(long userID, KeyValuePair<int, TFMorph> target_morph)
        {
            var db = DbHandler.Instance.GetAllRows<MorphModel>();
            Dictionary<long, MorphModel> morphs = db.Where(t => t.UserId.Equals(userID)).ToDictionary(x => x.UserId, y => y);

            MorphModel morph;
            if (morphs.ContainsKey(userID))
            { morph = morphs[userID]; }
            else
            {
                morph = new MorphModel
                {
                    UserId = userID,
                    Gender = 0,
                    MorphCount = 0,
                    Weight = 22,
                    LipColor = 0,
                    SkinColor = 0,
                    TongueColor = 0,
                    EyeColor = 0,
                    HairColor = 0,
                    TailColor = 0,
                    WingColor = 0,
                    WingSize = 3,
                    TailSize = 3,
                    HornSize = 3
                };
            }

            morph.UpperType = target_morph.Key;
            morph.LowerType = target_morph.Key;

            morph.LegType = target_morph.Key;
            morph.ArmType = target_morph.Key;

            morph.FaceType = target_morph.Key;
            morph.EyeType = target_morph.Key;
            morph.EyeColor = getRandItem(target_morph.Value.Color.Eye);
            morph.HairType = target_morph.Key;
            morph.HairColor = getRandItem(target_morph.Value.Color.Hair);
            morph.EarType = target_morph.Key;

            morph.TongueType = target_morph.Key;
            morph.TeethType = target_morph.Key;

            morph.NeckFeature = target_morph.Key;
            morph.NeckColor = getRandItem(target_morph.Value.Color.Neck);
            morph.LegFeature = target_morph.Key;
            morph.LegColor = getRandItem(target_morph.Value.Color.Leg);
            morph.ArmFeature = target_morph.Key;
            morph.ArmColor = getRandItem(target_morph.Value.Color.Arm);

            morph.SkinType = getRandItem(target_morph.Value.SkinType);
            morph.SkinColor = getRandItem(target_morph.Value.Color.Skin);
            morph.SkinOrnaments = getRandItem(target_morph.Value.Ornaments);
            morph.OrnamentColor = getRandItem(target_morph.Value.Color.Ornament);

            morph.ArmCovering = getRandItem(target_morph.Value.SkinCovering);
            morph.TorsoCovering = getRandItem(target_morph.Value.SkinCovering);
            morph.LegCovering = getRandItem(target_morph.Value.SkinCovering);
            morph.CoveringColor = getRandItem(target_morph.Value.Color.Covering);

            morph.HandModification = target_morph.Key;
            morph.FeetModification = target_morph.Key;
            morph.HandType = target_morph.Key;
            morph.FeetType = target_morph.Key;

            morph.WingType = target_morph.Key;
            morph.TailType = target_morph.Key;
            morph.TailColor = getRandItem(target_morph.Value.Color.Tail);
            morph.WingColor = getRandItem(target_morph.Value.Color.Wing);

            morph.HornCount = target_morph.Value.Max.Horns;
            morph.HornType = target_morph.Key;
            morph.HornColor = getRandItem(target_morph.Value.Color.Horn);

            morph.LegCount = target_morph.Value.Max.Legs;
            morph.ArmCount = target_morph.Value.Max.Arms;
            morph.WingCount = target_morph.Value.Max.Wings;
            morph.TailCount = target_morph.Value.Max.Tails;
            morph.HairLength = target_morph.Value.Max.Hair;
            morph.EarCount = target_morph.Value.Max.Ears;
            morph.TongueLength = target_morph.Value.Max.TongueSize;
            morph.TongueCount = target_morph.Value.Max.TongueCount;
            morph.EyeCount = target_morph.Value.Max.Eyes;

            morph.MorphCount += 1;

            return morph;
        }

        private Tuple<MorphModel, String, String> transformUser(MorphModel original, Drink drink, Discord.User user)
        {
            TFDetails tf = drink.Transform;
            MorphModel changed = original.Copy();
            changed.MorphCount += 1;
            Boolean[] change_type = new bool[3] { false, false, false };

            List<string> changes = new List<string>();

            int i;
            int r;

            if (tf.ChangeCount > 0 && tf.Target != null)
            {
                KeyValuePair<int, TFMorph> target_morph;
                target_morph = Morphs.FirstOrDefault(x => x.Value.Code == drink.Transform.Target.ToLowerInvariant());

                if (target_morph.Value == null)
                { throw new Exception("broken JSON"); }

                int key = target_morph.Key;
                TFMorph value = target_morph.Value;

                change_type[0] = true;
                i = 0;

                while (i < tf.ChangeCount)
                {
                    r = rng.Next(0, tf.Balance.Length);

                    if (!changes.Contains(tf.Balance[r]))
                    {
                        changes.Add(tf.Balance[r]);
                        i += 1;

                        switch (tf.Balance[r])
                        {
                            case "upper":
                                if (rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }
                                
                                if (original.ArmFeature != key )
                                {
                                    changed.ArmFeature = key;
                                    if (!value.Color.Arm.Contains(original.ArmColor))
                                    { changed.ArmColor = getRandItem(value.Color.Arm); }
                                }
                                else if (original.ArmType != key || original.HandModification != key)
                                {
                                    Boolean done = false;
                                    switch(rng.Next(0,1))
                                    {
                                        case 0:
                                            if (original.ArmType != key)
                                            {
                                                changed.ArmType = key;
                                                done = true;
                                            }
                                            if (!done)
                                            { done = true; goto case 1; }
                                            break;
                                        case 1:
                                            if (original.HandModification != key)
                                            {
                                                changed.HandModification = key;
                                                done = true;
                                            }
                                            if (!done)
                                            { done = true; goto case 0; }
                                            break;
                                    }
                                }
                                else if (original.ArmCount != value.Max.Arms || original.HandType != key)
                                {
                                    Boolean done = false;
                                    switch (rng.Next(0, 1))
                                    {
                                        case 0:
                                            if (original.ArmCount != value.Max.Arms)
                                            {
                                                changed.ArmType = key;
                                                done = true;
                                            }
                                            if (!done)
                                            { done = true; goto case 1; }
                                            break;
                                        case 1:
                                            if (original.HandType != key)
                                            {
                                                changed.HandType = key;
                                                done = true;
                                            }
                                            if (!done)
                                            { done = true; goto case 0; }
                                            break;
                                    }
                                }
                                else
                                { changed.UpperType = key; }

                                break;
                            case "lower":
                                if (rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }

                                if (original.LegFeature != key)
                                {
                                    changed.LegFeature = key;
                                    if (!value.Color.Leg.Contains(original.LegColor))
                                    { changed.LegColor = getRandItem(value.Color.Leg); }
                                }
                                else if (original.LegType != key || original.FeetModification != key)
                                {
                                    Boolean done = false;
                                    switch (rng.Next(0, 1))
                                    {
                                        case 0:
                                            if (original.LegType != key)
                                            {
                                                changed.LegType = key;
                                                done = true;
                                            }
                                            if (!done)
                                            { done = true; goto case 1; }
                                            break;
                                        case 1:
                                            if (original.FeetModification != key)
                                            {
                                                changed.FeetModification = key;
                                                done = true;
                                            }
                                            if (!done)
                                            { done = true; goto case 0; }
                                            break;
                                    }
                                }
                                else if (original.LegCount != value.Max.Legs || original.FeetType != key)
                                {
                                    Boolean done = false;
                                    switch (rng.Next(0, 1))
                                    {
                                        case 0:
                                            if (original.LegCount != value.Max.Legs)
                                            {
                                                changed.LegType = key;
                                                done = true;
                                            }
                                            if (!done)
                                            { done = true; goto case 1; }
                                            break;
                                        case 1:
                                            if (original.FeetType != key)
                                            {
                                                changed.FeetType = key;
                                                done = true;
                                            }
                                            if (!done)
                                            { done = true; goto case 0; }
                                            break;
                                    }
                                }
                                else
                                { changed.UpperType = key; }

                                break;
                            case "face":
                                if (rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }
                                if (original.EyeType == key && original.EarType == key &&
                                    original.TongueType == key && original.TeethType == key
                                    && original.EyeCount == key)
                                {
                                    changed.FaceType = key;
                                }
                                else
                                {
                                    Boolean search = true;
                                    while (search)
                                    {
                                        switch (rng.Next(0, 5))
                                        {
                                            case 0:
                                                if (original.EyeType != key)
                                                {
                                                    changed.EyeType = key;
                                                    search = false;
                                                }
                                                break;
                                            case 1:
                                                if (original.EarType != key)
                                                {
                                                    changed.EarType = key;
                                                    search = false;
                                                }
                                                break;
                                            case 2:
                                                if (original.TongueType != key)
                                                {
                                                    changed.TongueType = key;
                                                    changed.TongueColor = getRandItem(value.Color.Tongue);
                                                    search = false;
                                                }
                                                break;
                                            case 3:
                                                if (original.TeethType != key)
                                                {
                                                    changed.TongueType = key;
                                                    search = false;
                                                }
                                                break;
                                            case 4:
                                                if (original.EyeCount != value.Max.Eyes)
                                                {
                                                    changed.EyeCount += BarHelp.Move(original.EyeCount, value.Max.Eyes, rng);
                                                    search = false;
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;
                            case "tongue":
                                if (rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }
                                if (original.TongueType != key)
                                {
                                    changed.TongueType = key;
                                    changed.TongueColor = getRandItem(value.Color.Tongue);
                                }
                                else if (!value.Color.Tongue.Contains(original.TongueColor))
                                {
                                    changed.TongueColor = getRandItem(value.Color.Tongue);

                                    changed.TongueLength += BarHelp.Move(original.TongueLength, value.Max.TongueSize, 3, rng);
                                }
                                else if (original.TongueCount != value.Max.TongueCount)
                                {
                                    changed.TongueCount += BarHelp.Move(original.TongueCount, value.Max.TongueCount, rng);

                                    changed.TongueLength += BarHelp.Move(original.TongueLength, value.Max.TongueSize, 1, rng);
                                }
                                else if (original.TongueLength != value.Max.TongueSize)
                                {
                                    changed.TongueLength += BarHelp.Move(original.TongueLength, value.Max.TongueSize, 3, rng);
                                }
                                break;
                            case "hair":
                                if (rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }

                                changed.HairType = key;

                                if (!value.Color.Hair.Contains(original.HairColor))
                                { changed.HairColor = getRandItem(value.Color.Hair); }

                                changed.HairLength += BarHelp.Move(original.HairLength, value.Max.Hair, 4, rng);

                                break;
                            case "horn":
                                if (rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }
                                if (original.HornType == key && original.HornType > 0)
                                { }
                                else if (original.HornType != key && original.HornType > 0)
                                { }
                                else if (original.HornCount == 0)
                                {
                                    changed.HornType = key;
                                    changed.HornColor = getRandItem(value.Color.Horn);
                                    changed.HornCount += 2 - value.Max.Horns % 2;
                                }
                                break;
                            case "wing":
                                if (rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }
                                if (original.WingType == key && original.WingCount > 0)
                                { }
                                else if (original.WingType != key && original.WingCount > 0)
                                { }
                                else if (original.WingCount == 0)
                                {
                                    changed.WingType = key;
                                    changed.WingColor = getRandItem(value.Color.Wing);
                                    changed.WingCount += 2 - value.Max.Horns % 2;
                                }
                                break;
                            case "tail":
                                if (rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }
                                break;
                            case "skin":
                                if (rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }
                                break;
                            default:
                                throw new Exception("null value in balance");
                        }
                    }
                }
            }
            else
            {
                if (tf.GrowCount > 0 && tf.Growth.Count >= tf.GrowCount)
                {
                    change_type[1] = true;
                    i = 0;

                    while (i < tf.GrowCount)
                    {
                        r = rng.Next(0, tf.Growth.Count);

                        if (!changes.Contains(tf.Growth.ElementAt(r).Key))
                        {
                            changes.Add(tf.Growth.ElementAt(r).Key);
                            i += 1;

                            switch (tf.Growth.ElementAt(r).Key)
                            {
                                case "hair":
                                    break;
                                case "tongue":
                                    break;
                                case "wing":
                                    break;
                                case "tail":
                                    break;
                                case "horn":
                                    break;
                                case "ear":
                                    break;
                            }
                        }
                    }
                }

                if (tf.ColorCount > 0 && tf.ColorTargets.Count >= tf.ColorCount)
                {
                    change_type[2] = true;
                    i = 0;

                    while (i < tf.ColorCount)
                    {
                        r = rng.Next(0, tf.ColorTargets.Count);

                        if (!changes.Contains(tf.ColorTargets[r]))
                        {
                            changes.Add(tf.ColorTargets[r]);
                            i += 1;

                            switch (tf.ColorTargets[r])
                            {
                                case "wing":
                                    break;
                                case "tail":
                                    break;
                                case "eye":
                                    break;
                                case "horn":
                                    break;
                                case "skin":
                                    break;
                                case "ornament":
                                    break;
                                case "arm":
                                    break;
                                case "leg":
                                    break;
                                case "neck":
                                    break;
                                case "covering":
                                    break;
                                case "tongue":
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            if (changed.LegCount < 0) { changed.LegCount = 0; }
            if (changed.ArmCount < 0) { changed.ArmCount = 0; }
            if (changed.WingCount < 0) { changed.WingCount = 0; }
            if (changed.TailCount < 0) { changed.TailCount = 0; }
            if (changed.HairLength < 0) { changed.HairLength = 0; }
            if (changed.EarCount < 0) { changed.EarCount = 0; }
            if (changed.TongueCount < 0) { changed.TongueCount = 0; }
            if (changed.TongueLength < 0) { changed.TongueLength = 0; }
            if (changed.EyeCount < 0) { changed.EyeCount = 0; }
            if (changed.HornCount < 0) { changed.HornCount = 0; }

            Tuple<String, String> change_text = Prose.GetChange(original, changed, tf, changes);

            DbHandler.Instance.Save<MorphModel>(changed);

            return Tuple.Create(changed, change_text.Item1, change_text.Item2);
        }

        public override void Install(ModuleManager manager)
        {
            manager.CreateCommands("", cgb =>
            {
                cgb.AddCheck(PermissionChecker.Instance);

                commands.ForEach(cmd => cmd.Init(cgb));

                cgb.CreateCommand(Prefix + "menu")
                    .Description($"List items in one of the drink menu's categories. | `{Prefix}menu \"beer\"`")
                    .Parameter("category", ParameterType.Required)
                    .Do(async e =>
                    {
                        try
                        {
                            Dictionary<String, Drink> drink_cat = Drinks.Where(t => t.Category == e.GetArg("category".ToLowerInvariant())).ToDictionary(x => x.Code, y => y);

                            if (drink_cat.Count > 0)
                            {
                                StringBuilder str = new StringBuilder(2000);
                                foreach (KeyValuePair<string, Drink> d in drink_cat)
                                {
                                    if (!d.Value.Dragon)
                                    {
                                        if (d.Value.Name != null)
                                        {
                                            str.Append(" - " + d.Value.Name + " (" + d.Value.Code + ", " + d.Value.Cost + NadekoBot.Config.CurrencySign + ").\n");
                                        }
                                        else
                                        {
                                            str.Append(" - " + d.Value.Code + " (" + d.Value.Cost + NadekoBot.Config.CurrencySign + ").\n");
                                        }
                                    }
                                }
                                await e.Channel.SendMessage($"Items in the **{e.GetArg("category").ToUpper()}** category:\n{str.ToString().CapitalizeFirst()}").ConfigureAwait(false);
                            }
                            else
                            { await e.Channel.SendMessage($"We don't have any drinks in that category, {e.User.Mention}.").ConfigureAwait(false); }
                        }
                        catch (Exception ex)
                        { Console.WriteLine(ex); }
                    });

                cgb.CreateCommand(Prefix + "info")
                    .Description($"Get additional information on a specific drink. | `{Prefix}info \"mead\"`")
                    .Parameter("drink", ParameterType.Required)
                    .Do(async e =>
                    {
                        Drink drink;

                        drink = Drinks.Find(t => t.Code.Equals(e.GetArg("drink").ToLowerInvariant()));

                        var db = DbHandler.Instance.GetAllRows<MorphModel>();
                        Dictionary<long, MorphModel> morphs = db.Where(t => t.UserId.Equals((long)e.User.Id)).ToDictionary(x => x.UserId, y => y);

                        if (drink == null)
                        {
                            await e.Channel.SendMessage($"Sorry, {e.User.Mention}, that's not on the menu.").ConfigureAwait(false);
                            return;
                        }

                        if (drink.Dragon)
                        {
                            await e.Message.Delete().ConfigureAwait(false);
                            await e.Channel.SendMessage($"Trying to order off-menu, dear?").ConfigureAwait(false);
                            return;
                        }

                        StringBuilder str = new StringBuilder(2000);
                        if (drink.Name != null)
                        {
                            str.Append(drink.Name + " (" + drink.Code + ", " + drink.Cost + NadekoBot.Config.CurrencySign + ").\n");
                        }
                        else
                        {
                            str.Append(drink.Code + " (" + drink.Cost + NadekoBot.Config.CurrencySign + ").\n");
                        }

                        str.Append(drink.Description);

                        await e.Channel.SendMessage(str.ToString().CapitalizeFirst()).ConfigureAwait(false);
                    });

                cgb.CreateCommand(Prefix + "buy")
                    .Description($"Purchase a drink for yourself. | `{Prefix}buy \"sex on the beach\"`")
                    .Parameter("drink", ParameterType.Required)
                    .Do(async e =>
                    {
                    Drink drink;

                    drink = Drinks.Find(t => t.Code.Equals(e.GetArg("drink").ToLowerInvariant()));

                    var db = DbHandler.Instance.GetAllRows<MorphModel>();
                    Dictionary<long, MorphModel> morphs = db.Where(t => t.UserId.Equals((long)e.User.Id)).ToDictionary(x => x.UserId, y => y);

                    if (drink == null)
                    {
                        await e.Channel.SendMessage($"Sorry, {e.User.Mention}, that's not on the menu.").ConfigureAwait(false);
                        return;
                    }

                    if (drink.Dragon)
                    {
                        await e.Message.Delete().ConfigureAwait(false);
                    }

                    //Payment~
                    var amount = drink.Cost;
                    var pts = DbHandler.Instance.GetStateByUserId((long)e.User.Id)?.Value ?? 0;
                    if (pts < amount)
                    {
                        await e.Channel.SendMessage($"{e.User.Mention} you don't have enough {NadekoBot.Config.CurrencyName}s! \nYou still need {amount - pts} {NadekoBot.Config.CurrencySign} to be able to do this!").ConfigureAwait(false);
                        return;
                    }
                    await FlowersHandler.RemoveFlowers(e.User, $"bought a {drink.Code}", amount).ConfigureAwait(false);

                    //await e.User.SendMessage($"{drink.Flavor_2nd}").ConfigureAwait(false);

                    MorphModel morph;

                    if (morphs.ContainsKey((long)e.User.Id))
                    { morph = morphs[(long)e.User.Id]; }
                    else
                    { morph = buildMorph((long)e.User.Id, Morphs.FirstOrDefault(x => x.Value.Code == "human")); }

                    StringBuilder str = new StringBuilder(2000);

                    if (drink.Name != null)
                    { str.Append($"{e.User.Mention} bought {PronounSelf[morph.Gender]} {(DescHelp.vowelFirst(drink.Name) ? "an" : "a")} {drink.Name}.\n\n{drink.Flavor_3rd}\n\n"); }
                    else
                    { str.Append($"{e.User.Mention} bought {PronounSelf[morph.Gender]} {(DescHelp.vowelFirst(drink.Code) ? "an" : "a")} {drink.Code}.\n\n{drink.Flavor_3rd}\n\n"); }


                    if (drink.Transformative)
                    {
                        try
                        {
                            Tuple<MorphModel, String, String> output = transformUser(morph, drink, e.User);

                            if (morphs.ContainsKey((long)e.User.Id))
                            { DbHandler.Instance.Delete<MorphModel>(morphs[(long)e.User.Id].Id.Value); }

                            DbHandler.Instance.Save<MorphModel>(output.Item1);

                            str.Append(output.Item3);

                                var swapper = new Dictionary<string, string>(
                                    StringComparer.OrdinalIgnoreCase) {
                                    {"$mention$", e.User.Mention },
                                    {"$name$", e.User.Name },

                                    {"$subjective$", ProseGen.PronounSubjective[morph.Gender]},
                                    {"$objective$", ProseGen.PronounObjective[morph.Gender]},
                                    {"$possessive$", ProseGen.PronounPossessive[morph.Gender]},
                                    {"$are$", ProseGen.Are[morph.Gender] },
                                    {"$has$", ProseGen.Has[morph.Gender]},
                                };

                                await e.Channel.SendMessage(
                                    swapper.Aggregate(str.ToString(), (current, value) => current.Replace(value.Key, value.Value)
                                    )).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            { Console.WriteLine(ex); }
                        }


                    });

                cgb.CreateCommand(Prefix + "buy")
                    .Description($"Purchase a drink for another person. | `{Prefix}buy \"sex on the beach\" @somegal`")
                    .Parameter("drink", ParameterType.Required)
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

                        Drink drink;

                        drink = Drinks.Find(t => t.Code.Equals(e.GetArg("drink").ToLowerInvariant()));

                        if (drink == null)
                        {
                            await e.Channel.SendMessage($"Sorry, {e.User.Mention}, that's not on the menu.").ConfigureAwait(false);
                            return;
                        }

                        if (drink.Dragon)
                        {
                            await e.Message.Delete().ConfigureAwait(false);
                        }

                        if (drink.Transformative == true)
                        {
                            await e.Channel.SendMessage($"Sorry, {e.User.Mention}, but you can't buy that for someone else.").ConfigureAwait(false);
                            return;
                        }

                        //Payment~
                        var amount = drink.Cost;
                        var pts = DbHandler.Instance.GetStateByUserId((long)e.User.Id)?.Value ?? 0;
                        if (pts < amount)
                        {
                            await e.Channel.SendMessage($"{e.User.Mention} you don't have enough {NadekoBot.Config.CurrencyName}s! \nYou still need {amount - pts} {NadekoBot.Config.CurrencySign} to be able to do this!").ConfigureAwait(false);
                            return;
                        }
                        await FlowersHandler.RemoveFlowers(e.User, $"bought {target.Name} a {drink.Code}", amount).ConfigureAwait(false);

                        if (drink.Name != null)
                        { await e.Channel.SendMessage($"{e.User.Mention} sent {target.Mention} {(DescHelp.vowelFirst(drink.Name) ? "an" : "a")} {drink.Name}.").ConfigureAwait(false); }
                        else
                        { await e.Channel.SendMessage($"{e.User.Mention} sent {target.Mention} {(DescHelp.vowelFirst(drink.Code) ? "an" : "a")} {drink.Code}.").ConfigureAwait(false); }
                    });

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

                                await e.Channel.SendMessage(Prose.GetState(morph, target)).ConfigureAwait(false);
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
                                MorphModel morph = buildMorph((long)e.User.Id, Morphs.FirstOrDefault(x => x.Value.Code == "human"));

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

                        target_morph = Morphs.FirstOrDefault(x => x.Value.Code == e.GetArg("morph_type").ToLowerInvariant());

                        if (target_morph.Value == null)
                        {
                            await e.Channel.SendMessage($"That's not a valid morph, {e.User.Mention}.").ConfigureAwait(false);
                            return;
                        }

                        try
                        {
                            DbHandler.Instance.Save(buildMorph((long)target.Id, target_morph));
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
            });
        }
    }
}
