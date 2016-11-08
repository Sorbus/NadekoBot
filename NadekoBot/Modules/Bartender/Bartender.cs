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

namespace NadekoBot.Modules.Bartender
{
    class Bartender : DiscordModule
    {
        public override string Prefix { get; } = NadekoBot.Config.CommandPrefixes.Bartender;

        public Random rng = new Random();

        // private ConcurrentDictionary<ulong, UserMorph> Morphs = new ConcurrentDictionary<ulong, UserMorph>();

        private List<TFMorph> Valid = NadekoBot.Config.ValidMorphs;
        private List<BarDrink> Drinks = NadekoBot.Config.DrinkMenu;
        private String[] PronounObjective = new String[3]  { "their", "her", "his" };
        private String[] PronounHas = new String[3]  { "have", "has", "has" };
        private String[] Pronoun = new String[3] { "they", "she", "he" };
        private String[] PronounSelf = new String[3] { "themself", "herself", "himself" };

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
            if (0 != m.BodyType + m.UpperType + m.LowerType + m.LegType + m.ArmType + m.FaceType +
                m.HairType + m.EarType + m.TongueType + m.TeethType + m.SkinType + m.SkinOrnamentsMorph +
                m.ArmCovering + m.LegCovering + m.TorsoCovering + m.HandModification + m.FeetModification +
                m.HandType + m.FeetType + m.WingType + m.TailType)
            {
                return false;
            }

            if (m.LegCount != 2 || m.ArmCount != 2 || m.EarCount != 2 || m.EyeCount != 2)
            {
                return false;
            }

            return true;
        }

