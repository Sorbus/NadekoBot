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

namespace NadekoBot.Modules.Bartender
{
    class _
    {
        public static Random rng = new Random();

        public static List<Drink> Drinks = NadekoBot.Config.Drinks;

        public static Dictionary<int, TFMorph> Morphs = NadekoBot.Config.Morphs;
        public static Dictionary<int, TFColor> Colors = NadekoBot.Config.Colors;
        public static Dictionary<int, TFOrnament> Ornament = NadekoBot.Config.Ornament;
        public static Dictionary<int, TFSkin> Skin = NadekoBot.Config.Skin;

        public static String[] PronounPossessive = new String[3] { "their", "her", "his" };
        public static String[] PronounSubjective = new String[3] { "they", "she", "he" };
        public static String[] PronounObjective = new String[3] { "them", "her", "him" };

        public static String[] Has = new String[3] { "have", "has", "has" };
        public static String[] Are = new String[3] { "are", "is", "is" };
        public static String[] Was = new String[3] { "were", "was", "was" };

        public static String[] PronounSelf = new String[3] { "themself", "herself", "himself" };

        public static readonly Regex replacer = new Regex(@"\$(\w+)\$", RegexOptions.Compiled);
    }
}
