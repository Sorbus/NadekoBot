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

        private static String[] PronounPossessive = new String[3] { "their", "her", "his" };
        private static String[] PronounSubjective = new String[3] { "they", "she", "he" };
        private static String[] PronounObjective = new String[3] { "them", "her", "him" };

        private static String[] Has = new String[3] { "have", "has", "has" };
        private static String[] Are = new String[3] { "are", "is", "is" };
        private static String[] Was = new String[3] { "were", "was", "was" };

        private static String[] PronounSelf = new String[3] { "themself", "herself", "himself" };

        private static readonly Regex re = new Regex(@"\$(\w+)\$", RegexOptions.Compiled);

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

                str.Append((Ornament[morph.SkinOrnaments].Name != null) ? "$possessive$ $skincolor$ $skintype$ is $skinornament$" : "$subjective$ $has$ $skincolor$ $skintype$");

                if ((Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0) &&
                    (Skin[morph.LegCovering].Cover != null && morph.LegCount > 0) &&
                    (Skin[morph.TorsoCovering].Cover != null))
                {
                    if ((Skin[morph.ArmCovering].Cover == Skin[morph.LegCovering].Cover) ==
                        (Skin[morph.LegCovering].Cover == Skin[morph.TorsoCovering].Cover))
                    { str.Append(", and $possessive$ entire body is $torsocovering$."); }
                    else if (Skin[morph.ArmCovering].Cover == Skin[morph.LegCovering].Cover)
                    { str.Append(", $possessive$ arms and legs are $armcovering$, and $possessive$ torso is $torsocovering$."); }
                    else if (Skin[morph.ArmCovering].Cover == Skin[morph.TorsoCovering].Cover)
                    { str.Append(", $possessive$ arms and torso are $armcovering$, and $possessive$ legs are $legcovering$."); }
                    else if (Skin[morph.LegCovering].Cover == Skin[morph.TorsoCovering].Cover)
                    { str.Append(", $possessive$ legs and torso are $legcovering$, and $possessive$ arms are $armcovering$."); }
                    else
                    { str.Append(", $possessive$ arms are $armcovering$, $possessive$ legs are $legcovering$, and $possessive$ torso is $torsocovering$."); }

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

                if (Morphs[morph.HandModification].Appendages.HandMod != null && Morphs[morph.FeetModification].Appendages.FeetMod != null && morph.LegCount > 0 && morph.ArmCount > 0)
                {
                    if (Morphs[morph.HandModification].Appendages.HandMod == Morphs[morph.FeetModification].Appendages.FeetMod)
                    { str.Append(" $possessive$ $handtype$ and $feettype$ are $handmodification$."); }
                    else
                    { str.Append(" $possessive$ $handtype$ are $handmodification$ and $possessive$ $feettype$ are $feetmodification$."); }
                }
                else if (Morphs[morph.HandModification].Appendages.HandMod != null && morph.ArmCount > 0)
                { str.Append(" $possessive$ $handtype$ are $handmoficiation$."); }
                else if (Morphs[morph.FeetModification].Appendages.FeetMod != null && morph.LegCount > 0)
                { str.Append(" $possessive$ $feettype$ are $feetmodification$."); }

                if (Morphs[morph.WingType].Appendages.Wings != null && Morphs[morph.UpperType].Body.WingAnchor != null && Morphs[morph.TailType].Appendages.Tail != null &&
                    Morphs[morph.LowerType].Body.TailAnchor != null && morph.TailCount > 0 && morph.WingCount > 0)
                { str.Append(" $subjective$ $has$ $wingtype$ $wingposition$ and $tailtype$ $tailposition$"); }
                else if (Morphs[morph.WingType].Appendages.Wings != null && Morphs[morph.UpperType].Body.WingAnchor != null && morph.WingCount > 0)
                { str.Append(" $subjective$ $has$ $wingtype$ $wingposition$."); }
                else if (Morphs[morph.TailType].Appendages.Tail != null && Morphs[morph.LowerType].Body.TailAnchor != null && morph.TailCount > 0)
                { str.Append(" $subjective$ $has$ $tailtype$ $tailposition$."); }

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

                    {"$weight$", DescHelp.getWeight(morph) },
                    {"$morphtype$", DescHelp.getDominantType(morph, Morphs) },

                    {"$subjective$", PronounSubjective[morph.Gender]},
                    {"$has$", Has[morph.Gender]},
                    {"$possessive$", PronounPossessive[morph.Gender]},
                    {"$are$", Are[morph.Gender] },

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
                    {"$wingcount$", BarHelp.NumberToWords(morph.WingCount)},
                    {"$wingposition$", Morphs[morph.UpperType].Body.WingAnchor },
                    {"$wingcolor$", Colors[morph.WingColor].Name },

                    {"$tailtype$", Morphs[morph.TailType].Appendages.Tail},
                    {"$tailsize$", (Morphs[morph.TailType].Appendages.TailSizes != null) ? Morphs[morph.TailType].Appendages.TailSizes[morph.TailSize] : ""},
                    {"$tailcount$", BarHelp.NumberToWords(morph.TailCount)},
                    {"$tailpositon$", Morphs[morph.LowerType].Body.TailAnchor },
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
    }
}
