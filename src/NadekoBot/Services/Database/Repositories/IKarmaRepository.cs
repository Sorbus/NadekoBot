﻿using NadekoBot.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Services.Database.Repositories
{
    public interface IKarmaRepository : IRepository<Karma>
    {
        Karma GetOrCreate(ulong userId);
        long GetUserKarma(ulong userId);
        bool TryUpdateState(ulong userId, long change);
        // IEnumerable<Karma> GetTopRichest(int count);
    }
}
