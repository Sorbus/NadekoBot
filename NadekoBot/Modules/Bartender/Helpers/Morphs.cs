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

namespace NadekoBot.Modules.Bartender.Helpers
{
    class Morphs
    {
        public static MorphModel buildMorph(long userID, KeyValuePair<int, TFMorph> target_morph)
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
            morph.EyeColor = Bar.getRandItem(target_morph.Value.Color.Eye);
            morph.HairType = target_morph.Key;
            morph.HairColor = Bar.getRandItem(target_morph.Value.Color.Hair);
            morph.EarType = target_morph.Key;

            morph.TongueType = target_morph.Key;
            morph.TeethType = target_morph.Key;

            morph.NeckFeature = target_morph.Key;
            morph.NeckColor = Bar.getRandItem(target_morph.Value.Color.Neck);
            morph.LegFeature = target_morph.Key;
            morph.LegColor = Bar.getRandItem(target_morph.Value.Color.Leg);
            morph.ArmFeature = target_morph.Key;
            morph.ArmColor = Bar.getRandItem(target_morph.Value.Color.Arm);

            morph.SkinType = Bar.getRandItem(target_morph.Value.SkinType);
            morph.SkinColor = Bar.getRandItem(target_morph.Value.Color.Skin);
            morph.SkinOrnaments = Bar.getRandItem(target_morph.Value.Ornaments);
            morph.OrnamentColor = Bar.getRandItem(target_morph.Value.Color.Ornament);

            morph.ArmCovering = Bar.getRandItem(target_morph.Value.SkinCovering);
            morph.TorsoCovering = Bar.getRandItem(target_morph.Value.SkinCovering);
            morph.LegCovering = Bar.getRandItem(target_morph.Value.SkinCovering);
            morph.CoveringColor = Bar.getRandItem(target_morph.Value.Color.Covering);

            morph.HandMod = target_morph.Key;
            morph.FeetMod = target_morph.Key;
            morph.HandType = target_morph.Key;
            morph.FeetType = target_morph.Key;

            morph.WingType = target_morph.Key;
            morph.TailType = target_morph.Key;
            morph.TailColor = Bar.getRandItem(target_morph.Value.Color.Tail);
            morph.WingColor = Bar.getRandItem(target_morph.Value.Color.Wing);

            morph.HornCount = target_morph.Value.Max.Horns;
            morph.HornType = target_morph.Key;
            morph.HornColor = Bar.getRandItem(target_morph.Value.Color.Horn);

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

        public static Tuple<MorphModel, String, String> transformUser(MorphModel original, Drink drink, Discord.User user)
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
                target_morph = _.Morphs.FirstOrDefault(x => x.Value.Code == drink.Transform.Target.ToLowerInvariant());

                if (target_morph.Value == null)
                { throw new Exception("broken JSON"); }

                int key = target_morph.Key;
                TFMorph value = target_morph.Value;

                change_type[0] = true;
                i = 0;

