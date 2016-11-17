using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class Drink
    {
        public Drink(string p, string n, int c, string t, string d, string f, bool s, bool w, TFDetails tf)
        {
            Code = p;
            Name = n;
            Cost = c;
            Category = t;
            Description = d;
            Flavor = f;
            Dragon = s;
            Transformative = w;
            Transform = tf;
        }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Flavor { get; set; }
        public Boolean Dragon { get; set; }
        public Boolean Transformative { get; set; }

        public TFDetails Transform { get; set; }
    }
    public class TFDetails
    {
        public TFDetails(string target, int changecount, String[] balance, int[] weightchange,
            int[] musculaturechange, SortedList<String, int[]> growth, int[] colors, List<String> colortargets,
            int[] skinpattern, int growcount, int colorcount)
        {
            Target = target;
            ChangeCount = changecount;
            GrowCount = growcount;
            ColorCount = colorcount;
            Balance = balance;
            WeightChange = weightchange;
            MusculatureChange = musculaturechange;
            Growth = growth;
            Colors = colors;
            ColorTargets = colortargets;
            SkinPattern = skinpattern;

        }
        public String Target { get; set; }
        public String Theme { get; set; }
        public int ChangeCount { get; set; }
        public int GrowCount { get; set; }
        public int ColorCount { get; set; }

        public String[] Balance { get; set; }

        public int[] WeightChange { get; set; }
        public int[] MusculatureChange { get; set; }
        
        public SortedList<String, int[]> Growth { get; set; }

        public int[] Colors { get; set; }
        public List<String> ColorTargets { get; set; }

        public int[] SkinPattern { get; set; }
    }
}
