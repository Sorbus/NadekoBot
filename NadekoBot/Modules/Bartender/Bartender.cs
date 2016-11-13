﻿using Discord.Commands;
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

        private List<TFMorph> Morphs = NadekoBot.Config.Morphs;
        private Dictionary<int, TFColor> Colors = NadekoBot.Config.Colors;
        private Dictionary<int, TFAppendages> Appendages = NadekoBot.Config.Appendages;
        private Dictionary<int, TFBody> Body = NadekoBot.Config.Body;
        private Dictionary<int, TFHead> Head = NadekoBot.Config.Head;
        private Dictionary<int, TFOrnament> Ornament = NadekoBot.Config.Ornament;
        private Dictionary<int, TFSkin> Skin = NadekoBot.Config.Skin;

        private static String[] PronounObjective = new String[3] { "their", "her", "his" };
        private static String[] PronounHas = new String[3] { "have", "has", "has" };
        private static String[] Pronoun = new String[3] { "they", "she", "he" };
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

        private Boolean isBaseline(UserMorph m)
        {
            int[] r = new int[] {m.UpperType, m.LowerType, m.LegType, m.ArmType, m.FaceType, m.EyeType, m.HairType,
                m.EarType, m.TongueType, m.TeethType, m.SkinType, m.SkinOrnaments, m.ArmCovering, m.LegCovering,
                m.TorsoCovering, m.HandType, m.FeetType};
            if (!Array.TrueForAll<int>(r, x => x == 1 ))
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
            //Dictionary<int, int> c = new Dictionary<int, int>();

            
                return "hybrid";
            
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

        private Tuple<UserMorph, String, String> transformUser(UserMorph original, TFMorph target, Drink drink)
        {
            String str_third = "";
            string str_second = "";

            int[] rolls = new int[3] { rng.Next(0, 100), rng.Next(0, 100), rng.Next(0, 100) };

            // modify UpperType
            if (anyInRange(rolls, 0, 10)) { }

            // modify LowerType
            if (anyInRange(rolls, 0, 10)) { }

            // modify legs. Not that legtype only displays if lowertype is null
            // leg count
            // leg type
            // feet type
            // feet modification
            if (anyInRange(rolls, 0, 10)) { }

            // modify arms
            // arm count
            // arm type
            // hand type
            // hand modification
            if (anyInRange(rolls, 0, 10)) { }

            // modify head
            // face type
            // eyes
            // eye color
            // ear type
            // ear count
            // tongue
            // tongue type
            // if not set by drink, bring closer to morph's max
            // hair
            // hair type
            // if not set by drink, bring closer to morph's max
            if (anyInRange(rolls, 0, 10)) { }

            // modify skin
            // skin ornaments
            // arm covering
            // leg covering
            // torso covering
            if (anyInRange(rolls, 0, 10)) { }

            // wings
            // add if lower than morph's maximum
            // expand if at morph's maximum
            // remove if higher than morph's maximum
            if (anyInRange(rolls, 0, 10)) { }

            // tails
            // add if lower than morph's maximum
            // expand if at morph's maximum
            // remove if higher than morph's maximum
            if (anyInRange(rolls, 0, 10)) { }

            // weight



            return Tuple.Create(original, str_third, str_second);
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
                            Dictionary<String, Drink> drink_cat = Drinks.Where(t => t.Cat == e.GetArg("category".ToLowerInvariant())).ToDictionary(x => x.Code, y => y);

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
                        await FlowersHandler.RemoveFlowers(e.User, $"bought a {drink.Code}", amount).ConfigureAwait(false);

                        await e.User.SendMessage($"{drink.Flavor}").ConfigureAwait(false);

                        if (morphs.ContainsKey((long)e.User.Id))
                        {
                            UserMorph morph = morphs[(long)e.User.Id];
                            if (drink.Name != null)
                            { await e.Channel.SendMessage($"{e.User.Mention} bought {PronounSelf[morph.Gender]} {(vowelFirst(drink.Name) ? "an" : "a")} {drink.Name}.").ConfigureAwait(false); }
                            else
                            { await e.Channel.SendMessage($"{e.User.Mention} bought {PronounSelf[morph.Gender]} {(vowelFirst(drink.Code) ? "an" : "a")} {drink.Code}.").ConfigureAwait(false); }
                        }
                        else
                        {
                            if (drink.Name != null)
                            { await e.Channel.SendMessage($"{e.User.Mention} bought {PronounSelf[0]} {(vowelFirst(drink.Name) ? "an" : "a")} {drink.Name}.").ConfigureAwait(false); }
                            else
                            { await e.Channel.SendMessage($"{e.User.Mention} bought {PronounSelf[0]} {(vowelFirst(drink.Code) ? "an" : "a")} {drink.Code}.").ConfigureAwait(false); }
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

                                    String str = "$mention$ is a $weight$$morphtype$. ";

                                    str += "$pronoun$ $has$ a $bodytype$ body";
                                    if (Body[morph.UpperType].UpperType != null && Body[morph.LowerType].LowerType != null)
                                    { str += ", with $a_uppertype$ upper body and the lower body of $a_lowertype$."; }
                                    else if (Body[morph.UpperType].UpperType != null)
                                    { str += ", with the upper body of $a_uppertype$."; }
                                    else if (Body[morph.LowerType].LowerType != null)
                                    { str += ", with the lower body of $a_lowertype$."; }
                                    else
                                    { str += "."; }
                                    

                                    if (morph.LegCount > 0 && morph.ArmCount > 0 && Appendages[morph.LowerType].Legs!= null && Appendages[morph.ArmType].Arms != null)
                                    {
                                        str += " $pronoun$ $has$ $armcount$ $armtype$ and $legcount$ $legtype$";
                                        str += (Body[morph.LowerType].LegAnchor != null) ? " $legposition$." : ".";
                                    }
                                    else if (morph.LegCount > 0 && Appendages[morph.LowerType].Legs != null)
                                    {
                                        str += " $pronoun$ $has$ $legcount$ $legtype$$legposition$.";
                                    }
                                    else if (morph.ArmCount > 0 && Appendages[morph.ArmType].Arms != null)
                                    { str += " $pronoun$ $has$ $armcount$ $armtype$."; }
                                    else if (morph.ArmCount == 0 && morph.LegCount == 0)
                                    { str += " $pronoun$ $has$ neither arms nor legs."; }

                                    str += " $pronoun$ $has$ a $facetype$ with";
                                    str += (morph.EyeCount > 0) ? " $eyecount$ $eyecolor$$eyetype$" : " with no eyes";

                                    str += (Colors[morph.LipColor].Name != null) ? ", $lipcolor$ lips, and" : " and";

                                    if (morph.HairLength > 0)
                                    { str += " $hairlength$ $haircolor$$hairtype$."; }
                                    else { str += " no hair."; }

                                    str += " $pronoun$ $has$ $earcount$ $eartype$";
                                    if (morph.EarCount > 1) { str += "s"; }
                                    str += " $earposition$ $objective$ head,";
                                    if (morph.TongueLength > 0)
                                    { str += " $tonguesize$ $tonguetype$,"; }
                                    else { str += " no tongue,"; }
                                    str += " and $teethtype$.\n\n";

                                    str += (Ornament[morph.SkinOrnaments].Name != null) ? "$objective$ $skincolor$$skintype$ is $skinornament$" : "$pronoun$ $has$ $skincolor$$skintype$";

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

                                    if (Appendages[morph.HandModification].HandMod != null && Appendages[morph.FeetModification].FeetMod != null && morph.LegCount > 0 && morph.ArmCount > 0)
                                    {
                                        if (Appendages[morph.HandModification].HandMod == Appendages[morph.FeetModification].FeetMod)
                                        { str += " $objective$ $handtype$ and $feettype$ are $handmodification$."; }
                                        else
                                        { str += " $objective$ $handtype$ are $handmodification$ and $objective$ $feettype$ are $feetmodification$."; }
                                    }
                                    else if (Appendages[morph.HandModification].HandMod != null && morph.ArmCount > 0)
                                    { str += " $objective$ $handtype$ are $handmoficiation$."; }
                                    else if (Appendages[morph.FeetModification].FeetMod != null && morph.LegCount > 0)
                                    { str += " $objective$ $feettype$ are $feetmodification$."; }

                                    if (Body[morph.UpperType].WingAnchor != null && Body[morph.LowerType].TailAnchor != null && morph.TailCount > 0 && morph.WingCount > 0)
                                    { str += " $pronoun$ $has$ $wingcount$ $wingsize$ $wingcolor$$wingtype$ $wingposition$ and $tailcount$ $tailsize$ $tailcolor$$tailtype$ $tailposition$"; }
                                    else if (Body[morph.UpperType].WingAnchor != null && morph.WingCount > 0)
                                    { str += " $pronoun$ $has$ $wingcount$ $wingsize$ $wingcolor$$wingtype$ $wingposition$."; }
                                    else if (Body[morph.LowerType].TailAnchor != null && morph.TailCount > 0)
                                    { str += " $pronoun$ $has$ $tailcount$ $tailsize$ $tailcolor$$tailtype% $tailposition$."; }

                                    var swapper = new Dictionary<string, string>(
                                        StringComparer.OrdinalIgnoreCase) {
                                            {"$mention$", target.Mention },

                                            {"$weight$", getWeight(morph) },
                                            {"$morphtype$", getDominantType(morph) },

                                            {"$pronoun$", Pronoun[morph.Gender]},
                                            {"$has$", PronounHas[morph.Gender]},
                                            {"$objective$", PronounObjective[morph.Gender]},

                                            {"$bodytype$",(morph.UpperType == morph.LowerType) ? Body[morph.LowerType].BodyType : "tauric " + Body[morph.LowerType].BodyType },
                                            {"$a_uppertype$", (vowelFirst(Body[morph.UpperType].UpperType) ? "an " : "a ") + Body[morph.UpperType].UpperType },
                                            {"$a_lowertype$", (vowelFirst(Body[morph.LowerType].LowerType) ? "an " : "a ") + Body[morph.LowerType].LowerType },

                                            {"$armcount$", NumberToWords(morph.ArmCount) },
                                            {"$armtype$", Appendages[morph.ArmType].Arms },
                                            {"$legcount$", NumberToWords(morph.LegCount) },
                                            {"$legtype$", Appendages[morph.LegType].Legs },
                                            {"$legposition$", (Body[morph.LowerType].LegAnchor != null) ? " " + Body[morph.LowerType].LegAnchor : "" },

                                            {"$wingtype$", Appendages[morph.WingType].Wings},
                                            {"$wingsize$", (Appendages[morph.WingType].WingSizes != null) ? Appendages[morph.WingType].WingSizes[morph.WingSize] : ""},
                                            {"$wingcount$", NumberToWords(morph.WingCount)},
                                            {"$wingposition$", Body[morph.UpperType].WingAnchor },
                                            {"$wingcolor$", (Colors[morph.WingColor] != null) ? (Colors[morph.WingColor].Name + " ") : "" },

                                            {"$tailtype$", Appendages[morph.TailType].Tail},
                                            {"$tailsize$", (Appendages[morph.TailType].TailSizes != null) ? Appendages[morph.TailType].TailSizes[morph.TailSize] : ""},
                                            {"$tailcount$", NumberToWords(morph.TailCount)},
                                            {"$tailpositon$", Body[morph.LowerType].TailAnchor },
                                            {"$tailcolor$", (Colors[morph.TailColor] != null) ? (Colors[morph.TailColor].Name + " ") : ""},

                                            {"$facetype$", Head[morph.FaceType].Head },
                                            {"$eyetype$", (morph.EyeCount > 1) ? Head[morph.EyeType].Eyes + "s" : Head[morph.EyeType].Eyes },
                                            {"$eyecount$", NumberToWords(morph.EyeCount) },
                                            {"$eyecolor$", (Colors[morph.EyeColor].Name != null) ? Colors[morph.EyeColor].Name + " " : "" },
                                            {"$tonguetype$", Head[morph.TongueType].Tongue },
                                            {"$tonguesize$", getTongue(morph) },
                                            {"$teethtype$", Head[morph.TeethType].Teeth },
                                            {"$lipcolor$", Colors[morph.LipColor].Name },

                                            {"$earposition$", Head[morph.EarType].EarAnchor },
                                            {"$earcount$", NumberToWords(morph.EarCount) },
                                            {"$eartype$", Head[morph.EarType].Ears },

                                            {"$feettype$", Appendages[morph.FeetType].Feet },
                                            {"$handtype$", Appendages[morph.HandType].Hands },
                                            {"$handmodification$", Appendages[morph.HandModification].HandMod },
                                            {"$feetmodification$", Appendages[morph.FeetModification].FeetMod },

                                            {"$torsocovering$", Skin[morph.TorsoCovering].Cover },
                                            {"$legcovering$", Skin[morph.LegCovering].Cover },
                                            {"$armcovering$", Skin[morph.ArmCovering].Cover },
                                            {"$skintype$", Skin[morph.SkinType].Text },
                                            {"$skincolor$", (Colors[morph.SkinColor] != null) ? (Colors[morph.SkinColor].Name + " ") : "" },
                                            {"$skinornament$", Ornament[morph.SkinOrnaments].Name},
                                            {"$ornamentcolor$", Colors[morph.OrnamentColor].Name },

                                            {"$hairtype$", Head[morph.HairType].Hair },
                                            {"$haircolor$", (Colors[morph.HairColor] != null) ? (Colors[morph.HairColor].Name + " ") : "" },
                                            {"$hairlength$", getHair(morph) },

                                            {"  ", " "},
                                        };

                                    // first pass
                                    str = swapper.Aggregate(str, (current, value) => current.Replace(value.Key, value.Value));
                                    // second pass
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
                                DbHandler.Instance.Connection.Insert(new UserMorph
                                {
                                    UserId = (long)e.User.Id,

                                    Gender = gender_marker,
                                    // default morph is a standard human
                                    LegCount = 2,
                                    ArmCount = 2,
                                    HairLength = rng.Next(1, 8),
                                    EarCount = 2,
                                    TongueLength = rng.Next(3, 5),
                                    EyeCount = 2,
                                    MorphCount = 0,

                                }, typeof(UserMorph));

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

                        TFMorph target_morph;

                        target_morph = Morphs.FirstOrDefault(x => x.Code == e.GetArg("morph_type").ToLowerInvariant());

                        if (target_morph == null)
                        {
                            await e.Channel.SendMessage($"That's not a valid morph, {e.User.Mention}.").ConfigureAwait(false);
                            return;
                        }

                        try
                        {
                            var db = DbHandler.Instance.GetAllRows<UserMorph>();
                            Dictionary<long, UserMorph> morphs = db.Where(t => t.UserId.Equals((long)e.User.Id)).ToDictionary(x => x.UserId, y => y);

                            UserMorph morph;
                            if (morphs.ContainsKey((long)e.User.Id))
                            { morph = morphs[(long)e.User.Id]; }
                            else
                            {
                                morph = new UserMorph
                                {
                                    UserId = (long)e.User.Id,
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
                                    TailSize = 3
                                };
                            }

                            morph.UpperType = target_morph.UpperType;
                            morph.LowerType = target_morph.LowerType;

                            morph.LegType = target_morph.LegType;
                            morph.ArmType = target_morph.ArmType;

                            morph.FaceType = target_morph.FaceType;
                            morph.EyeType = target_morph.EyeType;
                            morph.EyeColor = target_morph.EyeColor[0];
                            morph.HairType = target_morph.HairType;
                            morph.HairColor = target_morph.HairColor[0];
                            morph.EarType = target_morph.EarType;

                            morph.TongueType = target_morph.TongueType;
                            morph.TeethType = target_morph.TeethType;

                            morph.SkinType = target_morph.SkinType;
                            morph.SkinColor = target_morph.SkinColor[0];
                            morph.SkinOrnaments = target_morph.Ornaments[0];
                            morph.OrnamentColor = target_morph.OrnamentColor[0];

                            morph.ArmCovering = target_morph.SkinCovering[0];
                            morph.TorsoCovering = target_morph.SkinCovering[0];
                            morph.LegCovering = target_morph.SkinCovering[0];

                            morph.HandModification = target_morph.HandModification[0];
                            morph.FeetModification = target_morph.FeetModification[0];
                            morph.HandType = target_morph.HandType;
                            morph.FeetType = target_morph.FeetType;

                            morph.WingType = target_morph.WingType;
                            morph.TailType = target_morph.TailType;
                            morph.TailColor = target_morph.TailColor[0];
                            morph.WingColor = target_morph.WingColor[0];

                            morph.LegCount = target_morph.MaxLegs;
                            morph.ArmCount = target_morph.MaxArms;
                            morph.WingCount = target_morph.MaxWings;
                            morph.TailCount = target_morph.MaxTails;
                            morph.HairLength = target_morph.MaxHair;
                            morph.EarCount = target_morph.MaxEars;
                            morph.TongueLength = target_morph.MaxTongueSize;
                            morph.TongueCount = target_morph.MaxTongueCount;
                            morph.EyeCount = target_morph.MaxEyes;

                            morph.MorphCount += 1;

                            DbHandler.Instance.Save(morph);

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
