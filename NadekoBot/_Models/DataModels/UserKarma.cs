using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.DataModels
{
    class UserKarma : IDataModel
    {
        public long UserID { get; set; }
        public int Karma { get; set; }
    }
}
