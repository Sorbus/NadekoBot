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

        // private ConcurrentDictionary<ulong, UserMorph> Morphs = new ConcurrentDictionary<ulong, UserMorph>();

        private List<Drink> Drinks = NadekoBot.Config.Drinks;

        private Dictionary<int, TFMorph> Morphs = NadekoBot.Config.Morphs;
        private Dictionary<int, TFColor> Colors = NadekoBot.Config.Colors;
        private Dictionary<int, TFOrnament> Ornament = NadekoBot.Config.Ornament;
        private Dictionary<int, TFSkin> Skin = NadekoBot.Config.Skin;

        private static String[] PronounObjective = new String[3] { "their", "her", "his" };
        private static String[] PronounHas = new String[3] { "have", "has", "has" };
        private static String[] Pronoun = new String[3] { "they", "she", "he" };
        private static String[] Are = new String[3] { "are", "is", "is" };
        private static String[] Was = new String[3] { "were", "was", "was" };
        private static String[] PronounSelf = new String[3] { "themself", "herself", "himself" };
        private static readonly Regex re = new Regex(@"\$(\w+)\$", RegexOptions.Compiled);

        // from https://stackoverflow.com/a/2730393
        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }
            return words;
        }

        private int getRandItem(int[] a)
        { return a[rng.Next(0, a.Length)]; }

        private Boolean isBaseline(UserMorph m)
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

        private string getWeight(UserMorph m)
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

        private UserMorph buildMorph(long userID, KeyValuePair<int, TFMorph> target_morph)
        {
            var db = DbHandler.Instance.GetAllRows<UserMorph>();
            Dictionary<long, UserMorph> morphs = db.Where(t => t.UserId.Equals(userID)).ToDictionary(x => x.UserId, y => y);

            UserMorph morph;
            if (morphs.ContainsKey(userID))
            { morph = morphs[userID]; }
            else
            {
                morph = new UserMorph
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

        private string getTongue(UserMorph m)
        {
            if (m.TongueLength < 3) { return "a stubby"; }
            else if (m.TongueLength < 4) { return "a short"; }
            else if (m.TongueLength < 5) { return "an average"; }
            else if (m.TongueLength < 8) { return "a long"; }
            else if (m.TongueLength < 12) { return "a very long"; }
            else { return "an obscenely long"; }
        }

        private string getHair(UserMorph m)
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

        private string getDominantType(UserMorph m)
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
            if (c.ContainsKey(m.HandModification)) { c[m.HandModification] += 1; }
            else { c[m.HandModification] = 1; }
            if (c.ContainsKey(m.FeetModification)) { c[m.FeetModification] += 1; }
            else { c[m.FeetModification] = 1; }
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
                return Morphs[o.First().Key].Name + "-" + Morphs[o.ElementAt(1).Value].Name + " hybrid";
            }
            else
            {
                return "hybrid";
            }
        }

        private Boolean vowelFirst(string str)
        {
            if ("aeiou".Contains(str[0]))
            {
                return true;
            }
            return false;
        }

        private Boolean anyInRange(int[] nums, int upper, int lower)
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

        private Tuple<UserMorph, String, String> transformUser(UserMorph original, Drink drink, Discord.User user)
        {
            String str_3rd = "";
            String str_2nd = "";
            TFDetails tf = drink.Transform;
            UserMorph changed = original.Copy();
            changed.MorphCount += 1;
            Boolean[] change_type = new bool[3] { false, false, false };

            List<string> changes = new List<string>();

            var swapper = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase) {
                    {"$mention$", user.Mention },
                    {"$pronoun$", Pronoun[original.Gender]},
                    {"$has$", PronounHas[original.Gender]},
                    {"$objective$", PronounObjective[original.Gender]},
                    {"$are$", Are[original.Gender] },
                };
            int i;
            int r;

            if (tf.ChangeCount > 0 && tf.Target != null)
            {
                KeyValuePair<int, TFMorph> target_morph;
                target_morph = Morphs.FirstOrDefault(x => x.Value.Code == drink.Transform.Target.ToLowerInvariant());

                if (target_morph.Value == null)
                { throw new Exception("broken JSON"); }

                change_type[0] = true;
                i = 0;

                while (i < tf.ChangeCount)
                {
                    r = rng.Next(0, tf.Balance.Length);

                    if (!changes.Contains("big_" + tf.Balance[r]))
                    {
                        changes.Add("big_" + tf.Balance[r]);
                        i += 1;

                        switch (tf.Balance[r])
                        {
                            case "upper":
                                if (rng.Next(0,100) > target_morph.Value.Transform.Permanence)
                                { break; }
                                if (original.ArmType == target_morph.Key)
                                {
                                    if (original.ArmCount >= target_morph.Value.Max.Arms)
                                    {
                                        if (original.UpperType != target_morph.Key)
                                        { original.UpperType = target_morph.Key; }
                                        else if (original.ArmCount > target_morph.Value.Max.Arms)
                                        { changed.ArmCount -= 2 - target_morph.Value.Max.Arms % 2; }
                                    }
                                    else
                                    { changed.ArmCount += 2 - target_morph.Value.Max.Arms % 2; }
                                }
                                else
                                { changed.ArmType = target_morph.Key; }
                                break;
                            case "lower":
                                if (rng.Next(0, 100) > target_morph.Value.Transform.Permanence)
                                { break; }
                                if (original.LegType == target_morph.Key)
                                {
                                    if (original.LegCount >= target_morph.Value.Max.Legs)
                                    {
                                        if (original.LowerType != target_morph.Key)
                                        { original.LowerType = target_morph.Key; }
                                        else if (original.LegCount > target_morph.Value.Max.Legs)
                                        { changed.LegCount -= 2 - target_morph.Value.Max.Legs % 2; }
                                    }
                                    else
                                    { changed.LegCount += 2 - target_morph.Value.Max.Legs % 2; }
                                }
                                else
                                { changed.ArmType = target_morph.Key; }
                                break;
                            case "face":
                                if (rng.Next(0, 100) > target_morph.Value.Transform.Permanence)
                                { break; }
                                if (original.EyeType == target_morph.Key && original.EarType == target_morph.Key &&
                                    original.TongueType == target_morph.Key && original.TeethType == target_morph.Key
                                    && original.EyeCount == target_morph.Key)
                                {
                                    if (original.FaceType != target_morph.Key)
                                    { changed.FaceType = target_morph.Key; }
                                }
                                else
                                {
                                    Boolean search = true;
                                    while (search)
                                    {
                                        switch (rng.Next(0, 5))
                                        {
                                            case 0:
                                                if (original.EyeType != target_morph.Key)
                                                {
                                                    changed.EyeType = target_morph.Key;
                                                    search = false;
                                                }
                                                break;
                                            case 1:
                                                if (original.EarType != target_morph.Key)
                                                {
                                                    changed.EarType = target_morph.Key;
                                                    search = false;
                                                }
                                                break;
                                            case 2:
                                                if (original.TongueType != target_morph.Key)
                                                {
                                                    changed.TongueType = target_morph.Key;
                                                    changed.TongueColor = getRandItem(target_morph.Value.Color.Tongue);
                                                    search = false;
                                                }
                                                break;
                                            case 3:
                                                if (original.TeethType != target_morph.Key)
                                                {
                                                    changed.TongueType = target_morph.Key;
                                                    search = false;
                                                }
                                                break;
                                            case 4:
                                                if (original.EyeCount < target_morph.Value.Max.Eyes)
                                                {
                                                    changed.EyeCount += 1;
                                                    search = false;
                                                }
                                                else if (original.EyeCount > target_morph.Value.Max.Eyes)
                                                {
                                                    changed.EyeCount += 1;
                                                    search = false;
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;
                            case "tongue":
                                if (rng.Next(0, 100) > target_morph.Value.Transform.Permanence)
                                { break; }
                                if (original.TongueType != target_morph.Key)
                                {
                                    changed.TongueType = target_morph.Key;
                                    changed.TongueColor = getRandItem(target_morph.Value.Color.Tongue);
                                }
                                else if (!target_morph.Value.Color.Tongue.Contains(original.TongueColor))
                                {
                                    changed.TongueColor = getRandItem(target_morph.Value.Color.Tongue);
                                    if (original.TongueLength > target_morph.Value.Max.TongueSize)
                                    { changed.TongueLength -= rng.Next(0, 3); }
                                    else if (original.TongueLength < target_morph.Value.Max.TongueSize)
                                    { changed.TongueLength += rng.Next(0, 3); }
                                }
                                else if (original.TongueCount != target_morph.Value.Max.TongueCount)
                                {
                                    if (original.TongueCount > target_morph.Value.Max.TongueCount)
                                    { changed.TongueCount -= 1; }
                                    else if (original.TongueCount < target_morph.Value.Max.TongueCount)
                                    { changed.TongueCount += 1; }

                                    if (original.TongueLength > target_morph.Value.Max.TongueSize)
                                    { changed.TongueLength -= rng.Next(0, 1); }
                                    else if (original.TongueLength < target_morph.Value.Max.TongueSize)
                                    { changed.TongueLength += rng.Next(0, 1); }
                                }
                                else if (original.TongueLength != target_morph.Value.Max.TongueSize)
                                {
                                    if (original.TongueLength > target_morph.Value.Max.TongueSize)
                                    { changed.TongueLength -= rng.Next(0, 3); }
                                    else if (original.TongueLength < target_morph.Value.Max.TongueSize)
                                    { changed.TongueLength += rng.Next(0, 3); }
                                }
                                break;
                            case "hair":
                                if (rng.Next(0, 100) > target_morph.Value.Transform.Permanence)
                                { break; }
                                if (original.HairType != target_morph.Key)
                                { changed.HairType = target_morph.Key; }
                                if (!target_morph.Value.Color.Hair.Contains(original.HairColor))
                                { changed.HairColor = getRandItem(target_morph.Value.Color.Hair); }
                                if (original.HairLength < target_morph.Value.Max.Hair)
                                { changed.HairLength += rng.Next(0, 4); }
                                else if (original.HairLength > target_morph.Value.Max.Hair)
                                { changed.HairLength -= rng.Next(0, 4); }
                                break;
                            case "horn":
                                if (rng.Next(0, 100) > target_morph.Value.Transform.Permanence)
                                { break; }
                                if (original.HornType == target_morph.Key && original.HornType > 0)
                                { }
                                else if (original.HornType != target_morph.Key && original.HornType > 0)
                                { }
                                else if (original.HornCount == 0)
                                {
                                    changed.HornType = target_morph.Key;
                                    changed.HornColor = getRandItem(target_morph.Value.Color.Horn);
                                    changed.HornCount += 2 - target_morph.Value.Max.Horns % 2;
                                }
                                break;
                            case "wing":
                                if (rng.Next(0, 100) > target_morph.Value.Transform.Permanence)
                                { break; }
                                if (original.WingType == target_morph.Key && original.WingCount > 0)
                                { }
                                else if (original.WingType != target_morph.Key && original.WingCount > 0 )
                                { }
                                else if (original.WingCount == 0)
                                { }
                                break;
                            case "tail":
                                if (rng.Next(0, 100) > target_morph.Value.Transform.Permanence)
                                { break; }
                                break;
                            case "feature":
                                if (rng.Next(0, 100) > target_morph.Value.Transform.Permanence)
                                { break; }
                                break;
                            case "color":
                                if (rng.Next(0, 100) > target_morph.Value.Transform.Permanence)
                                { break; }
                                break;
                            case "skin":
                                if (rng.Next(0, 100) > target_morph.Value.Transform.Permanence)
                                { break; }
                                break;
                            case "mod":
                                if (rng.Next(0, 100) > target_morph.Value.Transform.Permanence)
                                { break; }
                                break;
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

                        if (!changes.Contains("grow_" + tf.Growth.ElementAt(r).Key))
                        {
                            changes.Add("grow_" + tf.Growth.ElementAt(r).Key);
                            i += 1;
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

                        if (!changes.Contains("color_" + tf.ColorTargets[r]))
                        {
                            changes.Add("color_" + tf.ColorTargets[r]);
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

            switch (tf.Theme)
            {
                case "dust":
                    if (true) // included to make code folding work.
                    {
                        str_3rd += "";
                    }
                    break;
                case "viscera":
                    if (true)
                    {
                        str_3rd += "";
                    }
                    break;
                case "light":
                    if (true)
                    {
                        str_3rd += "";
                    }
                    break;
                case "static":
                    if (true)
                    {
                        str_3rd += "";
                    }
                    break;
                case "instant":
                    if (true)
                    {
                        str_3rd += "";
                    }
                    break;
                case "crossfade":
                    if (true)
                    {
                        str_3rd += "";
                    }
                    break;
                case "bubbles":
                    if (true)
                    {
                        str_3rd += "";
                    }
                    break;
                case "sarcastic":
                    if (true)
                    {
                        str_3rd += "";
                    }
                    break;
                case "flow":
                    if (true)
                    {
                        str_3rd += "";
                    }
                    break;
                default:
                    break;
            }


            return Tuple.Create(changed, str_2nd, str_3rd);
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
                    });

                cgb.CreateCommand(Prefix + "buy")
                    .Description($"Purchase a drink for yourself. | `{Prefix}buy \"sex on the beach\"`")
                    .Parameter("drink", ParameterType.Required)
                    .Do(async e =>
                    {
                        Drink drink;

                        drink = Drinks.Find(t => t.Code.Equals(e.GetArg("drink").ToLowerInvariant()));

                        var db = DbHandler.Instance.GetAllRows<UserMorph>();
                        Dictionary<long, UserMorph> morphs = db.Where(t => t.UserId.Equals((long)e.User.Id)).ToDictionary(x => x.UserId, y => y);

                        if (drink == null)
                        {
                            await e.Channel.SendMessage($"Sorry, {e.User.Mention}, that's not on the menu.").ConfigureAwait(false);
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
                        await FlowersHandler.RemoveFlowers(e.User, $"bought a {drink.Code}", amount).ConfigureAwait(false);

                        await e.User.SendMessage($"{drink.Flavor}").ConfigureAwait(false);

                        UserMorph morph;

                        if (morphs.ContainsKey((long)e.User.Id))
                        { morph = morphs[(long)e.User.Id]; }
                        else
                        { morph = buildMorph((long)e.User.Id, Morphs.FirstOrDefault(x => x.Value.Code == "human")); }

                        if (drink.Name != null)
                        { await e.Channel.SendMessage($"{e.User.Mention} bought {PronounSelf[morph.Gender]} {(vowelFirst(drink.Name) ? "an" : "a")} {drink.Name}.").ConfigureAwait(false); }
                        else
                        { await e.Channel.SendMessage($"{e.User.Mention} bought {PronounSelf[morph.Gender]} {(vowelFirst(drink.Code) ? "an" : "a")} {drink.Code}.").ConfigureAwait(false); }


                        if (drink.Transformative)
                        {
                            await e.Channel.SendMessage(transformUser(morph, drink, e.User).Item3).ConfigureAwait(false);
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
                        { await e.Channel.SendMessage($"{e.User.Mention} sent {target.Mention} {(vowelFirst(drink.Name) ? "an" : "a")} {drink.Name}.").ConfigureAwait(false); }
                        else
                        { await e.Channel.SendMessage($"{e.User.Mention} sent {target.Mention} {(vowelFirst(drink.Code) ? "an" : "a")} {drink.Code}.").ConfigureAwait(false); }
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
                            var db = DbHandler.Instance.GetAllRows<UserMorph>();
                            Dictionary<long, UserMorph> morphs = db.Where(t => t.UserId.Equals((long)target.Id)).ToDictionary(x => x.UserId, y => y);

                            if (morphs.ContainsKey((long)e.User.Id))
                            {
                                UserMorph morph = morphs[(long)e.User.Id];
                                if (isBaseline(morph))
                                {
                                    await e.Channel.SendMessage($"{target.Mention} is a {getWeight(morph)}baseline human.").ConfigureAwait(false);
                                }
                                else
                                {
                                    await e.Channel.SendIsTyping();

                                    String str = "$mention$ is a $weight$ $morphtype$";

                                    if (Morphs[morph.UpperType].Body.UpperType == Morphs[morph.LowerType].Body.LowerType && Morphs[morph.LowerType].Body.LowerType != null)
                                    {
                                        str += ", $a_bodytype$ $uppertype$.";
                                    }
                                    else
                                    {
                                        str += ". $pronoun$ $has$ a $bodytype$ body";
                                        if (Morphs[morph.UpperType].Body.UpperType != null && Morphs[morph.LowerType].Body.LowerType != null)
                                        { str += ", with $a_uppertype$ upper body and the lower body of $a_lowertype$."; }
                                        else if (Morphs[morph.UpperType].Body.UpperType != null)
                                        { str += ", with the upper body of $a_uppertype$."; }
                                        else if (Morphs[morph.LowerType].Body.LowerType != null)
                                        { str += ", with the lower body of $a_lowertype$."; }
                                        else
                                        { str += "."; }
                                    }


                                    if (morph.LegCount > 0 && morph.ArmCount > 0 && Morphs[morph.LowerType].Appendages.Legs != null && Morphs[morph.ArmType].Appendages.Arms != null)
                                    {
                                        str += " $pronoun$ $has$ $armcount$ $armtype$ and $legcount$ $legtype$";
                                        str += (Morphs[morph.LowerType].Body.LegAnchor != null) ? " $legposition$." : ".";
                                    }
                                    else if (morph.LegCount > 0 && Morphs[morph.LowerType].Appendages.Legs != null)
                                    {
                                        str += " $pronoun$ $has$ $legcount$ $legtype$ $legposition$.";
                                    }
                                    else if (morph.ArmCount > 0 && Morphs[morph.ArmType].Appendages.Arms != null)
                                    { str += " $pronoun$ $has$ $armcount$ $armtype$."; }
                                    else if (morph.ArmCount == 0 && morph.LegCount == 0)
                                    { str += " $pronoun$ $has$ neither arms nor legs."; }

                                    str += " $pronoun$ $has$ a $facetype$ with";
                                    str += (morph.EyeCount > 0) ? " $eyecount$ $eyecolor$ $eyetype$" : " with no eyes";

                                    if (Morphs[morph.HairType].Head.Hair != null)
                                    {
                                        str += (Colors[morph.LipColor].Name != null) ? ", $lipcolor$ lips, and" : " and";
                                        if (morph.HairLength > 0)
                                        { str += " $hairlength$ $haircolor$ $hairtype$"; }
                                        else { str += " no hair"; }
                                    }
                                    else
                                    { str += (Colors[morph.LipColor].Name != null) ? " and $lipcolor$ lips." : "."; }

                                    if (morph.HornCount > 0 && Morphs[morph.HornType].Head.Horns != null && Morphs[morph.FaceType].Head.HornAnchor != null)
                                    { str += " $hornanchor$" + ((morph.HornCount > 1) ? "s." : "."); }
                                    else { str += "."; }

                                    str += " $pronoun$ $has$ $earcount$ $eartype$";
                                    if (morph.EarCount > 1) { str += "s"; }
                                    str += " $earposition$ $objective$ head,";
                                    if (morph.TongueLength > 0)
                                    { str += " $tonguesize$ $tonguetype$,"; }
                                    else { str += " no tongue,"; }
                                    str += " and $teethtype$.\n\n";

                                    str += (Ornament[morph.SkinOrnaments].Name != null) ? "$objective$ $skincolor$ $skintype$ is $skinornament$" : "$pronoun$ $has$ $skincolor$ $skintype$";

                                    if ((Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0) &&
                                        (Skin[morph.LegCovering].Cover != null && morph.LegCount > 0) &&
                                        (Skin[morph.TorsoCovering].Cover != null))
                                    {
                                        if ((Skin[morph.ArmCovering].Cover == Skin[morph.LegCovering].Cover) ==
                                            (Skin[morph.LegCovering].Cover == Skin[morph.TorsoCovering].Cover))
                                        { str += ", and $objective$ entire body is $torsocovering$."; }
                                        else if (Skin[morph.ArmCovering].Cover == Skin[morph.LegCovering].Cover)
                                        { str += ", $objective$ arms and legs are $armcovering$, and $objective$ torso is $torsocovering$."; }
                                        else if (Skin[morph.ArmCovering].Cover == Skin[morph.TorsoCovering].Cover)
                                        { str += ", $objective$ arms and torso are $armcovering$, and $objective$ legs are $legcovering$."; }
                                        else if (Skin[morph.LegCovering].Cover == Skin[morph.TorsoCovering].Cover)
                                        { str += ", $objective$ legs and torso are $legcovering$, and $objective$ arms are $armcovering$."; }
                                        else
                                        { str += ", $objective$ arms are $armcovering$, $objective$ legs are $legcovering$, and $objective$ torso is $torsocovering$."; }

                                    }
                                    else if ((Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0) &&
                                            (Skin[morph.LegCovering].Cover != null && morph.LegCount > 0))
                                    {
                                        if (Skin[morph.ArmCovering].Cover == Skin[morph.LegCovering].Cover)
                                        { str += ", and $objective$ arms and legs are $armcovering$."; }
                                        else
                                        { str += ", $objective$ arms are $armcovering$, and $objective$ legs are $legcovering$."; }
                                    }
                                    else if ((Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0) &&
                                            (Skin[morph.TorsoCovering].Cover != null))
                                    {
                                        if (Skin[morph.ArmCovering].Cover == Skin[morph.TorsoCovering].Cover)
                                        { str += ", and $objective$ arms and torso are $armcovering$."; }
                                        else
                                        { str += ", $objective$ arms are $armcovering$, and $objective$ torso is $torsocovering$."; }
                                    }
                                    else if ((Skin[morph.LegCovering].Cover != null && morph.LegCount > 0) &&
                                            (Skin[morph.TorsoCovering].Cover != null))
                                    {
                                        if (Skin[morph.LegCovering].Cover == Skin[morph.TorsoCovering].Cover)
                                        { str += ", and $objective$ torso and legs are $legcovering$."; }
                                        else
                                        { str += ", $objective$ torso is $torsocovering$, and $objective$ legs are $legcovering$."; }
                                    }
                                    else if (Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0)
                                    { str += ", and $objective$ arms are $armcovering$."; }
                                    else if (Skin[morph.LegCovering].Cover != null && morph.LegCount > 0)
                                    { str += ", and $objective$ legs are $legcovering."; }
                                    else if (Skin[morph.TorsoCovering].Cover != null)
                                    { str += ", and $objective$ torso is $torsocovering$."; }
                                    else { str += "."; }

                                    if (Morphs[morph.HandModification].Appendages.HandMod != null && Morphs[morph.FeetModification].Appendages.FeetMod != null && morph.LegCount > 0 && morph.ArmCount > 0)
                                    {
                                        if (Morphs[morph.HandModification].Appendages.HandMod == Morphs[morph.FeetModification].Appendages.FeetMod)
                                        { str += " $objective$ $handtype$ and $feettype$ are $handmodification$."; }
                                        else
                                        { str += " $objective$ $handtype$ are $handmodification$ and $objective$ $feettype$ are $feetmodification$."; }
                                    }
                                    else if (Morphs[morph.HandModification].Appendages.HandMod != null && morph.ArmCount > 0)
                                    { str += " $objective$ $handtype$ are $handmoficiation$."; }
                                    else if (Morphs[morph.FeetModification].Appendages.FeetMod != null && morph.LegCount > 0)
                                    { str += " $objective$ $feettype$ are $feetmodification$."; }

                                    if (Morphs[morph.WingType].Appendages.Wings != null && Morphs[morph.UpperType].Body.WingAnchor != null && Morphs[morph.TailType].Appendages.Tail != null &&
                                        Morphs[morph.LowerType].Body.TailAnchor != null && morph.TailCount > 0 && morph.WingCount > 0)
                                    { str += " $pronoun$ $has$ $wingtype$ $wingposition$ and $tailtype$ $tailposition$"; }
                                    else if (Morphs[morph.WingType].Appendages.Wings != null && Morphs[morph.UpperType].Body.WingAnchor != null && morph.WingCount > 0)
                                    { str += " $pronoun$ $has$ $wingtype$ $wingposition$."; }
                                    else if (Morphs[morph.TailType].Appendages.Tail != null && Morphs[morph.LowerType].Body.TailAnchor != null && morph.TailCount > 0)
                                    { str += " $pronoun$ $has$ $tailtype% $tailposition$."; }

                                    if (Morphs[morph.ArmFeature].Appendages.ArmFeature != null && Morphs[morph.LegFeature].Appendages.LegFeature != null)
                                    {
                                        if (morph.ArmFeature == morph.LegFeature)
                                        { str += " $pronoun$ $has$ $bothfeature$."; }
                                        else
                                        { str += " $pronoun$ $has$ $legfeature$ and $armfeature$."; }
                                    }
                                    else if (Morphs[morph.ArmFeature].Appendages.ArmFeature != null)
                                    { str += " $pronoun$ $has$ $armfeature$."; }
                                    else if (Morphs[morph.LegFeature].Appendages.LegFeature != null)
                                    { str += " $pronoun$ $has$ $legfeature$."; }

                                    var swapper = new Dictionary<string, string>(
                                        StringComparer.OrdinalIgnoreCase) {
                                            {"$mention$", target.Mention },

                                            {"$weight$", getWeight(morph) },
                                            {"$morphtype$", getDominantType(morph) },

                                            {"$pronoun$", Pronoun[morph.Gender]},
                                            {"$has$", PronounHas[morph.Gender]},
                                            {"$objective$", PronounObjective[morph.Gender]},
                                            {"$are$", Are[morph.Gender] },

                                            {"$bodytype$",(morph.UpperType == morph.LowerType) ? Morphs[morph.LowerType].Body.BodyType : "tauric " + Morphs[morph.LowerType].Body.BodyType },
                                            {"$a_uppertype$", (vowelFirst(Morphs[morph.UpperType].Body.UpperType) ? "an " : "a ") + Morphs[morph.UpperType].Body.UpperType },
                                            {"$a_lowertype$", (vowelFirst(Morphs[morph.LowerType].Body.LowerType) ? "an " : "a ") + Morphs[morph.LowerType].Body.LowerType },
                                            {"$uppertype$", Morphs[morph.UpperType].Body.UpperType },
                                            {"$a_bodytype$", (vowelFirst(Morphs[morph.UpperType].Body.BodyType) ? "an " : "a ") + Morphs[morph.LowerType].Body.BodyType },

                                            {"$armcount$", NumberToWords(morph.ArmCount) },
                                            {"$armtype$", Morphs[morph.ArmType].Appendages.Arms },
                                            {"$legcount$", NumberToWords(morph.LegCount) },
                                            {"$legtype$", Morphs[morph.LegType].Appendages.Legs },
                                            {"$legposition$", Morphs[morph.LowerType].Body.LegAnchor },

                                            {"$wingtype$", Morphs[morph.WingType].Appendages.Wings},
                                            {"$wingsize$", (Morphs[morph.WingType].Appendages.WingSizes != null) ? Morphs[morph.WingType].Appendages.WingSizes[morph.WingSize] : ""},
                                            {"$wingcount$", NumberToWords(morph.WingCount)},
                                            {"$wingposition$", Morphs[morph.UpperType].Body.WingAnchor },
                                            {"$wingcolor$", Colors[morph.WingColor].Name },

                                            {"$tailtype$", Morphs[morph.TailType].Appendages.Tail},
                                            {"$tailsize$", (Morphs[morph.TailType].Appendages.TailSizes != null) ? Morphs[morph.TailType].Appendages.TailSizes[morph.TailSize] : ""},
                                            {"$tailcount$", NumberToWords(morph.TailCount)},
                                            {"$tailpositon$", Morphs[morph.LowerType].Body.TailAnchor },
                                            {"$tailcolor$", Colors[morph.TailColor].Name },

                                            {"$facetype$", Morphs[morph.FaceType].Head.Head },
                                            {"$eyetype$", (morph.EyeCount > 1) ? Morphs[morph.EyeType].Head.Eyes + "s" : Morphs[morph.EyeType].Head.Eyes },
                                            {"$eyecount$", NumberToWords(morph.EyeCount) },
                                            {"$eyecolor$", Colors[morph.EyeColor].Name},
                                            {"$tonguetype$", Morphs[morph.TongueType].Head.Tongue },
                                            {"$tonguesize$", getTongue(morph) },
                                            {"$teethtype$", Morphs[morph.TeethType].Head.Teeth },
                                            {"$lipcolor$", Colors[morph.LipColor].Name },

                                            {"$earposition$", Morphs[morph.EarType].Head.EarAnchor },
                                            {"$earcount$", NumberToWords(morph.EarCount) },
                                            {"$eartype$", Morphs[morph.EarType].Head.Ears },

                                            {"$feettype$", Morphs[morph.FeetType].Appendages.Feet },
                                            {"$handtype$", Morphs[morph.HandType].Appendages.Hands },
                                            {"$handmodification$", Morphs[morph.HandModification].Appendages.HandMod },
                                            {"$feetmodification$", Morphs[morph.FeetModification].Appendages.FeetMod },

                                            {"$torsocovering$", Skin[morph.TorsoCovering].Cover },
                                            {"$legcovering$", Skin[morph.LegCovering].Cover },
                                            {"$armcovering$", Skin[morph.ArmCovering].Cover },
                                            {"$covercolor$", Colors[morph.CoveringColor].Name },

                                            {"$skintype$", Skin[morph.SkinType].Text },
                                            {"$skincolor$", Colors[morph.SkinColor].Name },
                                            {"$skinornament$", Ornament[morph.SkinOrnaments].Name},
                                            {"$ornamentcolor$", Colors[morph.OrnamentColor].Name },

                                            {"$hairtype$", Morphs[morph.HairType].Head.Hair },
                                            {"$haircolor$", Colors[morph.HairColor].Name },
                                            {"$hairlength$", getHair(morph) },

                                            {"$horntype$", Morphs[morph.HornType].Head.Horns },
                                            {"$hornsize$", (Morphs[morph.HornType].Head.HornSizes != null) ? Morphs[morph.HornType].Head.HornSizes[morph.HornSize] : ""},
                                            {"$hornanchor$", Morphs[morph.FaceType].Head.HornAnchor },
                                            {"$horncolor$", Colors[morph.HornColor].Name },
                                            {"$horncount$", NumberToWords(morph.HornCount) },

                                            {"$neckfeature$", Morphs[morph.NeckFeature].Head.NeckFeature },
                                            {"$neckcolor$", Colors[morph.NeckColor].Name },
                                            {"$armfeature$", Morphs[morph.ArmFeature].Appendages.ArmFeature },
                                            {"$armcolor$", Colors[morph.ArmColor].Name },
                                            {"$legfeature$", Morphs[morph.LegFeature].Appendages.LegFeature },
                                            {"$legcolor$", Colors[morph.LegColor].Name },
                                            {"$bothfeature$", Morphs[morph.ArmFeature].Appendages.BothFeature },

                                            {"  ", " "},
                                        };

                                    // first pass
                                    str = swapper.Aggregate(str, (current, value) => current.Replace(value.Key, value.Value));
                                    // second pass
                                    str = swapper.Aggregate(str, (current, value) => current.Replace(value.Key, value.Value));
                                    // third pass
                                    str = swapper.Aggregate(str, (current, value) => current.Replace(value.Key, value.Value));

                                    await e.Channel.SendMessage(str.CapitalizeFirst()).ConfigureAwait(false);
                                }

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
                            var db = DbHandler.Instance.GetAllRows<UserMorph>();
                            Dictionary<long, UserMorph> morphs = db.Where(t => t.UserId.Equals((long)e.User.Id)).ToDictionary(x => x.UserId, y => y);

                            if (morphs.ContainsKey((long)e.User.Id))
                            {
                                UserMorph morph = morphs[(long)e.User.Id];
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
                                UserMorph morph = buildMorph((long)e.User.Id, Morphs.FirstOrDefault(x => x.Value.Code == "human"));

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
                        // see dbhandler.save

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
                            DbHandler.Instance.Save(buildMorph((long)e.User.Id, target_morph));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                        await e.Channel.SendMessage($"All will be well when you wake, {target.Mention}. Relax and embrace the void.").ConfigureAwait(false);
                    });
            });
        }
    }
}
