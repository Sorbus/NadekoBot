using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NadekoBot.Classes.JSONModels;

namespace NadekoBot.DataModels.Bartender
{
    class InventoryModel : IDataModel
    {
        public long UserId { get; set; }

        public String DrinkCode { get; set; }
        public int Count { get; set; }
    }
}
