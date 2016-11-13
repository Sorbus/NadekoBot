using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class TFOrnament
    {
        public TFOrnament(String n)
        {
            Name = n;
        }
        public String Name { get; set; }
    }
}
