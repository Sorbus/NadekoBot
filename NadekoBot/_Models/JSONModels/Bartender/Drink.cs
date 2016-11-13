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
            Cat = t;
            Description = d;
            Flavor = f;
            Dragon = s;
            Transformative = w;
            Transform = tf;
        }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public string Cat { get; set; }
        public string Description { get; set; }
        public string Flavor { get; set; }
        public Boolean Dragon { get; set; }
        public Boolean Transformative { get; set; }

        public TFDetails Transform { get; set; }
    }
    public class TFDetails
    {
        public TFDetails()
        {

        }
        public String Target { get; set; }
        public int[] HairMod { get; set; }
        public int WeightMod { get; set; }
        public int MusculatureMod { get; set; }
    }
}
