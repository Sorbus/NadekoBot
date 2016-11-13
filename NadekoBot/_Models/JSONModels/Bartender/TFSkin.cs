using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class TFSkin
    {
        public TFSkin(string n, string t, string c)
        {
            Name = n;
            Text = t;
            Cover = c;
        }
        public String Name { get; set; }
        public String Text { get; set; }
        public String Cover { get; set; }
    }
}
