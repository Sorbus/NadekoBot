using NadekoBot.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NadekoBot.Services.Database.Repositories.Impl
{
    public class InventoryRepository : Repository<Inventory>, IInventoryRepository
    {
        public InventoryRepository(DbContext context) : base(context)
        {
        }

        public Inventory Get(ulong userId, string code)
        {
            var kar = _set.FirstOrDefault(c => (c.UserId == userId && c.Code == code));

            if (kar == null)
            {
                kar = new Inventory()
                {
                    UserId = userId,
                    Code = code,
                    Amount = 0
                };
            }
            return kar;
        }

        public bool TryUpdateState(ulong userId, string code, int change)
        {
            var cur = Get(userId, code);

            if (change == 0)
                return true;

            if (change > 0)
            {
                cur.Amount += change;
                return true;
            }
            //change is negative
            if (cur.Amount + change >= 0)
            {
                cur.Amount += change;

                if (cur.Amount == 0)
                {
                    _context.Remove(cur);
                }

                return true;
            }
            return false;
        }
    }
}