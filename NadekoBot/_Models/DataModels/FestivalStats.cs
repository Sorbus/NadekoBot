using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.DataModels
{
    class FestivalStats : IDataModel
    {
        public long UserID { get; set; }
        public int Gems { get; set; }
        public int Lens { get; set; }
        public int Bars { get; set; }
        public int Gears { get; set; }
        public int Plates { get; set; }
        public int Assembled { get; set; }
        public int Summoned { get; set; }
    }
}
