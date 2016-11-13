using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class TFHead
    {
        public TFHead(String n, String f, String h, String e, String to, String te, String ey, String ea)
        {
            Name = n;
            Head = f;
            Hair = h;
            Ears = e;
            Tongue = to;
            Teeth = te;
            Eyes = ey;
            EarAnchor = ea;
        }
        public String Name { get; set; }
        public String Head { get; set; }
        public String Hair { get; set; }
        public String Ears { get; set; }
        public String Tongue { get; set; }
        public String Teeth { get; set; }
        public String Eyes { get; set; }
        public String EarAnchor { get; set; }
    }
}
