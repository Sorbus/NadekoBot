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

namespace NadekoBot.Modules.Bartender.Helpers
{
    class Bar
    {
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

        public static int Move(int original, int max, int low, int top, Random rng)
        {
            if (original > max)
            { return -rng.Next(low, top); }
            else if (original < max)
            { return rng.Next(low, top); }
            else
            { return 0; }
        }

        public static int Move(int original, int max, int top, Random rng)
        {
            if (original > max)
            { return -rng.Next(0, top); }
            else if (original < max)
            { return rng.Next(0, top); }
            else
            { return 0; }
        }

        public static int Move(int original, int max, Random rng)
        {
            if (original > max)
            { return -1; }
            else if (original < max)
            { return 1; }
            else
            { return 0; }
        }

        public static int getRandItem(int[] a)
        { return a[_.rng.Next(0, a.Length)]; }
    }
}
