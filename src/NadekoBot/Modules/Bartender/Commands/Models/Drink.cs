using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Bartender.Models
{
    public class Drink
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public string Category { get; set; }

        public string Description { get; set; }
        public string Flavor_2nd { get; set; }
        public string Flavor_3rd { get; set; }

        public Boolean Dragon { get; set; }
        public Boolean Transformative { get; set; }

        public TFDetails Transform { get; set; }
    }

    public class TFDetails
    {
        public String Target { get; set; }
        public String Theme { get; set; }
        public int ChangeCount { get; set; }
        public int GrowCount { get; set; }
        public int ColorCount { get; set; }

        public String[] Balance { get; set; }

        public int[] WeightChange { get; set; }
        public int[] MusculatureChange { get; set; }

        public SortedList<String, int[]> Growth { get; set; }

        public String[] ColorRange { get; set; }
        public int[] Colors { get; set; }
        public List<String> ColorTargets { get; set; }

        public int[] SkinPattern { get; set; }
    }
}
