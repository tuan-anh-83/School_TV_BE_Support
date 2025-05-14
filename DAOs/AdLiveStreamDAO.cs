using Azure.Core;
using BOs.Data;
using BOs.Migrations;
using BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdLiveStream = BOs.Models.AdLiveStream;

namespace DAOs
{
    public class AdLiveStreamDAO
    {
        private static AdLiveStreamDAO instance = null;
        private readonly DataContext _context;

        private AdLiveStreamDAO()
        {
            _context = new DataContext();
        }

        public static AdLiveStreamDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AdLiveStreamDAO();
                }
                return instance;
            }
        }

        public async Task<IEnumerable<AdLiveStream>> GetExistsAdLiveStreams(int scheduleID)
        {
            return await _context.AdLiveStreams
                .AsNoTracking()
                .Where(x => x.ScheduleID == scheduleID)
                .ToListAsync();
        }

        public async Task<int> AddRangeAdLiveStreamAsync(List<AdLiveStream> ads)
        {
            if (ads == null || ads.Count == 0)
                return 0;
            await _context.AdLiveStreams.AddRangeAsync(ads);
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AdLiveStream>> GetDueAds(DateTime now)
        {
            return await _context.AdLiveStreams
                .AsNoTracking()
                .Where(x => x.PlayAt <= now && !x.IsPlayed)
                .Include(x => x.AdSchedule)
                .Include(x => x.Schedule)
                .ToListAsync();
        }

        public async Task<int> UpdateAdLiveStream(AdLiveStream adLiveStream)
        {
            if (adLiveStream == null)
                return 0;
            _context.AdLiveStreams.Update(adLiveStream);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateRangeAdLiveStream(List<AdLiveStream> adLiveStreams)
        {
            if (adLiveStreams == null || adLiveStreams.Count == 0)
                return 0;
            _context.AdLiveStreams.UpdateRange(adLiveStreams);
            return await _context.SaveChangesAsync();
        }

        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void UpdateStatus(int adLiveStreamId)
        {
            var existingEntity = _context.AdLiveStreams.Find(adLiveStreamId);
            if (existingEntity != null)
            {
                existingEntity.IsPlayed = true;
                // No need to call Update, EF Core will track this change
            }
            else
            {
                // If entity isn't already tracked, then use your approach
                var stub = new AdLiveStream
                {
                    AdLiveStreamID = adLiveStreamId,
                    IsPlayed = true
                };
                _context.AdLiveStreams.Attach(stub);
                _context.Entry(stub).Property(x => x.IsPlayed).IsModified = true;
            }
        }

        public async Task UpdateStatusAlternative(int adLiveStreamId)
        {
            // This uses raw SQL to update only what's needed without tracking issues
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE AdLiveStream SET IsPlayed = 1 WHERE AdLiveStreamID = {0}",
                adLiveStreamId);
        }

        public async Task<IEnumerable<AdLiveStream>> GetExpiredAds(DateTime now)
        {
            return await _context.AdLiveStreams
                .Where(x => x.PlayAt < now && !x.IsPlayed)
                .Include(x => x.AdSchedule)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