        private string getDominantType(UserMorph m)
        {
            Dictionary<int, int> c = new Dictionary<int, int>();
            if (c.ContainsKey(m.BodyType)) { c[m.BodyType] += 1; }
            else { c[m.BodyType] = 1; }
            if (c.ContainsKey(m.UpperType)) { c[m.UpperType] += 1; }
            else { c[m.UpperType] = 1; }
            if (c.ContainsKey(m.LowerType)) { c[m.LowerType] += 1; }
            else { c[m.LowerType] = 1; }
            if (c.ContainsKey(m.LegType)) { c[m.LegType] += 1; }
            else { c[m.LegType] = 1; }
            if (c.ContainsKey(m.ArmType)) { c[m.ArmType] += 1; }
            else { c[m.ArmType] = 1; }
            if (c.ContainsKey(m.FaceType)) { c[m.FaceType] += 1; }
            else { c[m.FaceType] = 1; }
            if (c.ContainsKey(m.HairType)) { c[m.HairType] += 1; }
            else { c[m.HairType] = 1; }
            if (c.ContainsKey(m.EarType)) { c[m.EarType] += 1; }
            else { c[m.EarType] = 1; }
            if (c.ContainsKey(m.TongueType)) { c[m.TongueType] += 1; }
            else { c[m.TongueType] = 1; }
            if (c.ContainsKey(m.TeethType)) { c[m.TeethType] += 1; }
            else { c[m.TeethType] = 1; }
            if (c.ContainsKey(m.SkinType)) { c[m.SkinType] += 1; }
            else { c[m.SkinType] = 1; }
            if (c.ContainsKey(m.SkinOrnamentsMorph)) { c[m.SkinOrnamentsMorph] += 1; }
            else { c[m.SkinOrnamentsMorph] = 1; }
            if (c.ContainsKey(m.ArmCovering)) { c[m.ArmCovering] += 1; }
            else { c[m.ArmCovering] = 1; }
            if (c.ContainsKey(m.LegCovering)) { c[m.LegCovering] += 1; }
            else { c[m.LegCovering] = 1; }
            if (c.ContainsKey(m.TorsoCovering)) { c[m.TorsoCovering] += 1; }
            else { c[m.TorsoCovering] = 1; }
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
            c.OrderByDescending(t => t.Value);

            if (c.First().Value >= 19)
            {
                return Valid[c.First().Key].Name;
            }
            else if (c.First().Value > 14)
            {
                if (c.First().Key == 0) { return "modified human"; }
                return $"hybridized {Valid[c.First().Key].Name}";
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

        public override void Install(ModuleManager manager)
        {
            manager.CreateCommands("", cgb =>
            {
                cgb.AddCheck(PermissionChecker.Instance);

                commands.ForEach(cmd => cmd.Init(cgb));

                cgb.CreateCommand(Prefix + "menu")
                    .Description($"List items in one of the drink menu's categories. | `{Prefix}menu \"beer\"`")
                    .Parameter("move", ParameterType.Required)
                    .Parameter("category", ParameterType.Unparsed)
                    .Do(async e =>
                    {
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
                        BarDrink drink;

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

                        BarDrink drink;

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
                                    await e.Channel.SendMessage($"{target.Mention} is a baseline human.").ConfigureAwait(false);
                                }
                                else
                                {
                                    await e.Channel.SendIsTyping();
                                    // await e.Channel.SendMessage($"{target.Mention} has modifications.").ConfigureAwait(false);

                                    String str = $"{target.Mention} is a {getDominantType(morph)}. ";

                                    str += $"{Pronoun[morph.Gender]} {PronounHas[morph.Gender]} a {Valid[morph.BodyType].BodyType} body";
                                    if (Valid[morph.UpperType].UpperType != null && Valid[morph.LowerType].LowerType != null)
                                    {
                                        str += $", with {(vowelFirst(Valid[morph.UpperType].UpperType) ? "an" : "a")} {Valid[morph.UpperType].UpperType}" +
                                                $" upper body and the lower body of { (vowelFirst(Valid[morph.LowerType].LowerType) ? "an" : "a")}" + 
                                                $" { Valid[morph.LowerType].LowerType}.";
                                        //  which merge seamlessly together at { PronounObjective[morph.Gender]} waist.
                                    }
                                    else if (Valid[morph.UpperType].UpperType != null)
                                    {
                                        str += $", with the upper body of {(vowelFirst(Valid[morph.UpperType].UpperType) ? "an" : "a")} {Valid[morph.UpperType].UpperType}.";
                                    }
                                    else if (Valid[morph.LowerType].LowerType != null)
                                    {
                                        str += $", with the lower body of {(vowelFirst(Valid[morph.LowerType].LowerType) ? "an" : "a")} {Valid[morph.LowerType].LowerType}.";
                                    }
                                    else
                                    { str += "."; }

                                    if (morph.LegCount > 0 && morph.ArmCount > 0 && Valid[morph.LegType].LegType != null && Valid[morph.ArmType].ArmType != null)
                                    {
                                        str += $" {Pronoun[morph.Gender]} {PronounHas[morph.Gender]} {NumberToWords(morph.ArmCount)} {Valid[morph.ArmType].ArmType}" +
                                            $" and {NumberToWords(morph.LegCount)} {Valid[morph.LegType].LegType}";
                                        if (Valid[morph.LowerType].LowerType != null)
                                        {
                                            str += $" {Valid[morph.LowerType].LegPosition[0]} {PronounObjective[morph.Gender]} {Valid[morph.LowerType].LegPosition[1]}.";
                                        }
                                        else { str += ".";  }
                                    }
                                    else if (morph.LegCount > 0 && Valid[morph.LegType].LegType != null)
                                    {
                                        str += $" {Pronoun[morph.Gender]} {PronounHas[morph.Gender]} {NumberToWords(morph.LegCount)} {Valid[morph.LegType].LegType}";
                                        if (Valid[morph.LowerType].LowerType != null)
                                        {
                                            str += $" {Valid[morph.LowerType].LegPosition[0]} {PronounObjective[morph.Gender]} {Valid[morph.LowerType].LegPosition[1]}.";
                                        }
                                        else { str += "."; }
                                    }
                                    else if (morph.ArmCount > 0 && Valid[morph.ArmType].ArmType != null)
                                    {
                                        str += $" {Pronoun[morph.Gender]} {PronounHas[morph.Gender]} {NumberToWords(morph.ArmCount)} {Valid[morph.ArmType].ArmType}.";
                                    }
                                    else if (morph.ArmCount == 0 && morph.LegCount == 0)
                                    { str += $" {Pronoun[morph.Gender]} has neither arms nor legs."; }

                                    str += $" {Pronoun[morph.Gender]} has a {Valid[morph.FaceType].FaceType} with";
                                    if (morph.EyeCount > 2)
                                    {
                                        str += $" {NumberToWords(morph.EyeCount)}";
                                        if (Valid[morph.FaceType].EyeColor[morph.EyeColor] != null)
                                        { str += $" {Valid[morph.FaceType].EyeColor[morph.EyeColor]}"; }
                                        str += " eyes and";
                                    }
                                    else if (morph.EyeCount == 0) { str += " with no eyes"; }

                                    if (morph.HairLength > 0)
                                    {
                                        if (morph.HairLength < 2) { str += " buzzcut"; }
                                        else if (morph.HairLength < 5) { str += " short"; }
                                        else if (morph.HairLength < 9) { str += " modest"; }
                                        else if (morph.HairLength < 13) { str += " shoulder-length"; }
                                        else if (morph.HairLength < 28) { str += " chest-length"; }
                                        else if (morph.HairLength < 35) { str += " waist-length"; }
                                        else if (morph.HairLength < 40) { str += " butt-length"; }
                                        else if (morph.HairLength < 48) { str += " thigh-length"; }
                                        else if (morph.HairLength < 52) { str += " knee-length"; }
                                        else if (morph.HairLength < 70) { str += " ankle-length"; }
                                        else if (morph.HairLength < 74) { str += " floor-length"; }
                                        else { str += " rapunzelesque"; }

                                        str += $" {Valid[morph.HairType].HairColor[morph.HairColor]} {Valid[morph.HairType].HairType}.";
                                    }
                                    else { str += " no hair."; }

                                    str += $" {Pronoun[morph.Gender]} {PronounHas[morph.Gender]} {NumberToWords(morph.EarCount)} " +
                                        $" {Valid[morph.EarType].EarType}";
                                    if (morph.EarCount > 0) { str += "s"; }
                                    str += $" {Valid[morph.EarType].EarPosition} {PronounObjective[morph.Gender]} head,";
                                    if (morph.TongueLength > 0)
                                    {
                                        if (morph.TongueLength < 3) { str += " a stubby"; }
                                        else if (morph.TongueLength < 4) { str += " a short"; }
                                        else if (morph.TongueLength < 5) { str += " an average"; }
                                        else if (morph.TongueLength < 8) { str += " a long"; }
                                        else if (morph.TongueLength < 12) { str += " a very long"; }
                                        else { str += " an obscenely long"; }
                                        str += $" {Valid[morph.TongueType].TongueType},";
                                    }
                                    else { str += " no tongue,";  }
                                    str += $" and {Valid[morph.TeethType].TeethType}.\n\n";

                                    if (Valid[morph.SkinOrnamentsMorph].SkinOrnaments != null)
                                    {
                                        str += $"{PronounObjective[morph.Gender]} {Valid[morph.SkinType].SkinType} is {Valid[morph.SkinOrnamentsMorph].SkinOrnaments[morph.SkinOrnaments]}";
                                    }
                                    else
                                    {
                                        str += $"{Pronoun[morph.Gender]} has {Valid[morph.SkinType].SkinType}"; 
                                    }

                                    if ((Valid[morph.ArmCovering].SkinCovering != null && morph.ArmCount > 0) &&
                                        (Valid[morph.LegCovering].SkinCovering != null && morph.LegCount > 0) &&
                                        (Valid[morph.TorsoCovering].SkinCovering != null))
                                    {
                                        if ((Valid[morph.ArmCovering].SkinCovering == Valid[morph.LegCovering].SkinCovering) == 
                                            (Valid[morph.LegCovering].SkinCovering == Valid[morph.TorsoCovering].SkinCovering))
                                        {
                                            str += $", and {PronounObjective[morph.Gender]} entire body is {Valid[morph.ArmCovering].SkinCovering}.";
                                        }
                                        else if (Valid[morph.ArmCovering].SkinCovering == Valid[morph.LegCovering].SkinCovering)
                                        {
                                            str += $", {PronounObjective[morph.Gender]} arms and legs are {Valid[morph.ArmCovering].SkinCovering}" +
                                                $", and {PronounObjective[morph.Gender]} torso is {Valid[morph.TorsoCovering].SkinCovering}.";
                                        }
                                        else if (Valid[morph.ArmCovering].SkinCovering == Valid[morph.TorsoCovering].SkinCovering)
                                        {
                                            str += $", {PronounObjective[morph.Gender]} arms and torso are {Valid[morph.ArmCovering].SkinCovering}" +
                                                $", and {PronounObjective[morph.Gender]} legs are {Valid[morph.LegCovering].SkinCovering}.";
                                        }
                                        else if (Valid[morph.LegCovering].SkinCovering == Valid[morph.TorsoCovering].SkinCovering)
                                        {
                                            str += $", {PronounObjective[morph.Gender]} legs and torso are {Valid[morph.LegCovering].SkinCovering}" +
                                                $", and {PronounObjective[morph.Gender]} arms are {Valid[morph.ArmCovering].SkinCovering}.";
                                        }
                                        else
                                        {
                                            str += $", {PronounObjective[morph.Gender]} arms are {Valid[morph.ArmCovering].SkinCovering}" +
                                                $", {PronounObjective[morph.Gender]} legs are {Valid[morph.LegCovering].SkinCovering}" +
                                                $", and {PronounObjective[morph.Gender]} torso is {Valid[morph.TorsoCovering].SkinCovering}.";
                                        }

                                    }
                                    else if ((Valid[morph.ArmCovering].SkinCovering != null && morph.ArmCount > 0) &&
                                            (Valid[morph.LegCovering].SkinCovering != null && morph.LegCount > 0))
                                    {
                                        if (Valid[morph.ArmCovering].SkinCovering == Valid[morph.LegCovering].SkinCovering)
                                        {
                                            str += $", and {PronounObjective[morph.Gender]} arms and legs are {Valid[morph.ArmCovering].SkinCovering}.";
                                        }
                                        else
                                        {
                                            str += $", {PronounObjective[morph.Gender]} arms are {Valid[morph.ArmCovering].SkinCovering}, and " +
                                            $"{PronounObjective[morph.Gender]} legs are {Valid[morph.LegCovering].SkinCovering}.";
                                        }
                                    }
                                    else if ((Valid[morph.ArmCovering].SkinCovering != null && morph.ArmCount > 0) &&
                                            (Valid[morph.TorsoCovering].SkinCovering != null))
                                    {
                                        if (Valid[morph.ArmCovering].SkinCovering == Valid[morph.TorsoCovering].SkinCovering)
                                        {
                                            str += $", and {PronounObjective[morph.Gender]} arms and torso are {Valid[morph.ArmCovering].SkinCovering}.";
                                        }
                                        else
                                        {
                                            str += $", {PronounObjective[morph.Gender]} arms are {Valid[morph.ArmCovering].SkinCovering}, and " +
                                            $"{PronounObjective[morph.Gender]} torso is {Valid[morph.TorsoCovering].SkinCovering}.";
                                        }
                                    }
                                    else if ((Valid[morph.LegCovering].SkinCovering != null && morph.LegCount > 0) &&
                                            (Valid[morph.TorsoCovering].SkinCovering != null))
                                    {
                                        if (Valid[morph.LegCovering].SkinCovering == Valid[morph.TorsoCovering].SkinCovering)
                                        {
                                            str += $", and {PronounObjective[morph.Gender]} torso and legs are {Valid[morph.LegCovering].SkinCovering}.";
                                        }
                                        else
                                        {
                                            str += $", {PronounObjective[morph.Gender]} torso is {Valid[morph.TorsoCovering].SkinCovering}, and " +
                                            $"{PronounObjective[morph.Gender]} legs are {Valid[morph.LegCovering].SkinCovering}.";
                                        }
                                    }
                                    else if (Valid[morph.ArmCovering].SkinCovering != null && morph.ArmCount > 0)
                                    { str += $", and {PronounObjective[morph.Gender]} arms are {Valid[morph.TorsoCovering].SkinCovering}."; }
                                    else if (Valid[morph.LegCovering].SkinCovering != null && morph.LegCount > 0)
                                    { str += $", and {PronounObjective[morph.Gender]} legs are {Valid[morph.TorsoCovering].SkinCovering}."; }
                                    else if (Valid[morph.TorsoCovering].SkinCovering != null)
                                    { str += $", and {PronounObjective[morph.Gender]} torso is {Valid[morph.TorsoCovering].SkinCovering}."; }

                                    if (Valid[morph.HandModification].HandModification != null && Valid[morph.FeetModification].FeetModification != null && morph.LegCount > 0 && morph.ArmCount > 0)
                                    {
                                        if (Valid[morph.HandModification].HandModification == Valid[morph.FeetModification].FeetModification)
                                        {
                                            str += $" {PronounObjective[morph.Gender]} {Valid[morph.HandType].HandType} and {Valid[morph.FeetType].FeetType} are {Valid[morph.FeetModification].FeetModification}.";
                                        }
                                        else
                                        {
                                            str += $" {PronounObjective[morph.Gender]} {Valid[morph.HandType].HandType} are {Valid[morph.HandModification].HandModification} and" +
                                                $" {PronounObjective[morph.Gender]} {Valid[morph.FeetType].FeetType} are {Valid[morph.FeetModification].FeetModification}.";
                                        }
                                    }
                                    else if (Valid[morph.HandModification].HandModification != null && morph.ArmCount > 0)
                                    { str += $" {PronounObjective[morph.Gender]} {Valid[morph.HandType].HandType} are {Valid[morph.HandModification].HandModification}.";  }
                                    else if (Valid[morph.FeetModification].FeetModification != null && morph.LegCount > 0)
                                    { str += $" {PronounObjective[morph.Gender]} {Valid[morph.FeetType].FeetType} are {Valid[morph.FeetModification].FeetModification}."; }

                                    if (Valid[morph.UpperType].WingPosition != null && Valid[morph.LowerType].TailPosition != null && morph.TailCount > 0 && morph.WingCount > 0)
                                    {
                                        str += $" {Pronoun[morph.Gender]} has {NumberToWords(morph.WingCount)} {Valid[morph.WingType].WingSize[morph.WingSize]} {Valid[morph.WingType].WingType}" + 
                                            $" {Valid[morph.UpperType].WingPosition[0]}{PronounObjective[morph.Gender]} {Valid[morph.UpperType].WingPosition[1]} and" +
                                            $" {NumberToWords(morph.TailCount)} {Valid[morph.TailType].TailSize[morph.TailSize]} {Valid[morph.TailType].TailType}" +
                                            $" {Valid[morph.LowerType].TailPosition[0]}{PronounObjective[morph.Gender]} {Valid[morph.LowerType].TailPosition[1]}.";
                                    }
                                    else if (Valid[morph.UpperType].WingPosition != null && morph.WingCount > 0)
                                    {
                                        str += $" {Pronoun[morph.Gender]} has {NumberToWords(morph.WingCount)} {Valid[morph.WingType].WingSize[morph.WingSize]} {Valid[morph.WingType].WingType}" +
                                           $" {Valid[morph.UpperType].WingPosition[0]}{PronounObjective[morph.Gender]} {Valid[morph.UpperType].WingPosition[1]}.";
                                    }
                                    else if (Valid[morph.LowerType].TailPosition != null && morph.TailCount > 0)
                                    {
                                        str += $" {Pronoun[morph.Gender]} has {NumberToWords(morph.TailCount)} {Valid[morph.TailType].WingSize[morph.TailSize]} {Valid[morph.TailType].TailType}" +
                                            $" {Valid[morph.LowerType].TailPosition[0]}{PronounObjective[morph.Gender]} {Valid[morph.LowerType].TailPosition[1]}.";
                                    }

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
                        if(e.GetArg("gender").ToLowerInvariant() == "female")
                        {
                            gender_marker = 1;
                        }
                        else if(e.GetArg("gender").ToLowerInvariant() == "male")
                        {
                            gender_marker = 2;
                        }
                        else if(e.GetArg("gender").ToLowerInvariant() == "neutral")
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
                                    BodyType = 0,
                                    UpperType = 0,
                                    LowerType = 0,
                                    LegType = 0,
                                    ArmType = 0,
                                    FaceType = 0,
                                    EyeColor = 0,
                                    HairType = 0,
                                    HairColor = 0,
                                    EarType = 0,
                                    TongueType = 0,
                                    TeethType = 0,
                                    SkinType = 0,
                                    SkinOrnamentsMorph = 0,
                                    SkinOrnaments = 0,
                                    // SkinCovering = 0,
                                    ArmCovering = 0,
                                    TorsoCovering = 0,
                                    LegCovering = 0,
                                    HandModification = 0,
                                    FeetModification = 0,
                                    HandType = 0,
                                    FeetType = 0,
                                    WingType = 0,
                                    TailType = 0,
                                    LegCount = 2,
                                    ArmCount = 2,
                                    WingCount = 0,
                                    TailCount = 0,
                                    WingSize = 0,
                                    TailSize = 0,
                                    HairLength = rng.Next(1, 8),
                                    EarCount = 2,
                                    TongueLength = rng.Next(3, 5),
                                    EyeCount = 2,
                                    MorphCount = 0

                                }, typeof(UserMorph));

                                await e.Channel.SendMessage($"Set your gender to {e.GetArg("gender").ToUpperInvariant()}, {e.User.Mention}.").ConfigureAwait(false);
                            }
                        }
                        catch(Exception ex)
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

                        int target_key;
                        TFMorph target_morph;

                        target_morph = Valid.Find(t => t.Code.Equals(e.GetArg("morph_type").ToLowerInvariant()));
                        target_key = Valid.FindIndex(t => t.Code.Equals(e.GetArg("morph_type").ToLowerInvariant()));
                        
                        if (target_morph == null)
                        {
                            await e.Channel.SendMessage($"That's not a valid morph, {e.User.Mention}.").ConfigureAwait(false);
                            return;
                        }

                        try
                        {
                            var db = DbHandler.Instance.GetAllRows<UserMorph>();
                            Dictionary<long, UserMorph> morphs = db.Where(t => t.UserId.Equals((long)e.User.Id)).ToDictionary(x => x.UserId, y => y);

                            if (morphs.ContainsKey((long)e.User.Id))
                            {
                                UserMorph morph = morphs[(long)e.User.Id];

                                morph.BodyType = target_key;
                                morph.UpperType = target_key;
                                morph.LowerType = target_key;
                                morph.LegType = target_key;
                                morph.ArmType = target_key;
                                morph.FaceType = target_key;
                                morph.EyeColor = 0;
                                morph.HairType = target_key;
                                morph.HairColor = 0;
                                morph.EarType = target_key;
                                morph.TongueType = target_key;
                                morph.TeethType = target_key;
                                morph.SkinType = target_key;
                                morph.SkinOrnamentsMorph = target_key;
                                morph.SkinOrnaments = 0;
                                // morph.SkinCovering = target_key;
                                morph.ArmCovering = target_key;
                                morph.TorsoCovering = target_key;
                                morph.LegCovering = target_key;
                                morph.HandModification = target_key;
                                morph.FeetModification = target_key;
                                morph.WingType = target_key;
                                morph.TailType = target_key;
                                morph.LegCount = target_morph.MaxLegs;
                                morph.ArmCount = target_morph.MaxArms;
                                morph.WingCount = target_morph.MaxWings;
                                morph.TailCount = target_morph.MaxTails;
                                morph.WingSize = target_morph.WingSize.Length - 1;
                                morph.TailSize = target_morph.WingSize.Length - 1;
                                morph.HairLength = target_morph.MaxHair;
                                morph.EarCount = target_morph.MaxEars;
                                morph.TongueLength = target_morph.MaxTongue;
                                morph.EyeCount = target_morph.MaxEyes;
                                morph.FeetType = target_key;
                                morph.HandType = target_key;
                                morph.MorphCount = +1;

                                DbHandler.Instance.Save(morph);
                            }
                            else
                            {
                                DbHandler.Instance.Connection.Insert(new UserMorph
                                {
                                    UserId = (long)e.User.Id,

                                    Gender = 0,
                                    BodyType = target_key,
                                    UpperType = target_key,
                                    LowerType = target_key,
                                    LegType = target_key,
                                    ArmType = target_key,
                                    FaceType = target_key,
                                    EyeColor = 0,
                                    HairType = target_key,
                                    HairColor = 0,
                                    EarType = target_key,
                                    TongueType = target_key,
                                    TeethType = target_key,
                                    SkinType = target_key,
                                    SkinOrnamentsMorph = target_key,
                                    SkinOrnaments = 0,
                                    // SkinCovering = target_key,
                                    ArmCovering = target_key,
                                    TorsoCovering = target_key,
                                    LegCovering = target_key,
                                    HandModification = target_key,
                                    FeetModification = target_key,
                                    WingType = target_key,
                                    TailType = target_key,
                                    LegCount = target_morph.MaxLegs,
                                    ArmCount = target_morph.MaxArms,
                                    WingCount = target_morph.MaxWings,
                                    TailCount = target_morph.MaxTails,
                                    WingSize = target_morph.WingSize.Length - 1,
                                    TailSize = target_morph.WingSize.Length - 1,
                                    HairLength = target_morph.MaxHair,
                                    EarCount = target_morph.MaxEars,
                                    TongueLength = target_morph.MaxTongue,
                                    EyeCount = target_morph.MaxEyes,
                                    FeetType = target_key,
                                    HandType = target_key,
                                    MorphCount = 0
                                }, typeof(UserMorph));
                            }
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
