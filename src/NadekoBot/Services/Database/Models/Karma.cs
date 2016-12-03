using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Services.Database.Models
{
    public class Karma : DbEntity
    {
        public ulong UserId { get; set; }
        public long Amount { get; set; }
    }
}
