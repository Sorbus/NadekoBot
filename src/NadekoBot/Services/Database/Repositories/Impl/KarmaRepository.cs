using NadekoBot.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NadekoBot.Services.Database.Repositories.Impl
{
    public class KarmaRepository : Repository<Karma>, IKarmaRepository
    {
        public KarmaRepository(DbContext context) : base(context)
        {
        }

        public Karma GetOrCreate(ulong userId)
        {
            var kar = _set.FirstOrDefault(c => c.UserId == userId);

            if (kar == null)
            {
                _set.Add(kar = new Karma()
                {
                    UserId = userId,
                    Amount = 0
                });
                _context.SaveChanges();
            }
            return kar;
        }

        public IEnumerable<Karma> GetTopRichest(int count) =>
            _set.OrderByDescending(c => c.Amount).Take(count).ToList();

        public long GetUserKarma(ulong userId) =>
            GetOrCreate(userId).Amount;

        public bool TryUpdateState(ulong userId, long change)
        {
            var cur = GetOrCreate(userId);

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
                return true;
            }
            return false;
        }
    }
}
