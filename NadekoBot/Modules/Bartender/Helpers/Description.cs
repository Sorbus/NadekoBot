using NadekoBot.Classes;
using NadekoBot.Classes.JSONModels;
using NadekoBot.DataModels.Bartender;
using NadekoBot.Extensions;
using NadekoBot.Modules.Permissions.Classes;
using System;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NadekoBot.Modules.Bartender.Helpers
{
    class Desc
    {
        public static Boolean isBaseline(MorphModel m)
        {
            int[] r = new int[] {m.UpperType, m.LowerType, m.LegType, m.ArmType, m.FaceType, m.EyeType, m.HairType,
                m.EarType, m.TongueType, m.TeethType, m.SkinType, m.SkinOrnaments, m.ArmCovering, m.LegCovering,
                m.TorsoCovering, m.HandType, m.FeetType};
            if (!Array.TrueForAll<int>(r, x => x == 0))
            {
                return false;
            }

            if (m.LegCount != 2 || m.ArmCount != 2 || m.EarCount != 2 || m.EyeCount != 2)
            {
                return false;
            }

            return true;
        }

        public static string getWeight(MorphModel m)
        {
            if (m.Weight != null && m.Gender < 2)
            {
                if (m.Weight > 20 && m.Weight <= 24) { return ""; }
                else if (m.Weight > 19.5 && m.Weight <= 20) { return "slender "; }
                else if (m.Weight > 19 && m.Weight <= 19.5) { return "skinny "; }
                else if (m.Weight > 18.5 && m.Weight <= 19) { return "waifish "; }
                else if (m.Weight > 17.5 && m.Weight <= 18.5) { return "underweight "; }
                else if (m.Weight > 16 && m.Weight <= 17.5) { return "starving "; }
                else if (m.Weight <= 16) { return "skeletal "; }
                else if (m.Weight > 24 && m.Weight <= 25.5) { return "plump "; }
                else if (m.Weight > 25.5 && m.Weight <= 27) { return "chubby "; }
                else if (m.Weight > 27 && m.Weight <= 30) { return "overweight "; }
                else if (m.Weight > 30) { return "obese "; }
            }
            else if (m.Weight != null && m.Gender == 2)
            {
                if (m.Weight > 20.5 && m.Weight <= 23) { return ""; }
                else if (m.Weight > 19.5 && m.Weight <= 20.5) { return "slender "; }
                else if (m.Weight > 18.5 && m.Weight <= 19.5) { return "skinny "; }
                else if (m.Weight > 17.5 && m.Weight <= 18.5) { return "underweight "; }
                else if (m.Weight > 16 && m.Weight <= 17.5) { return "starving "; }
                else if (m.Weight <= 16) { return "skeletal "; }
                else if (m.Weight > 23 && m.Weight <= 24) { return "stout "; }
                else if (m.Weight > 24 && m.Weight <= 25) { return "thickset "; }
                else if (m.Weight > 25 && m.Weight <= 27) { return "chubby "; }
                else if (m.Weight > 27 && m.Weight <= 30) { return "overweight "; }
                else if (m.Weight > 30) { return "obese "; }
            }
            return "";
        }

        public static string getTongue(MorphModel m)
        {
            if (m.TongueLength < 3) { return "a stubby"; }
            else if (m.TongueLength < 4) { return "a short"; }
            else if (m.TongueLength < 5) { return "an average"; }
            else if (m.TongueLength < 8) { return "a long"; }
            else if (m.TongueLength < 12) { return "a very long"; }
            else { return "an obscenely long"; }
        }

        public static string getHair(MorphModel m)
        {
            if (m.HairLength < 2) { return " buzzcut"; }
            else if (m.HairLength < 5) { return "short"; }
            else if (m.HairLength < 9) { return "modest"; }
            else if (m.HairLength < 13) { return "shoulder-length"; }
            else if (m.HairLength < 28) { return "chest-length"; }
            else if (m.HairLength < 35) { return "waist-length"; }
            else if (m.HairLength < 40) { return "butt-length"; }
            else if (m.HairLength < 48) { return "thigh-length"; }
            else if (m.HairLength < 52) { return "knee-length"; }
            else if (m.HairLength < 70) { return "ankle-length"; }
            else if (m.HairLength < 74) { return "floor-length"; }
            else { return " rapunzelesque"; }
        }

        public static string getDominantType(MorphModel m, Dictionary<int, TFMorph> Morphs)
        {
            SortedList<int, int> c = new SortedList<int, int>();

            if (c.ContainsKey(m.UpperType)) { c[m.UpperType] += 1; }
            else { c[m.UpperType] = 1; }
            if (c.ContainsKey(m.LowerType)) { c[m.LowerType] += 1; }
            else { c[m.LowerType] = 1; }
            if (c.ContainsKey(m.ArmType)) { c[m.ArmType] += 1; }
            else { c[m.ArmType] = 1; }
            if (c.ContainsKey(m.LegType)) { c[m.LegType] += 1; }
            else { c[m.LegType] = 1; }
            if (c.ContainsKey(m.FaceType)) { c[m.FaceType] += 1; }
            else { c[m.FaceType] = 1; }
            if (c.ContainsKey(m.EyeType)) { c[m.EyeType] += 1; }
            else { c[m.EyeType] = 1; }
            if (c.ContainsKey(m.HairType)) { c[m.HairType] += 1; }
            else { c[m.HairType] = 1; }
            if (c.ContainsKey(m.EarType)) { c[m.EarType] += 1; }
            else { c[m.EarType] = 1; }
            if (c.ContainsKey(m.TongueType)) { c[m.TongueType] += 1; }
            else { c[m.TongueType] = 1; }
            if (c.ContainsKey(m.HandMod)) { c[m.HandMod] += 1; }
            else { c[m.HandMod] = 1; }
            if (c.ContainsKey(m.FeetMod)) { c[m.FeetMod] += 1; }
            else { c[m.FeetMod] = 1; }
            if (c.ContainsKey(m.HandType)) { c[m.HandType] += 1; }
            else { c[m.HandType] = 1; }
            if (c.ContainsKey(m.FeetType)) { c[m.FeetType] += 1; }
            else { c[m.FeetType] = 1; }
            if (c.ContainsKey(m.WingType)) { c[m.WingType] += 1; }
            else { c[m.WingType] = 1; }
            if (c.ContainsKey(m.TailType)) { c[m.TailType] += 1; }
            else { c[m.TailType] = 1; }

            if (c.ContainsKey(m.SkinType)) { c[m.SkinType] += 1; }
            else { c[m.SkinType] = 1; }

            IOrderedEnumerable<KeyValuePair<int, int>> o = c.OrderByDescending(x => x.Value);

            if (o.First().Value == 15)
            {
                if (o.First().Key == 0)
                {
                    Boolean cosmetic;
                    Boolean counts;
                    if (m.ArmCount <= 2 && m.LegCount <= 2 && m.WingCount == 0 && m.TailCount == 0 && m.EarCount <= 2 &&
                        m.TongueCount <= 1 && m.EyeCount <= 2 && m.TongueLength <= Morphs[0].Max.TongueSize)
                    { counts = false; }
                    else { counts = true; }

                    if (Morphs[0].Color.Hair.Contains(m.HairColor) && Morphs[0].Color.Eye.Contains(m.EyeColor) &&
                        Morphs[0].Color.Lip.Contains(m.LipColor) && Morphs[0].Ornaments.Contains(m.SkinOrnaments) &&
                        Morphs[0].Color.Skin.Contains(m.SkinColor))
                    { cosmetic = true; }
                    else { cosmetic = false; }


                    if (cosmetic && counts) { return "modified human"; }
                    else if (cosmetic) { return "cosmetically modified human"; }
                    else if (counts) { return "modified human"; }
                    else { return "baseline human"; }
                }
                else { return Morphs[o.First().Key].Name; }
            }
            else if (o.First().Value > 12)
            {
                if (o.First().Key == 0) { return "modified human"; }
                return "hybridized " + Morphs[c.First().Key].Name;
            }
            else if (o.First().Value >= 6 && o.ElementAt(1).Value >= 6)
            {
                return Morphs[o.First().Key].Name + "-" + Morphs[o.ElementAt(1).Key].Name + " hybrid";
            }
            else
            {
                return "hybrid";
            }
        }

        public static Boolean vowelFirst(string str)
        {
            if ("aeiou".Contains(str[0]))
            {
                return true;
            }
            return false;
        }

        public static Boolean anyInRange(int[] nums, int upper, int lower)
        {
            foreach (int i in nums)
            {
                if (nums[i] > lower && nums[i] <= upper)
                {
                    return true;
                }
            }
            return false;
        }

        public static Boolean isChanged(MorphModel first, MorphModel second, string item)
        {
            // if (typeof(MorphModel).GetMethod(item).Invoke(first, null).Equals(typeof(MorphModel).GetMethod(item).Invoke(second, null)))
            if (typeof(MorphModel).GetField(item).GetValue(first).Equals(typeof(MorphModel).GetField(item).GetValue(second)))
            { return false; }
            return true;
        } 

        public static string locationName(Dictionary<int, TFMorph> Morphs, MorphModel old, MorphModel now, string name)
        {
            switch(name)
            {
                case "big_upper":
                    return "upper body";
                    break;
                case "big_lower":
                    return "lower body";
                    break;
                case "big_face":
                    return "$headtype$";
                    break;
                case "big_tongue":
                    if (Morphs[old.TongueType] == null || old.TongueCount == 0)
                    { return "mouth"; }
                    else { return "$tonguetype$"; }
                case "big_hair":
                    if (Morphs[old.HairType] == null || old.HairLength == 0)
                    { return "bare head"; }
                    return "$hairtype$";
                    break;
                case "big_horn":
                    if (Morphs[old.HornType] == null || old.HornCount == 0)
                    { return "head"; }
                    return "$horntype$";
                    break;
                case "big_wing":
                    if (Morphs[old.WingType].Body.WingLoc == null)
                    { return null; }
                    else if (old.WingCount == 0 || Morphs[old.WingType].Appendages.Wings == null)
                    { return Morphs[old.UpperType].Body.WingLoc; }
                    else { return "$wingtype$"; }
                case "big_tail":
                    if (Morphs[old.WingType].Body.TailLoc == null)
                    { return null; }
                    else if (old.TailCount == 0 || Morphs[old.TailType].Appendages.Tail == null)
                    { return Morphs[old.LowerType].Body.TailLoc; }
                    else { return "$tailtype$"; }
                case "big_feature":
                    break;
                case "big_color":
                    break;
                case "big_skin":
                    break;
                case "big_mod":
                    break;
                default:
                    // new Exception("unhandled case, pls send help");
                    break;
            }
            return "";
        }

    }
}
