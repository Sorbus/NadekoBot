using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class TFBody
    {
        public TFBody(string n, string bt, string ut, string aa, string lt, string la, string wa, string ta,
            string tl, string wl, string al, string ll)
        {
            Name = n;
            BodyType = bt;
            UpperType = ut;
            ArmAnchor = aa;
            LowerType = lt;
            LegAnchor = la;
            WingAnchor = wa;
            TailAnchor = ta;
            WingLoc = wl;
            TailLoc = tl;
            ArmLoc = al;
            LegLoc = ll;
        }
        public String Name { get; set; }
        public String BodyType { get; set; }
        public String UpperType { get; set; }
        public String ArmAnchor { get; set; }
        public String LowerType { get; set; }
        public String LegAnchor { get; set; }
        public String WingAnchor { get; set; }
        public String TailAnchor { get; set; }
        public String WingLoc { get; set; }
        public String TailLoc { get; set; }
        public String ArmLoc { get; set; }
        public String LegLoc { get; set; }
    }
}
