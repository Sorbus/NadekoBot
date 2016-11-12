using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class TFColors
    {
        public TFColors(string name, string hue)
        {
            Name = name;
            Hue = hue;
        }
        public String Name { get; set; }
        public String Hue { get; set; }
    }
}