                while (i < tf.ChangeCount)
                {
                    r = _.rng.Next(0, tf.Balance.Length);

                    if (!changes.Contains(tf.Balance[r]))
                    {
                        changes.Add(tf.Balance[r]);
                        i += 1;

                        switch (tf.Balance[r])
                        {
                            case "upper":
                                if (_.rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }

                                if (original.ArmFeature != key)
                                {
                                    changed.ArmFeature = key;
                                    if (!value.Color.Arm.Contains(original.ArmColor))
                                    { changed.ArmColor = Bar.getRandItem(value.Color.Arm); }
                                }
                                else if (original.ArmType != key || original.HandMod != key)
                                {
                                    Boolean done = false;
                                    switch (_.rng.Next(0, 1))
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
                                            if (original.HandMod != key)
                                            {
                                                changed.HandMod = key;
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
                                    switch (_.rng.Next(0, 1))
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
                                if (_.rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }

                                if (original.LegFeature != key)
                                {
                                    changed.LegFeature = key;
                                    if (!value.Color.Leg.Contains(original.LegColor))
                                    { changed.LegColor = Bar.getRandItem(value.Color.Leg); }
                                }
                                else if (original.LegType != key || original.FeetMod != key)
                                {
                                    Boolean done = false;
                                    switch (_.rng.Next(0, 1))
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
                                            if (original.FeetMod != key)
                                            {
                                                changed.FeetMod = key;
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
                                    switch (_.rng.Next(0, 1))
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
                                if (_.rng.Next(0, 100) < value.Transform.Permanence)
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
                                        switch (_.rng.Next(0, 5))
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
                                                    changed.TongueColor = Bar.getRandItem(value.Color.Tongue);
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
                                                    changed.EyeCount += Bar.Move(original.EyeCount, value.Max.Eyes, _.rng);
                                                    search = false;
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;
                            case "tongue":
                                if (_.rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }
                                if (original.TongueType != key)
                                {
                                    changed.TongueType = key;
                                    changed.TongueColor = Bar.getRandItem(value.Color.Tongue);
                                }
                                else if (!value.Color.Tongue.Contains(original.TongueColor))
                                {
                                    changed.TongueColor = Bar.getRandItem(value.Color.Tongue);

                                    changed.TongueLength += Bar.Move(original.TongueLength, value.Max.TongueSize, 3, _.rng);
                                }
                                else if (original.TongueCount != value.Max.TongueCount)
                                {
                                    changed.TongueCount += Bar.Move(original.TongueCount, value.Max.TongueCount, _.rng);

                                    changed.TongueLength += Bar.Move(original.TongueLength, value.Max.TongueSize, 1, _.rng);
                                }
                                else if (original.TongueLength != value.Max.TongueSize)
                                {
                                    changed.TongueLength += Bar.Move(original.TongueLength, value.Max.TongueSize, 3, _.rng);
                                }
                                break;
                            case "hair":
                                if (_.rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }

                                changed.HairType = key;

                                if (!value.Color.Hair.Contains(original.HairColor))
                                { changed.HairColor = Bar.getRandItem(value.Color.Hair); }

                                changed.HairLength += Bar.Move(original.HairLength, value.Max.Hair, 4, _.rng);

                                break;
                            case "horn":
                                if (_.rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }
                                if (original.HornType == key && original.HornType > 0)
                                { }
                                else if (original.HornType != key && original.HornType > 0)
                                { }
                                else if (original.HornCount == 0)
                                {
                                    changed.HornType = key;
                                    changed.HornColor = Bar.getRandItem(value.Color.Horn);
                                    changed.HornCount += 2 - value.Max.Horns % 2;
                                }
                                break;
                            case "wing":
                                if (_.rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }
                                if (original.WingType == key && original.WingCount > 0)
                                { }
                                else if (original.WingType != key && original.WingCount > 0)
                                { }
                                else if (original.WingCount == 0)
                                {
                                    changed.WingType = key;
                                    changed.WingColor = Bar.getRandItem(value.Color.Wing);
                                    changed.WingCount += 2 - value.Max.Horns % 2;
                                }
                                break;
                            case "tail":
                                if (_.rng.Next(0, 100) < value.Transform.Permanence)
                                { break; }
                                break;
                            case "skin":
                                if (_.rng.Next(0, 100) < value.Transform.Permanence)
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
                        r = _.rng.Next(0, tf.Growth.Count);

                        if (!changes.Contains(tf.Growth.ElementAt(r).Key))
                        {
                            changes.Add(tf.Growth.ElementAt(r).Key);
                            i += 1;

                            switch (tf.Growth.ElementAt(r).Key)
                            {
                                case "hair":
                                    changed.HairLength += tf.Growth.ElementAt(r).Value[_.rng.Next(0, tf.Growth.ElementAt(r).Value.Length)];
                                    break;
                                case "tongue":
                                    changed.TongueLength += tf.Growth.ElementAt(r).Value[_.rng.Next(0, tf.Growth.ElementAt(r).Value.Length)];
                                    break;
                                case "wing":
                                    changed.WingSize += tf.Growth.ElementAt(r).Value[_.rng.Next(0, tf.Growth.ElementAt(r).Value.Length)];
                                    if (changed.WingSize >= _.Morphs[original.WingType].Appendages.WingSizes.Length)
                                    { changed.WingSize = _.Morphs[original.WingType].Appendages.WingSizes.Length - 1; }
                                    else if (changed.WingSize < 0)
                                    { changed.WingSize = 0; }
                                    break;
                                case "tail":
                                    changed.TailSize += tf.Growth.ElementAt(r).Value[_.rng.Next(0, tf.Growth.ElementAt(r).Value.Length)];
                                    if (changed.TailSize >= _.Morphs[original.TailSize].Appendages.TailSizes.Length)
                                    { changed.TailSize = _.Morphs[original.TailSize].Appendages.TailSizes.Length - 1; }
                                    else if (changed.TailSize < 0)
                                    { changed.TailSize = 0; }
                                    break;
                                case "horn":
                                    changed.HornSize += tf.Growth.ElementAt(r).Value[_.rng.Next(0, tf.Growth.ElementAt(r).Value.Length)];
                                    if (changed.HornSize >= _.Morphs[original.HornSize].Head.HornSizes.Length)
                                    { changed.HornSize = _.Morphs[original.HornSize].Head.HornSizes.Length - 1; }
                                    else if (changed.HornSize < 0)
                                    { changed.HornSize = 0; }
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
                        r = _.rng.Next(0, tf.ColorTargets.Count);

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
                                case "lip":
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

            Tuple<String, String> change_text = Prose.Gen.GetChange(original, changed, tf, changes);

            DbHandler.Instance.Save<MorphModel>(changed);

            return Tuple.Create(changed, change_text.Item1, change_text.Item2);
        }
    }
}
