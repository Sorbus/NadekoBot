using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using NadekoBot.Services.Database;
using NadekoBot.Extensions;
using NadekoBot.Modules.Gambling;
using NadekoBot.Services.Database.Models;

namespace NadekoBot.Services
{
    public static class KarmaHandler
    {
        public static async Task AddKarmaAsync(IGuildUser target, long amount)
        {
            await AddKarmaAsync(target.Id, amount);
        }

        public static async Task AddKarmaAsync(ulong receiverId, long amount)
        {
            if (amount < 0)
                throw new ArgumentNullException(nameof(amount));

            using (var uow = DbHandler.UnitOfWork())
            {
                uow.Karma.TryUpdateState(receiverId, amount);
                await uow.CompleteAsync();
            }
        }
    }
}
