using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class BarDrink
    {
        public BarDrink(string n, int c, string t, string d, string f, string i, bool s, bool w, string[] h)
        {
            Name = n;
            Cost = c;
            Type = t;
            Description = d;
            Flavor = f;
            Image = i;
            Dragon = s;
            Transformative = w;
            HairMod = h;
        }
        public string Name { get; set; }
        public int Cost { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Flavor { get; set; }
        public string Image { get; set; }
        public Boolean Dragon { get; set; }
        public Boolean Transformative { get; set; }
        public String[] HairMod { get; set; }
    }
    public class Transformative
    {
        public Transformative()
        {

        }
    }
}
