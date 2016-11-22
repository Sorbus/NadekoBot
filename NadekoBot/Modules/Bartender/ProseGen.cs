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
    class ProseGen
    {
        private List<Drink> Drinks = NadekoBot.Config.Drinks;

        private Dictionary<int, TFMorph> Morphs = NadekoBot.Config.Morphs;
        private Dictionary<int, TFColor> Colors = NadekoBot.Config.Colors;
        private Dictionary<int, TFOrnament> Ornament = NadekoBot.Config.Ornament;
        private Dictionary<int, TFSkin> Skin = NadekoBot.Config.Skin;

        public static String[] PronounPossessive = new String[3] { "their", "her", "his" };
        public static String[] PronounSubjective = new String[3] { "they", "she", "he" };
        public static String[] PronounObjective = new String[3] { "them", "her", "him" };

        public static String[] Has = new String[3] { "have", "has", "has" };
        public static String[] Are = new String[3] { "are", "is", "is" };
        public static String[] Was = new String[3] { "were", "was", "was" };

        public static String[] PronounSelf = new String[3] { "themself", "herself", "himself" };

        public static readonly Regex re = new Regex(@"\$(\w+)\$", RegexOptions.Compiled);

        public string GetState(MorphModel morph, Discord.User target)
        {
            if (DescHelp.isBaseline(morph))
            {
                return $"{target.Mention} is a {DescHelp.getWeight(morph)} baseline human.".Replace("  ", " ");
            }
            else
            {

                StringBuilder str = new StringBuilder("$mention$ is a $weight$ $morphtype$", 2000);

                if (Morphs[morph.UpperType].Body.UpperType == Morphs[morph.LowerType].Body.LowerType && Morphs[morph.LowerType].Body.LowerType != null)
                {
                    if (Morphs[morph.UpperType].Body.BodyType != null)
                    { str.Append(", $a_bodytype$ $uppertype$."); }
                    else
                    { str.Append("."); }
                }
                else
                {
                    str.Append(". $subjective$ $has$ a $bodytype$ body");
                    if (Morphs[morph.UpperType].Body.UpperType != null && Morphs[morph.LowerType].Body.LowerType != null)
                    { str.Append(", with $a_uppertype$ upper body and the lower body of $a_lowertype$."); }
                    else if (Morphs[morph.UpperType].Body.UpperType != null)
                    { str.Append(", with the upper body of $a_uppertype$."); }
                    else if (Morphs[morph.LowerType].Body.LowerType != null)
                    { str.Append(", with the lower body of $a_lowertype$."); }
                    else
                    { str.Append("."); }
                }


                if (morph.LegCount > 0 && morph.ArmCount > 0 && Morphs[morph.LowerType].Appendages.Legs != null && Morphs[morph.ArmType].Appendages.Arms != null)
                {
                    str.Append(" $subjective$ $has$ $armcount$ $armtype$ and $legcount$ $legtype$");
                    str.Append((Morphs[morph.LowerType].Body.LegAnchor != null) ? " $legposition$." : ".");
                }
                else if (morph.LegCount > 0 && Morphs[morph.LowerType].Appendages.Legs != null)
                {
                    str.Append(" $subjective$ $has$ $legcount$ $legtype$ $legposition$.");
                }
                else if (morph.ArmCount > 0 && Morphs[morph.ArmType].Appendages.Arms != null)
                { str.Append(" $subjective$ $has$ $armcount$ $armtype$."); }
                else if (morph.ArmCount == 0 && morph.LegCount == 0)
                { str.Append(" $subjective$ $has$ neither arms nor legs."); }

                str.Append(" $subjective$ $has$ $a_headtype$ with");
                str.Append((morph.EyeCount > 0) ? " $eyecount$ $eyecolor$ $eyetype$" : " with no eyes");

                if (Morphs[morph.HairType].Head.Hair != null)
                {
                    str.Append((Colors[morph.LipColor].Name != null) ? ", $lipcolor$ lips, and" : " and");
                    if (morph.HairLength > 0)
                    { str.Append(" $hairlength$ $haircolor$ $hairtype$"); }
                    else { str.Append(" no hair"); }
                }
                else
                { str.Append((Colors[morph.LipColor].Name != null) ? " and $lipcolor$ lips." : "."); }

                if (morph.HornCount > 0 && Morphs[morph.HornType].Head.Horns != null && Morphs[morph.FaceType].Head.HornAnchor != null)
                { str.Append((" $hornanchor$") + ((morph.HornCount > 1) ? "s." : ".")); }
                else { str.Append("."); }

                str.Append(" $subjective$ $has$ $earcount$ $eartype$");
                if (morph.EarCount > 1) { str.Append("s"); }
                str.Append(" $earposition$ $possessive$ head,");
                if (morph.TongueLength > 0)
                { str.Append(" $tonguesize$ $tonguetype$,"); }
                else { str.Append(" no tongue,"); }
                str.Append(" and $teethtype$.\n\n");

                if (Skin[morph.SkinType].Text != null)
                {
                    str.Append((Ornament[morph.SkinOrnaments].Name != null) ? "$possessive$ $skincolor$ $skintype$ is $skinornament$" : "$subjective$ $has$ $skincolor$ $skintype$");

                    if ((Skin[morph.ArmCovering].Cover == Skin[morph.LegCovering].Cover) ==
                        (Skin[morph.LegCovering].Cover == Skin[morph.TorsoCovering].Cover))
                    { str.Append(", and "); }
                    else { str.Append(", "); }
                }

                if ((Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0) &&
                    (Skin[morph.LegCovering].Cover != null && morph.LegCount > 0) &&
                    (Skin[morph.TorsoCovering].Cover != null))
                {
                    if ((Skin[morph.ArmCovering].Cover == Skin[morph.LegCovering].Cover) ==
                        (Skin[morph.LegCovering].Cover == Skin[morph.TorsoCovering].Cover))
                    { str.Append("$possessive$ entire body is $torsocovering$."); }
                    else if (Skin[morph.ArmCovering].Cover == Skin[morph.LegCovering].Cover)
                    { str.Append("$possessive$ arms and legs are $armcovering$, and $possessive$ torso is $torsocovering$."); }
                    else if (Skin[morph.ArmCovering].Cover == Skin[morph.TorsoCovering].Cover)
                    { str.Append("$possessive$ arms and torso are $armcovering$, and $possessive$ legs are $legcovering$."); }
                    else if (Skin[morph.LegCovering].Cover == Skin[morph.TorsoCovering].Cover)
                    { str.Append("$possessive$ legs and torso are $legcovering$, and $possessive$ arms are $armcovering$."); }
                    else
                    { str.Append("$possessive$ arms are $armcovering$, $possessive$ legs are $legcovering$, and $possessive$ torso is $torsocovering$."); }

                }
                else if ((Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0) &&
                        (Skin[morph.LegCovering].Cover != null && morph.LegCount > 0))
                {
                    if (Skin[morph.ArmCovering].Cover == Skin[morph.LegCovering].Cover)
                    { str.Append(", and $possessive$ arms and legs are $armcovering$."); }
                    else
                    { str.Append(", $possessive$ arms are $armcovering$, and $possessive$ legs are $legcovering$."); }
                }
                else if ((Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0) &&
                        (Skin[morph.TorsoCovering].Cover != null))
                {
                    if (Skin[morph.ArmCovering].Cover == Skin[morph.TorsoCovering].Cover)
                    { str.Append(", and $possessive$ arms and torso are $armcovering$."); }
                    else
                    { str.Append(", $possessive$ arms are $armcovering$, and $possessive$ torso is $torsocovering$."); }
                }
                else if ((Skin[morph.LegCovering].Cover != null && morph.LegCount > 0) &&
                        (Skin[morph.TorsoCovering].Cover != null))
                {
                    if (Skin[morph.LegCovering].Cover == Skin[morph.TorsoCovering].Cover)
                    { str.Append(", and $possessive$ torso and legs are $legcovering$."); }
                    else
                    { str.Append(", $possessive$ torso is $torsocovering$, and $possessive$ legs are $legcovering$."); }
                }
                else if (Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0)
                { str.Append(", and $possessive$ arms are $armcovering$."); }
                else if (Skin[morph.LegCovering].Cover != null && morph.LegCount > 0)
                { str.Append(", and $possessive$ legs are $legcovering."); }
                else if (Skin[morph.TorsoCovering].Cover != null)
                { str.Append(", and $possessive$ torso is $torsocovering$."); }
                else { str.Append("."); }

                if (Morphs[morph.HandMod].Appendages.HandMod != null && Morphs[morph.FeetMod].Appendages.FeetMod != null && morph.LegCount > 0 && morph.ArmCount > 0)
                {
                    if (Morphs[morph.HandMod].Appendages.HandMod == Morphs[morph.FeetMod].Appendages.FeetMod)
                    { str.Append(" $possessive$ $handtype$ and $feettype$ are $handmodification$."); }
                    else
                    { str.Append(" $possessive$ $handtype$ are $handmodification$ and $possessive$ $feettype$ are $feetmodification$."); }
                }
                else if (Morphs[morph.HandMod].Appendages.HandMod != null && morph.ArmCount > 0)
                { str.Append(" $possessive$ $handtype$ are $handmoficiation$."); }
                else if (Morphs[morph.FeetMod].Appendages.FeetMod != null && morph.LegCount > 0)
                { str.Append(" $possessive$ $feettype$ are $feetmodification$."); }

                if (Morphs[morph.WingType].Appendages.Wings != null && Morphs[morph.UpperType].Body.WingAnchor != null && Morphs[morph.TailType].Appendages.Tail != null &&
                    Morphs[morph.LowerType].Body.TailAnchor != null && morph.TailCount > 0 && morph.WingCount > 0)
                { str.Append(" $subjective$ $has$ $wingcount$ $wingsize$ $wingtype$ $wingposition$ and $tailcount$ $tailsize$ $tailtype$ $tailposition$"); }
                else if (Morphs[morph.WingType].Appendages.Wings != null && Morphs[morph.UpperType].Body.WingAnchor != null && morph.WingCount > 0)
                { str.Append(" $subjective$ $has$ $wingcount$ $wingsize$ $wingtype$ $wingposition$."); }
                else if (Morphs[morph.TailType].Appendages.Tail != null && Morphs[morph.LowerType].Body.TailAnchor != null && morph.TailCount > 0)
                { str.Append(" $subjective$ $has$ $tailcount$ $tailsize$ $tailtype$ $tailposition$."); }

                if (Morphs[morph.ArmFeature].Appendages.ArmFeature != null && Morphs[morph.LegFeature].Appendages.LegFeature != null)
                {
                    if (morph.ArmFeature == morph.LegFeature)
                    { str.Append(" $subjective$ $has$ $bothfeature$."); }
                    else
                    { str.Append(" $subjective$ $has$ $legfeature$ and $armfeature$."); }
                }
                else if (Morphs[morph.ArmFeature].Appendages.ArmFeature != null)
                { str.Append(" $subjective$ $has$ $armfeature$."); }
                else if (Morphs[morph.LegFeature].Appendages.LegFeature != null)
                { str.Append(" $subjective$ $has$ $legfeature$."); }

                var swapper = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase) {
                    {"$mention$", target.Mention },
                    {"$name$", target.Name },

                    {"$weight$", DescHelp.getWeight(morph) },
                    {"$morphtype$", DescHelp.getDominantType(morph, Morphs) },

                    {"$subjective$", PronounSubjective[morph.Gender]},
                    {"$objective$", PronounObjective[morph.Gender]},
                    {"$possessive$", PronounPossessive[morph.Gender]},
                    {"$are$", Are[morph.Gender] },
                    {"$has$", Has[morph.Gender]},

                    //{"$bodytype$",(morph.UpperType == morph.LowerType) ? Morphs[morph.LowerType].Body.BodyType : "tauric " + Morphs[morph.LowerType].Body.BodyType },
                    {"$a_uppertype$", (DescHelp.vowelFirst(Morphs[morph.UpperType].Body.UpperType) ? "an " : "a ") + Morphs[morph.UpperType].Body.UpperType },
                    {"$a_lowertype$", (DescHelp.vowelFirst(Morphs[morph.LowerType].Body.LowerType) ? "an " : "a ") + Morphs[morph.LowerType].Body.LowerType },
                    {"$uppertype$", Morphs[morph.UpperType].Body.UpperType },
                    //{"$a_bodytype$", (DescHelp.vowelFirst(Morphs[morph.UpperType].Body.BodyType) ? "an " : "a ") + Morphs[morph.LowerType].Body.BodyType },

                    {"$armcount$", BarHelp.NumberToWords(morph.ArmCount) },
                    {"$armtype$", Morphs[morph.ArmType].Appendages.Arms },
                    {"$legcount$", BarHelp.NumberToWords(morph.LegCount) },
                    {"$legtype$", Morphs[morph.LegType].Appendages.Legs },
                    {"$legposition$", Morphs[morph.LowerType].Body.LegAnchor },

                    {"$wingtype$", Morphs[morph.WingType].Appendages.Wings},
                    {"$wingsize$", (Morphs[morph.WingType].Appendages.WingSizes != null) ? Morphs[morph.WingType].Appendages.WingSizes[morph.WingSize] : ""},
                    {"$wingcount$", (morph.WingCount == 1) ? "a" : BarHelp.NumberToWords(morph.WingCount) },
                    {"$wingposition$", Morphs[morph.UpperType].Body.WingAnchor },
                    {"$wingcolor$", Colors[morph.WingColor].Name },

                    {"$tailtype$", Morphs[morph.TailType].Appendages.Tail},
                    {"$tailsize$", (Morphs[morph.TailType].Appendages.TailSizes != null) ? Morphs[morph.TailType].Appendages.TailSizes[morph.TailSize] : ""},
                    {"$tailcount$", (morph.TailCount == 1) ? "a" : BarHelp.NumberToWords(morph.TailCount) },
                    {"$tailposition$", Morphs[morph.LowerType].Body.TailAnchor },
                    {"$tailcolor$", Colors[morph.TailColor].Name },

                    {"$headtype$", Morphs[morph.FaceType].Head.Head },
                    {"$a_headtype$", (DescHelp.vowelFirst(Morphs[morph.FaceType].Head.Head) ? "an " : "a ") + Morphs[morph.FaceType].Head.Head },
                    {"$eyetype$", (morph.EyeCount > 1) ? Morphs[morph.EyeType].Head.Eyes + "s" : Morphs[morph.EyeType].Head.Eyes },
                    {"$eyecount$", BarHelp.NumberToWords(morph.EyeCount) },
                    {"$eyecolor$", Colors[morph.EyeColor].Name},
                    {"$tonguetype$", Morphs[morph.TongueType].Head.Tongue },
                    {"$tonguesize$", DescHelp.getTongue(morph) },
                    {"$teethtype$", Morphs[morph.TeethType].Head.Teeth },
                    {"$lipcolor$", Colors[morph.LipColor].Name },

                    {"$earposition$", Morphs[morph.EarType].Head.EarAnchor },
                    {"$earcount$", BarHelp.NumberToWords(morph.EarCount) },
                    {"$eartype$", Morphs[morph.EarType].Head.Ears },

                    {"$feettype$", Morphs[morph.FeetType].Appendages.Feet },
                    {"$handtype$", Morphs[morph.HandType].Appendages.Hands },
                    {"$handmodification$", Morphs[morph.HandMod].Appendages.HandMod },
                    {"$feetmodification$", Morphs[morph.FeetMod].Appendages.FeetMod },

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
                    {"$hairlength$", DescHelp.getHair(morph) },

                    {"$horntype$", Morphs[morph.HornType].Head.Horns },
                    {"$hornsize$", (Morphs[morph.HornType].Head.HornSizes != null) ? Morphs[morph.HornType].Head.HornSizes[morph.HornSize] : ""},
                    {"$hornanchor$", Morphs[morph.FaceType].Head.HornAnchor },
                    {"$horncolor$", Colors[morph.HornColor].Name },
                    {"$horncount$", BarHelp.NumberToWords(morph.HornCount) },

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

                return (str.ToString().CapitalizeFirst());
            }
        }

        public Tuple<String, String> GetChange(MorphModel old, MorphModel now, TFDetails tf, List<string> changes)
        {
            StringBuilder str_3rd = new StringBuilder(2000);
            StringBuilder str_2nd = new StringBuilder(2000);

            switch (tf.Theme)
            {
                case "debug":
                    PrintChanges(old, now, changes);
                    break;
                case "dust":
                    if (true) // included to make code folding work.
                    {
                        str_3rd.Append("");
                    }
                    break;
                case "viscera":
                    if (true)
                    {
                        str_3rd.Append("");
                    }
                    break;
                case "light":
                    if (true)
                    {
                        str_3rd.Append("");
                    }
                    break;
                case "static":
                    if (true)
                    {
                        str_3rd.Append("As $username$ swallows, the air around $objective$ begins to flicker and glitch." +
                            " While the glitches are everywhere, they are most concentrated around $possessive$ ");

                        if (changes.Count == 1)
                        { str_3rd.Append(DescHelp.locationName(Morphs, old, now, changes.First()) + " "); }
                        else
                        {
                            int c = changes.Count;
                            int i = 0;
                            while (c > 1)
                            {
                                str_3rd.Append(DescHelp.locationName(Morphs, old, now, changes[i]) + ", ");
                                c -= 1;
                                i += 1;
                            }
                            str_3rd.Append("and " + DescHelp.locationName(Morphs, old, now, changes[i]));
                        }


                    }
                    break;
                case "instant":
                    if (true)
                    {
                        str_3rd.Append("");
                    }
                    break;
                case "crossfade":
                    if (true)
                    {
                        str_3rd.Append("");
                    }
                    break;
                case "bubbles":
                    if (true)
                    {
                        str_3rd.Append("");
                    }
                    break;
                case "sarcastic":
                    if (true)
                    {
                        str_3rd.Append("");
                    }
                    break;
                case "flow":
                    if (true)
                    {
                        str_3rd.Append("");
                    }
                    break;
                default:
                    break;
            }

            return new Tuple<String, String>(str_2nd.ToString(), str_3rd.ToString());
        }

        public void PrintChanges(MorphModel old, MorphModel now, List<String> changes)
        {
            if (changes.Contains("upper")) // 7: ArmType, ArmFeature, ArmColor, ArmCount, HandModification, HandType, UpperType
            {
                if (DescHelp.isChanged(old, now, "ArmColor"))
                {
                    Console.WriteLine($"Arm Color: {Colors[old.ArmColor].Name} -> {Colors[now.ArmColor].Name}");
                }
                if (DescHelp.isChanged(old, now, "ArmCount"))
                {
                    Console.WriteLine($"Arm Count: {old.ArmCount} -> {now.ArmCount}");
                }
                if (DescHelp.isChanged(old, now, "ArmType"))
                {
                    Console.WriteLine($"Arm Type: {Morphs[old.ArmType].Appendages.Arms} -> {Morphs[now.ArmType].Appendages.Arms}");
                }
                if (DescHelp.isChanged(old, now, "HandType"))
                {
                    Console.WriteLine($"Hand Type: {Morphs[old.HandType].Appendages.Hands} -> {Morphs[now.HandType].Appendages.Hands}");
                }
                if (DescHelp.isChanged(old, now, "UpperType"))
                {
                    Console.WriteLine($"Upper Type: {Morphs[old.UpperType].Body.UpperType} -> {Morphs[now.UpperType].Body.UpperType}");
                }
                if (DescHelp.isChanged(old, now, "ArmFeature"))
                {
                    Console.WriteLine($"Arm Feature: {Morphs[old.ArmFeature].Appendages.ArmFeature} -> {Morphs[now.ArmFeature].Appendages.ArmFeature}");
                }
                if (DescHelp.isChanged(old, now, "HandtModification"))
                {
                    Console.WriteLine($"Foot Modification: {Morphs[old.HandMod].Appendages.HandMod} -> {Morphs[now.HandMod].Appendages.HandMod}");
                }
            }
            if (changes.Contains("lower")) // 7: LegType, LegFeature, LegColor, LegCount, FootModification, FootType, LowerType
            {
                if (DescHelp.isChanged(old, now, "LegColor"))
                {
                    Console.WriteLine($"Leg Color: {Colors[old.LegColor].Name} -> {Colors[now.LegColor].Name}");
                }
                if (DescHelp.isChanged(old, now, "LegCount"))
                {
                    Console.WriteLine($"Leg Count: {old.LegCount} -> {now.LegCount}");
                }
                if (DescHelp.isChanged(old, now, "LegType"))
                {
                    Console.WriteLine($"Leg Type: {Morphs[old.LegType].Appendages.Legs} -> {Morphs[now.LegType].Appendages.Legs}");
                }
                if (DescHelp.isChanged(old, now, "FeetType"))
                {
                    Console.WriteLine($"Feet Type: {Morphs[old.FeetType].Appendages.Feet} -> {Morphs[now.FeetType].Appendages.Feet}");
                }
                if (DescHelp.isChanged(old, now, "LowerType"))
                {
                    Console.WriteLine($"Lower Type: {Morphs[old.LowerType].Body.LowerType} -> {Morphs[now.LowerType].Body.LowerType}");
                }
                if (DescHelp.isChanged(old, now, "LegFeature"))
                {
                    Console.WriteLine($"Leg Feature: {Morphs[old.LegFeature].Appendages.LegFeature} -> {Morphs[now.LegFeature].Appendages.LegFeature}");
                }
                if (DescHelp.isChanged(old, now, "FeettModification"))
                {
                    Console.WriteLine($"Foot Modification: {Morphs[old.FeetMod].Appendages.FeetMod} -> {Morphs[now.FeetMod].Appendages.FeetMod}");
                }

            }
            if (changes.Contains("face")) // 8: EyeType, EyeColor, EarType, TongueType, TeethType, LipColor, EyeCount, FaceType
            {
                if (DescHelp.isChanged(old, now, "EyeColor"))
                {
                    Console.WriteLine($"Eye Color: {Colors[old.EyeColor].Name} -> {Colors[now.EyeColor].Name}");
                }
                if (DescHelp.isChanged(old, now, "EyeType"))
                {
                    Console.WriteLine($"Eye Type: {Morphs[old.EyeType].Head.Eyes} -> {Morphs[now.EyeType].Head.Eyes}");
                }
                if (DescHelp.isChanged(old, now, "EyeCount"))
                {
                    Console.WriteLine($"Eye Count: {old.EyeCount} -> {now.EyeCount}");
                }
                if (DescHelp.isChanged(old, now, "TongueType"))
                {
                    Console.WriteLine($"Tongue Type: {Morphs[old.TongueType].Head.Tongue} -> {Morphs[now.TongueType].Head.Tongue}");
                }
                if (DescHelp.isChanged(old, now, "TeethType"))
                {
                    Console.WriteLine($"Teeth Type: {Morphs[old.TeethType].Head.Teeth} -> {Morphs[now.TeethType].Head.Teeth}");
                }
                if (DescHelp.isChanged(old, now, "FaceType"))
                {
                    Console.WriteLine($"Face Type: {Morphs[old.FaceType].Head.Head} -> {Morphs[now.FaceType].Head.Head}");
                }
                if (DescHelp.isChanged(old, now, "EarType"))
                {
                    Console.WriteLine($"Ear Type: {Morphs[old.EarType].Head.Ears} -> {Morphs[now.EarType].Head.Ears}");
                }
                if (DescHelp.isChanged(old, now, "LipColor"))
                {
                    Console.WriteLine($"Lip Color: {Colors[old.LipColor].Name} -> {Colors[now.LipColor].Name}");
                }
            }
            if (changes.Contains("tongue")) // 4: TongueType, TongueColor, TongueCount, TongueLength
            {
                if (DescHelp.isChanged(old, now, "TongueColor"))
                {
                    Console.WriteLine($"Tongue Color: {Colors[old.TongueColor].Name} -> {Colors[now.TongueColor].Name}");
                }
                if (DescHelp.isChanged(old, now, "TongueType"))
                {
                    Console.WriteLine($"Tongue Type: {Morphs[old.TongueType].Head.Tongue} -> {Morphs[now.TongueType].Head.Tongue}");
                }
                if (DescHelp.isChanged(old, now, "TongueCount"))
                {
                    Console.WriteLine($"Tongue Count: {old.TongueCount} -> {now.TongueCount}");
                }
                if (DescHelp.isChanged(old, now, "TongueLength"))
                {
                    Console.WriteLine($"Tongue Size: {old.TongueLength} -> {now.TongueLength}");
                }
            }
            if (changes.Contains("hair")) // 3: HairType, HairColor, HairLength
            {
                if (DescHelp.isChanged(old, now, "HairColor")){
                    Console.WriteLine($"Hair Color: {Colors[old.HairColor].Name} -> {Colors[now.HairColor].Name}");
                }
                if (DescHelp.isChanged(old, now, "HairType"))
                {
                    Console.WriteLine($"Hair Type: {Morphs[old.HairType].Head.Hair} -> {Morphs[now.HairType].Head.Hair}");
                }
                if (DescHelp.isChanged(old, now, "HairLength"))
                {
                    Console.WriteLine($"Hair Size: {old.HairLength} -> {now.HairLength}");
                }
            }
            if (changes.Contains("horn")) // 4: HornCount, HornType, HornColor, HornSize
            {
                if (DescHelp.isChanged(old, now, "HornColor")){
                    Console.WriteLine($"Horn Color: {Colors[old.HornColor].Name} -> {Colors[now.HornColor].Name}");
                }
                if (DescHelp.isChanged(old, now, "HornType"))
                {
                    Console.WriteLine($"Horn Type: {Morphs[old.HornType].Head.Horns} -> {Morphs[now.HornType].Head.Horns}");
                }
                if (DescHelp.isChanged(old, now, "HornCount"))
                {
                    Console.WriteLine($"Horn Count: {old.HornCount} -> {now.HornCount}");
                }
                if (DescHelp.isChanged(old, now, "HornSize"))
                {
                    Console.WriteLine($"Horn Size: {old.HornSize} -> {now.HornSize}");
                }
            }
            if (changes.Contains("wing")) // 4: WingCount, WingType, WingColor, WingSize
            {
                if (DescHelp.isChanged(old, now, "WingColor"))
                {
                    Console.WriteLine($"Wing Color: {Colors[old.WingColor].Name} -> {Colors[now.WingColor].Name}");
                }
                if (DescHelp.isChanged(old, now, "WingType"))
                {
                    Console.WriteLine($"Wing Type: {Morphs[old.WingType].Appendages.Wings} -> {Morphs[now.WingType].Appendages.Wings}");
                }
                if (DescHelp.isChanged(old, now, "WingCount"))
                {
                    Console.WriteLine($"Wing Count: {old.WingCount} -> {now.WingCount}");
                }
                if (DescHelp.isChanged(old, now, "WingSize"))
                {
                    Console.WriteLine($"Wing Size: {old.WingSize} -> {now.WingSize}");
                }
            }
            if (changes.Contains("tail")) // 4: TailCount, TailType, TailColor, TailSize
            {
                if (DescHelp.isChanged(old, now, "TailColor"))
                {
                    Console.WriteLine($"Tail Color: {Colors[old.TailColor].Name} -> {Colors[now.TailColor].Name}");
                }
                if (DescHelp.isChanged(old, now, "TailType"))
                {
                    Console.WriteLine($"Tail Type: {Morphs[old.TailType].Appendages.Tail} -> {Morphs[now.TailType].Appendages.Tail}");
                }
                if (DescHelp.isChanged(old, now, "TailCount"))
                {
                    Console.WriteLine($"Tail Count: {old.TailCount} -> {now.TailCount}");
                }
                if (DescHelp.isChanged(old, now, "TailSize"))
                {
                    Console.WriteLine($"Tail Size: {old.TailSize} -> {now.TailSize}");
                }
            }
            if (changes.Contains("skin")) // 8: SkinType, SkinColor, SkinOrnaments, OrnamentColor, ArmCovering, TorsoCovering, LegCovering, CoveringColor
            {
                if (DescHelp.isChanged(old, now, "OrnamentColor"))
                {
                    Console.WriteLine($"Ornament Color: {Colors[old.OrnamentColor].Name} -> {Colors[now.OrnamentColor].Name}");
                }
                if (DescHelp.isChanged(old, now, "CoveringColor"))
                {
                    Console.WriteLine($"Covering Color: {Colors[old.CoveringColor].Name} -> {Colors[now.CoveringColor].Name}");
                }
                if (DescHelp.isChanged(old, now, "SkinColor"))
                {
                    Console.WriteLine($"Skin Color: {Colors[old.SkinColor].Name} -> {Colors[now.SkinColor].Name}");
                }
                if (DescHelp.isChanged(old, now, "SkinType"))
                {
                    Console.WriteLine($"Skin Type: {Skin[old.SkinType].Text} -> {Skin[now.SkinType].Text}");
                }
                if (DescHelp.isChanged(old, now, "SkinOrnament"))
                {
                    Console.WriteLine($"Skin Ornament: {Ornament[old.SkinOrnaments].Name} -> {Ornament[now.SkinOrnaments].Name}");
                }
                if (DescHelp.isChanged(old, now, "ArmCovering"))
                {
                    Console.WriteLine($"Arm Covering: {Skin[old.ArmCovering].Cover} -> {Skin[now.ArmCovering].Cover}");
                }
                if (DescHelp.isChanged(old, now, "LegCovering"))
                {
                    Console.WriteLine($"Leg Covering: {Skin[old.LegCovering].Cover} -> {Skin[now.LegCovering].Cover}");
                }
                if (DescHelp.isChanged(old, now, "TorsoCovering"))
                {
                    Console.WriteLine($"Torso Covering: {Skin[old.TorsoCovering].Cover} -> {Skin[now.TorsoCovering].Cover}");
                }
            }
            if (changes.Contains("ornament")) // OrnamentColor
            {
                if (DescHelp.isChanged(old, now, "OrnamentColor"))
                {
                    Console.WriteLine($"Ornament Color: {Colors[old.OrnamentColor].Name} -> {Colors[now.OrnamentColor].Name}");
                }
            }
            if (changes.Contains("arm")) // ArmColor
            {
                if (DescHelp.isChanged(old, now, "ArmColor"))
                {
                    Console.WriteLine($"Arm Color: {Colors[old.ArmColor].Name} -> {Colors[now.ArmColor].Name}");
                }
            }
            if (changes.Contains("leg")) // LegColor
            {
                if (DescHelp.isChanged(old, now, "LegColor"))
                {
                    Console.WriteLine($"Leg Color: {Colors[old.LegColor].Name} -> {Colors[now.LegColor].Name}");
                }
            }
            if (changes.Contains("neck")) // NeckColor
            {
                if (DescHelp.isChanged(old, now, "NeckColor")){
                    Console.WriteLine($"Neck Color: {Colors[old.NeckColor].Name} -> {Colors[now.NeckColor].Name}");
                }
            }
            if (changes.Contains("covering")) // CoveringColor
            {
                if (DescHelp.isChanged(old, now, "CoveringColor"))
                {
                    Console.WriteLine($"Covering Color: {Colors[old.CoveringColor].Name} -> {Colors[now.CoveringColor].Name}");
                }
            }
            if (changes.Contains("eye")) // EyeColor
            {
                if (DescHelp.isChanged(old, now, "EyeColor")){
                    Console.WriteLine($"Eye Color: {Colors[old.EyeColor].Name} -> {Colors[now.EyeColor].Name}");
                }
            }
            if (changes.Contains("lip")) // LipColor
            {
                if (DescHelp.isChanged(old, now, "LipColor")){
                    Console.WriteLine($"Lip Color: {Colors[old.LipColor].Name} -> {Colors[now.LipColor].Name}");
                }
            }
        }
    }
}
