using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class TFAppendages
    {
        public TFAppendages(String n, String l, String f, String fm, String a, String h, String hm, String t, String[] ts, String w, String[] ws)
        {
            Name = n;
            Legs = l;
            Feet = f;
            FeetMod = fm;
            Arms = a;
            Hands = h;
            HandMod = hm;
            Tail = t;
            TailSizes = ts;
            Wings = w;
            WingSizes = ws;
        }
        public String Name { get; set; }
        public String Legs { get; set; }
        public String Feet { get; set; }
        public String FeetMod { get; set; }
        public String Arms { get; set; }
        public String Hands { get; set; }
        public String HandMod { get; set; }
        public String Tail { get; set; }
        public String[] TailSizes { get; set; }
        public String Wings { get; set; }
        public String[] WingSizes { get; set; }
    }
}
