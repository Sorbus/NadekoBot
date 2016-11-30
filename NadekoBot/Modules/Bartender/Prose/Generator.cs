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

namespace NadekoBot.Modules.Bartender.Prose
{
    class Gen
    {
        public static string GetState(MorphModel morph, Discord.User target)
        {
            if (Helpers.Desc.isBaseline(morph))
            {
                return $"{target.Mention} is a {Helpers.Desc.getWeight(morph)} baseline human.".Replace("  ", " ");
            }
            else
            {

                StringBuilder str = new StringBuilder("$mention$ is a $weight$ $morphtype$", 2000);

                if (_.Morphs[morph.UpperType].Body.UpperType == _.Morphs[morph.LowerType].Body.LowerType && _.Morphs[morph.LowerType].Body.LowerType != null)
                {
                    if (_.Morphs[morph.UpperType].Body.BodyType != null)
                    { str.Append(", $a_bodytype$ $uppertype$."); }
                    else
                    { str.Append("."); }
                }
                else
                {
                    str.Append(". $subjective$ $has$ a $bodytype$ body");
                    if (_.Morphs[morph.UpperType].Body.UpperType != null && _.Morphs[morph.LowerType].Body.LowerType != null)
                    { str.Append(", with $a_uppertype$ upper body and the lower body of $a_lowertype$."); }
                    else if (_.Morphs[morph.UpperType].Body.UpperType != null)
                    { str.Append(", with the upper body of $a_uppertype$."); }
                    else if (_.Morphs[morph.LowerType].Body.LowerType != null)
                    { str.Append(", with the lower body of $a_lowertype$."); }
                    else
                    { str.Append("."); }
                }


                if (morph.LegCount > 0 && morph.ArmCount > 0 && _.Morphs[morph.LowerType].Appendages.Legs != null && _.Morphs[morph.ArmType].Appendages.Arms != null)
                {
                    str.Append(" $subjective$ $has$ $armcount$ $armtype$ and $legcount$ $legtype$");
                    str.Append((_.Morphs[morph.LowerType].Body.LegAnchor != null) ? " $legposition$." : ".");
                }
                else if (morph.LegCount > 0 && _.Morphs[morph.LowerType].Appendages.Legs != null)
                {
                    str.Append(" $subjective$ $has$ $legcount$ $legtype$ $legposition$.");
                }
                else if (morph.ArmCount > 0 && _.Morphs[morph.ArmType].Appendages.Arms != null)
                { str.Append(" $subjective$ $has$ $armcount$ $armtype$."); }
                else if (morph.ArmCount == 0 && morph.LegCount == 0)
                { str.Append(" $subjective$ $has$ neither arms nor legs."); }

                str.Append(" $subjective$ $has$ $a_headtype$ with");
                str.Append((morph.EyeCount > 0) ? " $eyecount$ $eyecolor$ $eyetype$" : " with no eyes");

                if (_.Morphs[morph.HairType].Head.Hair != null)
                {
                    str.Append((_.Colors[morph.LipColor].Name != null) ? ", $lipcolor$ lips, and" : " and");
                    if (morph.HairLength > 0)
                    { str.Append(" $hairlength$ $haircolor$ $hairtype$"); }
                    else { str.Append(" no hair"); }
                }
                else
                { str.Append((_.Colors[morph.LipColor].Name != null) ? " and $lipcolor$ lips." : "."); }

                if (morph.HornCount > 0 && _.Morphs[morph.HornType].Head.Horns != null && _.Morphs[morph.FaceType].Head.HornAnchor != null)
                { str.Append((" $hornanchor$") + ((morph.HornCount > 1) ? "s." : ".")); }
                else { str.Append("."); }

                str.Append(" $subjective$ $has$ $earcount$ $eartype$");
                if (morph.EarCount > 1) { str.Append("s"); }
                str.Append(" $earposition$ $possessive$ head,");
                if (morph.TongueLength > 0)
                { str.Append(" $tonguesize$ $tonguetype$,"); }
                else { str.Append(" no tongue,"); }
                str.Append(" and $teethtype$.\n\n");

                if (_.Skin[morph.SkinType].Text != null)
                {
                    str.Append((_.Ornament[morph.SkinOrnaments].Name != null) ? "$possessive$ $skincolor$ $skintype$ is $skinornament$" : "$subjective$ $has$ $skincolor$ $skintype$");

                    if ((_.Skin[morph.ArmCovering].Cover == _.Skin[morph.LegCovering].Cover) ==
                        (_.Skin[morph.LegCovering].Cover == _.Skin[morph.TorsoCovering].Cover))
                    { str.Append(", and "); }
                    else { str.Append(", "); }
                }

                if ((_.Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0) &&
                    (_.Skin[morph.LegCovering].Cover != null && morph.LegCount > 0) &&
                    (_.Skin[morph.TorsoCovering].Cover != null))
                {
                    if ((_.Skin[morph.ArmCovering].Cover == _.Skin[morph.LegCovering].Cover) ==
                        (_.Skin[morph.LegCovering].Cover == _.Skin[morph.TorsoCovering].Cover))
                    { str.Append("$possessive$ entire body is $torsocovering$."); }
                    else if (_.Skin[morph.ArmCovering].Cover == _.Skin[morph.LegCovering].Cover)
                    { str.Append("$possessive$ arms and legs are $armcovering$, and $possessive$ torso is $torsocovering$."); }
                    else if (_.Skin[morph.ArmCovering].Cover == _.Skin[morph.TorsoCovering].Cover)
                    { str.Append("$possessive$ arms and torso are $armcovering$, and $possessive$ legs are $legcovering$."); }
                    else if (_.Skin[morph.LegCovering].Cover == _.Skin[morph.TorsoCovering].Cover)
                    { str.Append("$possessive$ legs and torso are $legcovering$, and $possessive$ arms are $armcovering$."); }
                    else
                    { str.Append("$possessive$ arms are $armcovering$, $possessive$ legs are $legcovering$, and $possessive$ torso is $torsocovering$."); }

                }
                else if ((_.Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0) &&
                        (_.Skin[morph.LegCovering].Cover != null && morph.LegCount > 0))
                {
                    if (_.Skin[morph.ArmCovering].Cover == _.Skin[morph.LegCovering].Cover)
                    { str.Append(", and $possessive$ arms and legs are $armcovering$."); }
                    else
                    { str.Append(", $possessive$ arms are $armcovering$, and $possessive$ legs are $legcovering$."); }
                }
                else if ((_.Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0) &&
                        (_.Skin[morph.TorsoCovering].Cover != null))
                {
                    if (_.Skin[morph.ArmCovering].Cover == _.Skin[morph.TorsoCovering].Cover)
                    { str.Append(", and $possessive$ arms and torso are $armcovering$."); }
                    else
                    { str.Append(", $possessive$ arms are $armcovering$, and $possessive$ torso is $torsocovering$."); }
                }
                else if ((_.Skin[morph.LegCovering].Cover != null && morph.LegCount > 0) &&
                        (_.Skin[morph.TorsoCovering].Cover != null))
                {
                    if (_.Skin[morph.LegCovering].Cover == _.Skin[morph.TorsoCovering].Cover)
                    { str.Append(", and $possessive$ torso and legs are $legcovering$."); }
                    else
                    { str.Append(", $possessive$ torso is $torsocovering$, and $possessive$ legs are $legcovering$."); }
                }
                else if (_.Skin[morph.ArmCovering].Cover != null && morph.ArmCount > 0)
                { str.Append(", and $possessive$ arms are $armcovering$."); }
                else if (_.Skin[morph.LegCovering].Cover != null && morph.LegCount > 0)
                { str.Append(", and $possessive$ legs are $legcovering."); }
                else if (_.Skin[morph.TorsoCovering].Cover != null)
                { str.Append(", and $possessive$ torso is $torsocovering$."); }
                else { str.Append("."); }

                if (_.Morphs[morph.HandMod].Appendages.HandMod != null && _.Morphs[morph.FeetMod].Appendages.FeetMod != null && morph.LegCount > 0 && morph.ArmCount > 0)
                {
                    if (_.Morphs[morph.HandMod].Appendages.HandMod == _.Morphs[morph.FeetMod].Appendages.FeetMod)
                    { str.Append(" $possessive$ $handtype$ and $feettype$ are $handmodification$."); }
                    else
                    { str.Append(" $possessive$ $handtype$ are $handmodification$ and $possessive$ $feettype$ are $feetmodification$."); }
                }
                else if (_.Morphs[morph.HandMod].Appendages.HandMod != null && morph.ArmCount > 0)
                { str.Append(" $possessive$ $handtype$ are $handmoficiation$."); }
                else if (_.Morphs[morph.FeetMod].Appendages.FeetMod != null && morph.LegCount > 0)
                { str.Append(" $possessive$ $feettype$ are $feetmodification$."); }

                if (_.Morphs[morph.WingType].Appendages.Wings != null && _.Morphs[morph.UpperType].Body.WingAnchor != null && _.Morphs[morph.TailType].Appendages.Tail != null &&
                    _.Morphs[morph.LowerType].Body.TailAnchor != null && morph.TailCount > 0 && morph.WingCount > 0)
                { str.Append(" $subjective$ $has$ $wingcount$ $wingsize$ $wingtype$ $wingposition$ and $tailcount$ $tailsize$ $tailtype$ $tailposition$"); }
                else if (_.Morphs[morph.WingType].Appendages.Wings != null && _.Morphs[morph.UpperType].Body.WingAnchor != null && morph.WingCount > 0)
                { str.Append(" $subjective$ $has$ $wingcount$ $wingsize$ $wingtype$ $wingposition$."); }
                else if (_.Morphs[morph.TailType].Appendages.Tail != null && _.Morphs[morph.LowerType].Body.TailAnchor != null && morph.TailCount > 0)
                { str.Append(" $subjective$ $has$ $tailcount$ $tailsize$ $tailtype$ $tailposition$."); }

                if (_.Morphs[morph.ArmFeature].Appendages.ArmFeature != null && _.Morphs[morph.LegFeature].Appendages.LegFeature != null)
                {
                    if (morph.ArmFeature == morph.LegFeature)
                    { str.Append(" $subjective$ $has$ $bothfeature$."); }
                    else
                    { str.Append(" $subjective$ $has$ $legfeature$ and $armfeature$."); }
                }
                else if (_.Morphs[morph.ArmFeature].Appendages.ArmFeature != null)
                { str.Append(" $subjective$ $has$ $armfeature$."); }
                else if (_.Morphs[morph.LegFeature].Appendages.LegFeature != null)
                { str.Append(" $subjective$ $has$ $legfeature$."); }

                var swapper = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase) {
                    {"$mention$", target.Mention },
                    {"$name$", target.Name },

                    {"$weight$", Helpers.Desc.getWeight(morph) },
                    {"$morphtype$", Helpers.Desc.getDominantType(morph, _.Morphs) },

                    {"$subjective$", _.PronounSubjective[morph.Gender]},
                    {"$objective$", _.PronounObjective[morph.Gender]},
                    {"$possessive$", _.PronounPossessive[morph.Gender]},
                    {"$are$", _.Are[morph.Gender] },
                    {"$has$", _.Has[morph.Gender]},

                    //{"$bodytype$",(morph.UpperType == morph.LowerType) ? _.Morphs[morph.LowerType].Body.BodyType : "tauric " + _.Morphs[morph.LowerType].Body.BodyType },
                    {"$a_uppertype$", (Helpers.Desc.vowelFirst(_.Morphs[morph.UpperType].Body.UpperType) ? "an " : "a ") + _.Morphs[morph.UpperType].Body.UpperType },
                    {"$a_lowertype$", (Helpers.Desc.vowelFirst(_.Morphs[morph.LowerType].Body.LowerType) ? "an " : "a ") + _.Morphs[morph.LowerType].Body.LowerType },
                    {"$uppertype$", _.Morphs[morph.UpperType].Body.UpperType },
                    //{"$a_bodytype$", (Helpers.Desc.vowelFirst(_.Morphs[morph.UpperType].Body.BodyType) ? "an " : "a ") + _.Morphs[morph.LowerType].Body.BodyType },

                    {"$armcount$", Helpers.Bar.NumberToWords(morph.ArmCount) },
                    {"$armtype$", _.Morphs[morph.ArmType].Appendages.Arms },
                    {"$legcount$", Helpers.Bar.NumberToWords(morph.LegCount) },
                    {"$legtype$", _.Morphs[morph.LegType].Appendages.Legs },
                    {"$legposition$", _.Morphs[morph.LowerType].Body.LegAnchor },

                    {"$wingtype$", _.Morphs[morph.WingType].Appendages.Wings},
                    {"$wingsize$", (_.Morphs[morph.WingType].Appendages.WingSizes != null) ? _.Morphs[morph.WingType].Appendages.WingSizes[morph.WingSize] : ""},
                    {"$wingcount$", (morph.WingCount == 1) ? "a" : Helpers.Bar.NumberToWords(morph.WingCount) },
                    {"$wingposition$", _.Morphs[morph.UpperType].Body.WingAnchor },
                    {"$wingcolor$", _.Colors[morph.WingColor].Name },

                    {"$tailtype$", _.Morphs[morph.TailType].Appendages.Tail},
                    {"$tailsize$", (_.Morphs[morph.TailType].Appendages.TailSizes != null) ? _.Morphs[morph.TailType].Appendages.TailSizes[morph.TailSize] : ""},
                    {"$tailcount$", (morph.TailCount == 1) ? "a" : Helpers.Bar.NumberToWords(morph.TailCount) },
                    {"$tailposition$", _.Morphs[morph.LowerType].Body.TailAnchor },
                    {"$tailcolor$", _.Colors[morph.TailColor].Name },

                    {"$headtype$", _.Morphs[morph.FaceType].Head.Head },
                    {"$a_headtype$", (Helpers.Desc.vowelFirst(_.Morphs[morph.FaceType].Head.Head) ? "an " : "a ") + _.Morphs[morph.FaceType].Head.Head },
                    {"$eyetype$", (morph.EyeCount > 1) ? _.Morphs[morph.EyeType].Head.Eyes + "s" : _.Morphs[morph.EyeType].Head.Eyes },
                    {"$eyecount$", Helpers.Bar.NumberToWords(morph.EyeCount) },
                    {"$eyecolor$", _.Colors[morph.EyeColor].Name},
                    {"$tonguetype$", _.Morphs[morph.TongueType].Head.Tongue },
                    {"$tonguesize$", Helpers.Desc.getTongue(morph) },
                    {"$teethtype$", _.Morphs[morph.TeethType].Head.Teeth },
                    {"$lipcolor$", _.Colors[morph.LipColor].Name },

                    {"$earposition$", _.Morphs[morph.EarType].Head.EarAnchor },
                    {"$earcount$", Helpers.Bar.NumberToWords(morph.EarCount) },
                    {"$eartype$", _.Morphs[morph.EarType].Head.Ears },

                    {"$feettype$", _.Morphs[morph.FeetType].Appendages.Feet },
                    {"$handtype$", _.Morphs[morph.HandType].Appendages.Hands },
                    {"$handmodification$", _.Morphs[morph.HandMod].Appendages.HandMod },
                    {"$feetmodification$", _.Morphs[morph.FeetMod].Appendages.FeetMod },

                    {"$torsocovering$", _.Skin[morph.TorsoCovering].Cover },
                    {"$legcovering$", _.Skin[morph.LegCovering].Cover },
                    {"$armcovering$", _.Skin[morph.ArmCovering].Cover },
                    {"$covercolor$", _.Colors[morph.CoveringColor].Name },

                    {"$skintype$", _.Skin[morph.SkinType].Text },
                    {"$skincolor$", _.Colors[morph.SkinColor].Name },
                    {"$skinornament$", _.Ornament[morph.SkinOrnaments].Name},
                    {"$ornamentcolor$", _.Colors[morph.OrnamentColor].Name },

                    {"$hairtype$", _.Morphs[morph.HairType].Head.Hair },
                    {"$haircolor$", _.Colors[morph.HairColor].Name },
                    {"$hairlength$", Helpers.Desc.getHair(morph) },

                    {"$horntype$", _.Morphs[morph.HornType].Head.Horns },
                    {"$hornsize$", (_.Morphs[morph.HornType].Head.HornSizes != null) ? _.Morphs[morph.HornType].Head.HornSizes[morph.HornSize] : ""},
                    {"$hornanchor$", _.Morphs[morph.FaceType].Head.HornAnchor },
                    {"$horncolor$", _.Colors[morph.HornColor].Name },
                    {"$horncount$", Helpers.Bar.NumberToWords(morph.HornCount) },

                    {"$neckfeature$", _.Morphs[morph.NeckFeature].Head.NeckFeature },
                    {"$neckcolor$", _.Colors[morph.NeckColor].Name },
                    {"$armfeature$", _.Morphs[morph.ArmFeature].Appendages.ArmFeature },
                    {"$armcolor$", _.Colors[morph.ArmColor].Name },
                    {"$legfeature$", _.Morphs[morph.LegFeature].Appendages.LegFeature },
                    {"$legcolor$", _.Colors[morph.LegColor].Name },
                    {"$bothfeature$", _.Morphs[morph.ArmFeature].Appendages.BothFeature },

                   
                    
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

        public static Tuple<String, String> GetChange(MorphModel old, MorphModel now, TFDetails tf, List<string> changes)
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
                        { str_3rd.Append(Helpers.Desc.locationName(_.Morphs, old, now, changes.First()) + " "); }
                        else
                        {
                            int c = changes.Count;
                            int i = 0;
                            while (c > 1)
                            {
                                str_3rd.Append(Helpers.Desc.locationName(_.Morphs, old, now, changes[i]) + ", ");
                                c -= 1;
                                i += 1;
                            }
                            str_3rd.Append("and " + Helpers.Desc.locationName(_.Morphs, old, now, changes[i]));
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

        public static void PrintChanges(MorphModel old, MorphModel now, List<String> changes)
        {
            if (changes.Contains("upper")) // 7: ArmType, ArmFeature, ArmColor, ArmCount, HandModification, HandType, UpperType
            {
                if (Helpers.Desc.isChanged(old, now, "ArmColor"))
                {
                    Console.WriteLine($"Arm Color: {_.Colors[old.ArmColor].Name} -> {_.Colors[now.ArmColor].Name}");
                }
                if (Helpers.Desc.isChanged(old, now, "ArmCount"))
                {
                    Console.WriteLine($"Arm Count: {old.ArmCount} -> {now.ArmCount}");
                }
                if (Helpers.Desc.isChanged(old, now, "ArmType"))
                {
                    Console.WriteLine($"Arm Type: {_.Morphs[old.ArmType].Appendages.Arms} -> {_.Morphs[now.ArmType].Appendages.Arms}");
                }
                if (Helpers.Desc.isChanged(old, now, "HandType"))
                {
                    Console.WriteLine($"Hand Type: {_.Morphs[old.HandType].Appendages.Hands} -> {_.Morphs[now.HandType].Appendages.Hands}");
                }
                if (Helpers.Desc.isChanged(old, now, "UpperType"))
                {
                    Console.WriteLine($"Upper Type: {_.Morphs[old.UpperType].Body.UpperType} -> {_.Morphs[now.UpperType].Body.UpperType}");
                }
                if (Helpers.Desc.isChanged(old, now, "ArmFeature"))
                {
                    Console.WriteLine($"Arm Feature: {_.Morphs[old.ArmFeature].Appendages.ArmFeature} -> {_.Morphs[now.ArmFeature].Appendages.ArmFeature}");
                }
                if (Helpers.Desc.isChanged(old, now, "HandtModification"))
                {
                    Console.WriteLine($"Foot Modification: {_.Morphs[old.HandMod].Appendages.HandMod} -> {_.Morphs[now.HandMod].Appendages.HandMod}");
                }
            }
            if (changes.Contains("lower")) // 7: LegType, LegFeature, LegColor, LegCount, FootModification, FootType, LowerType
            {
                if (Helpers.Desc.isChanged(old, now, "LegColor"))
                {
                    Console.WriteLine($"Leg Color: {_.Colors[old.LegColor].Name} -> {_.Colors[now.LegColor].Name}");
                }
                if (Helpers.Desc.isChanged(old, now, "LegCount"))
                {
                    Console.WriteLine($"Leg Count: {old.LegCount} -> {now.LegCount}");
                }
                if (Helpers.Desc.isChanged(old, now, "LegType"))
                {
                    Console.WriteLine($"Leg Type: {_.Morphs[old.LegType].Appendages.Legs} -> {_.Morphs[now.LegType].Appendages.Legs}");
                }
                if (Helpers.Desc.isChanged(old, now, "FeetType"))
                {
                    Console.WriteLine($"Feet Type: {_.Morphs[old.FeetType].Appendages.Feet} -> {_.Morphs[now.FeetType].Appendages.Feet}");
                }
                if (Helpers.Desc.isChanged(old, now, "LowerType"))
                {
                    Console.WriteLine($"Lower Type: {_.Morphs[old.LowerType].Body.LowerType} -> {_.Morphs[now.LowerType].Body.LowerType}");
                }
                if (Helpers.Desc.isChanged(old, now, "LegFeature"))
                {
                    Console.WriteLine($"Leg Feature: {_.Morphs[old.LegFeature].Appendages.LegFeature} -> {_.Morphs[now.LegFeature].Appendages.LegFeature}");
                }
                if (Helpers.Desc.isChanged(old, now, "FeettModification"))
                {
                    Console.WriteLine($"Foot Modification: {_.Morphs[old.FeetMod].Appendages.FeetMod} -> {_.Morphs[now.FeetMod].Appendages.FeetMod}");
                }

            }
            if (changes.Contains("face")) // 8: EyeType, EyeColor, EarType, TongueType, TeethType, LipColor, EyeCount, FaceType
            {
                if (Helpers.Desc.isChanged(old, now, "EyeColor"))
                {
                    Console.WriteLine($"Eye Color: {_.Colors[old.EyeColor].Name} -> {_.Colors[now.EyeColor].Name}");
                }
                if (Helpers.Desc.isChanged(old, now, "EyeType"))
                {
                    Console.WriteLine($"Eye Type: {_.Morphs[old.EyeType].Head.Eyes} -> {_.Morphs[now.EyeType].Head.Eyes}");
                }
                if (Helpers.Desc.isChanged(old, now, "EyeCount"))
                {
                    Console.WriteLine($"Eye Count: {old.EyeCount} -> {now.EyeCount}");
                }
                if (Helpers.Desc.isChanged(old, now, "TongueType"))
                {
                    Console.WriteLine($"Tongue Type: {_.Morphs[old.TongueType].Head.Tongue} -> {_.Morphs[now.TongueType].Head.Tongue}");
                }
                if (Helpers.Desc.isChanged(old, now, "TeethType"))
                {
                    Console.WriteLine($"Teeth Type: {_.Morphs[old.TeethType].Head.Teeth} -> {_.Morphs[now.TeethType].Head.Teeth}");
                }
                if (Helpers.Desc.isChanged(old, now, "FaceType"))
                {
                    Console.WriteLine($"Face Type: {_.Morphs[old.FaceType].Head.Head} -> {_.Morphs[now.FaceType].Head.Head}");
                }
                if (Helpers.Desc.isChanged(old, now, "EarType"))
                {
                    Console.WriteLine($"Ear Type: {_.Morphs[old.EarType].Head.Ears} -> {_.Morphs[now.EarType].Head.Ears}");
                }
                if (Helpers.Desc.isChanged(old, now, "LipColor"))
                {
                    Console.WriteLine($"Lip Color: {_.Colors[old.LipColor].Name} -> {_.Colors[now.LipColor].Name}");
                }
            }
            if (changes.Contains("tongue")) // 4: TongueType, TongueColor, TongueCount, TongueLength
            {
                if (Helpers.Desc.isChanged(old, now, "TongueColor"))
                {
                    Console.WriteLine($"Tongue Color: {_.Colors[old.TongueColor].Name} -> {_.Colors[now.TongueColor].Name}");
                }
                if (Helpers.Desc.isChanged(old, now, "TongueType"))
                {
                    Console.WriteLine($"Tongue Type: {_.Morphs[old.TongueType].Head.Tongue} -> {_.Morphs[now.TongueType].Head.Tongue}");
                }
                if (Helpers.Desc.isChanged(old, now, "TongueCount"))
                {
                    Console.WriteLine($"Tongue Count: {old.TongueCount} -> {now.TongueCount}");
                }
                if (Helpers.Desc.isChanged(old, now, "TongueLength"))
                {
                    Console.WriteLine($"Tongue Size: {old.TongueLength} -> {now.TongueLength}");
                }
            }
            if (changes.Contains("hair")) // 3: HairType, HairColor, HairLength
            {
                if (Helpers.Desc.isChanged(old, now, "HairColor")){
                    Console.WriteLine($"Hair Color: {_.Colors[old.HairColor].Name} -> {_.Colors[now.HairColor].Name}");
                }
                if (Helpers.Desc.isChanged(old, now, "HairType"))
                {
                    Console.WriteLine($"Hair Type: {_.Morphs[old.HairType].Head.Hair} -> {_.Morphs[now.HairType].Head.Hair}");
                }
                if (Helpers.Desc.isChanged(old, now, "HairLength"))
                {
                    Console.WriteLine($"Hair Size: {old.HairLength} -> {now.HairLength}");
                }
            }
            if (changes.Contains("horn")) // 4: HornCount, HornType, HornColor, HornSize
            {
                if (Helpers.Desc.isChanged(old, now, "HornColor")){
                    Console.WriteLine($"Horn Color: {_.Colors[old.HornColor].Name} -> {_.Colors[now.HornColor].Name}");
                }
                if (Helpers.Desc.isChanged(old, now, "HornType"))
                {
                    Console.WriteLine($"Horn Type: {_.Morphs[old.HornType].Head.Horns} -> {_.Morphs[now.HornType].Head.Horns}");
                }
                if (Helpers.Desc.isChanged(old, now, "HornCount"))
                {
                    Console.WriteLine($"Horn Count: {old.HornCount} -> {now.HornCount}");
                }
                if (Helpers.Desc.isChanged(old, now, "HornSize"))
                {
                    Console.WriteLine($"Horn Size: {old.HornSize} -> {now.HornSize}");
                }
            }
            if (changes.Contains("wing")) // 4: WingCount, WingType, WingColor, WingSize
            {
                if (Helpers.Desc.isChanged(old, now, "WingColor"))
                {
                    Console.WriteLine($"Wing Color: {_.Colors[old.WingColor].Name} -> {_.Colors[now.WingColor].Name}");
                }
                if (Helpers.Desc.isChanged(old, now, "WingType"))
                {
                    Console.WriteLine($"Wing Type: {_.Morphs[old.WingType].Appendages.Wings} -> {_.Morphs[now.WingType].Appendages.Wings}");
                }
                if (Helpers.Desc.isChanged(old, now, "WingCount"))
                {
                    Console.WriteLine($"Wing Count: {old.WingCount} -> {now.WingCount}");
                }
                if (Helpers.Desc.isChanged(old, now, "WingSize"))
                {
                    Console.WriteLine($"Wing Size: {old.WingSize} -> {now.WingSize}");
                }
            }
            if (changes.Contains("tail")) // 4: TailCount, TailType, TailColor, TailSize
            {
                if (Helpers.Desc.isChanged(old, now, "TailColor"))
                {
                    Console.WriteLine($"Tail Color: {_.Colors[old.TailColor].Name} -> {_.Colors[now.TailColor].Name}");
                }
                if (Helpers.Desc.isChanged(old, now, "TailType"))
                {
                    Console.WriteLine($"Tail Type: {_.Morphs[old.TailType].Appendages.Tail} -> {_.Morphs[now.TailType].Appendages.Tail}");
                }
                if (Helpers.Desc.isChanged(old, now, "TailCount"))
                {
                    Console.WriteLine($"Tail Count: {old.TailCount} -> {now.TailCount}");
                }
                if (Helpers.Desc.isChanged(old, now, "TailSize"))
                {
                    Console.WriteLine($"Tail Size: {old.TailSize} -> {now.TailSize}");
                }
            }
            if (changes.Contains("skin")) // 8: SkinType, SkinColor, SkinOrnaments, OrnamentColor, ArmCovering, TorsoCovering, LegCovering, CoveringColor
            {
                if (Helpers.Desc.isChanged(old, now, "OrnamentColor"))
                {
                    Console.WriteLine($"Ornament Color: {_.Colors[old.OrnamentColor].Name} -> {_.Colors[now.OrnamentColor].Name}");
                }
                if (Helpers.Desc.isChanged(old, now, "CoveringColor"))
                {
                    Console.WriteLine($"Covering Color: {_.Colors[old.CoveringColor].Name} -> {_.Colors[now.CoveringColor].Name}");
                }
                if (Helpers.Desc.isChanged(old, now, "SkinColor"))
                {
                    Console.WriteLine($"Skin Color: {_.Colors[old.SkinColor].Name} -> {_.Colors[now.SkinColor].Name}");
                }
                if (Helpers.Desc.isChanged(old, now, "SkinType"))
                {
                    Console.WriteLine($"Skin Type: {_.Skin[old.SkinType].Text} -> {_.Skin[now.SkinType].Text}");
                }
                if (Helpers.Desc.isChanged(old, now, "SkinOrnament"))
                {
                    Console.WriteLine($"Skin Ornament: {_.Ornament[old.SkinOrnaments].Name} -> {_.Ornament[now.SkinOrnaments].Name}");
                }
                if (Helpers.Desc.isChanged(old, now, "ArmCovering"))
                {
                    Console.WriteLine($"Arm Covering: {_.Skin[old.ArmCovering].Cover} -> {_.Skin[now.ArmCovering].Cover}");
                }
                if (Helpers.Desc.isChanged(old, now, "LegCovering"))
                {
                    Console.WriteLine($"Leg Covering: {_.Skin[old.LegCovering].Cover} -> {_.Skin[now.LegCovering].Cover}");
                }
                if (Helpers.Desc.isChanged(old, now, "TorsoCovering"))
                {
                    Console.WriteLine($"Torso Covering: {_.Skin[old.TorsoCovering].Cover} -> {_.Skin[now.TorsoCovering].Cover}");
                }
            }
            if (changes.Contains("ornament")) // OrnamentColor
            {
                if (Helpers.Desc.isChanged(old, now, "OrnamentColor"))
                {
                    Console.WriteLine($"Ornament Color: {_.Colors[old.OrnamentColor].Name} -> {_.Colors[now.OrnamentColor].Name}");
                }
            }
            if (changes.Contains("arm")) // ArmColor
            {
                if (Helpers.Desc.isChanged(old, now, "ArmColor"))
                {
                    Console.WriteLine($"Arm Color: {_.Colors[old.ArmColor].Name} -> {_.Colors[now.ArmColor].Name}");
                }
            }
            if (changes.Contains("leg")) // LegColor
            {
                if (Helpers.Desc.isChanged(old, now, "LegColor"))
                {
                    Console.WriteLine($"Leg Color: {_.Colors[old.LegColor].Name} -> {_.Colors[now.LegColor].Name}");
                }
            }
            if (changes.Contains("neck")) // NeckColor
            {
                if (Helpers.Desc.isChanged(old, now, "NeckColor")){
                    Console.WriteLine($"Neck Color: {_.Colors[old.NeckColor].Name} -> {_.Colors[now.NeckColor].Name}");
                }
            }
            if (changes.Contains("covering")) // CoveringColor
            {
                if (Helpers.Desc.isChanged(old, now, "CoveringColor"))
                {
                    Console.WriteLine($"Covering Color: {_.Colors[old.CoveringColor].Name} -> {_.Colors[now.CoveringColor].Name}");
                }
            }
            if (changes.Contains("eye")) // EyeColor
            {
                if (Helpers.Desc.isChanged(old, now, "EyeColor")){
                    Console.WriteLine($"Eye Color: {_.Colors[old.EyeColor].Name} -> {_.Colors[now.EyeColor].Name}");
                }
            }
            if (changes.Contains("lip")) // LipColor
            {
                if (Helpers.Desc.isChanged(old, now, "LipColor")){
                    Console.WriteLine($"Lip Color: {_.Colors[old.LipColor].Name} -> {_.Colors[now.LipColor].Name}");
                }
            }
        }
    }
}
