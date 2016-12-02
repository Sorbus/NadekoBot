﻿using NadekoBot.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NadekoBot.Modules.Permissions;

namespace NadekoBot.Services.Database.Repositories.Impl
{
    public class GuildConfigRepository : Repository<GuildConfig>, IGuildConfigRepository
    {
        public GuildConfigRepository(DbContext context) : base(context)
        {
        }

        public new IEnumerable<GuildConfig> GetAll() =>
            _set.Include(gc => gc.LogSetting)
                    .ThenInclude(ls => ls.IgnoredChannels)
                .Include(gc => gc.LogSetting)
                    .ThenInclude(ls => ls.IgnoredVoicePresenceChannelIds)
                .Include(gc => gc.RootPermission)
                    .ThenInclude(gc => gc.Previous)
                .Include(gc => gc.RootPermission)
                    .ThenInclude(gc => gc.Next)
                .Include(gc => gc.GenerateCurrencyChannelIds)
                .Include(gc => gc.FilterInvitesChannelIds)
                .Include(gc => gc.FilterWordsChannelIds)
                .Include(gc => gc.FilteredWords)
                .Include(gc => gc.CommandCooldowns)
                .Include(gc => gc.ModuleCooldowns)
                .ToList();

        /// <summary>
        /// Gets and creates if it doesn't exist a config for a guild.
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public GuildConfig For(ulong guildId)
        {
            var config = _set
                            .Include(gc => gc.FollowedStreams)
                             .Include(gc => gc.LogSetting)
                                .ThenInclude(ls => ls.IgnoredChannels)
                            .Include(gc => gc.LogSetting)
                                .ThenInclude(ls => ls.IgnoredVoicePresenceChannelIds)
                            .Include(gc => gc.FilterInvitesChannelIds)
                            .Include(gc => gc.FilterWordsChannelIds)
                            .Include(gc => gc.FilteredWords)
                            .Include(gc => gc.GenerateCurrencyChannelIds)
                            .Include(gc => gc.CommandCooldowns)
                            .Include(gc => gc.ModuleCooldowns)
                            .FirstOrDefault(c => c.GuildId == guildId);

            if (config == null)
            {
                _set.Add((config = new GuildConfig
                {
                    GuildId = guildId,
                    RootPermission = Permission.GetDefaultRoot(),
                }));
                _context.SaveChanges();
            }
            return config;
        }

        public GuildConfig PermissionsFor(ulong guildId)
        {
            var query = _set.Include(gc => gc.RootPermission);

            //todo this is possibly a disaster for performance
            //What i could do instead is count the number of permissions in the permission table for this guild
            // and make a for loop with those.
            // or just select permissions for this guild and manually chain them
            for (int i = 0; i < 60; i++)
            {
                query = query.ThenInclude(gc => gc.Next);
            }

            var config = query.FirstOrDefault(c => c.GuildId == guildId);

            if (config == null)
            {
                _set.Add((config = new GuildConfig
                {
                    GuildId = guildId,
                    RootPermission = Permission.GetDefaultRoot(),
                }));
                _context.SaveChanges();
            }
            return config;
        }

        public IEnumerable<GuildConfig> PermissionsForAll()
        {
            var query = _set.Include(gc => gc.RootPermission);

            //todo this is possibly a disaster for performance
            //What i could do instead is count the number of permissions in the permission table for this guild
            // and make a for loop with those.
            // or just select permissions for this guild and manually chain them
            for (int i = 0; i < 60; i++)
            {
                query = query.ThenInclude(gc => gc.Next);
            }

            return query.ToList();
        }

        public IEnumerable<FollowedStream> GetAllFollowedStreams() =>
            _set.Include(gc => gc.FollowedStreams)
                .SelectMany(gc => gc.FollowedStreams)
                .ToList();

        public GuildConfig SetNewRootPermission(ulong guildId, Permission p)
        {
            var data = _set
                        .Include(gc => gc.RootPermission)
                        .FirstOrDefault(gc => gc.GuildId == guildId);

            data.RootPermission.Prepend(p);
            data.RootPermission = p;
            return data;
        }
    }
}
