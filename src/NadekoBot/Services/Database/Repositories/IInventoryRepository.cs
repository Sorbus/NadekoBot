using NadekoBot.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Services.Database.Repositories
{
    public interface IInventoryRepository : IRepository<Inventory>
    {
        Inventory Get(ulong userId, string code);
        bool TryUpdateState(ulong userId, string code, int change);
    }
}