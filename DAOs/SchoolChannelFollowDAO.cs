using BOs.Data;
using BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAOs
{
    public class SchoolChannelFollowDAO

    {
        private static SchoolChannelFollowDAO instance = null;
        private readonly DataContext _context;

        private SchoolChannelFollowDAO()
        {
            _context = new DataContext();
        }

        public static SchoolChannelFollowDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SchoolChannelFollowDAO();
                }
                return instance;
            }
        }
        public async Task<List<SchoolChannelFollow>> GetFollowersBySchoolChannelIdAsync(int schoolChannelId)
        {
            return await _context.SchoolChannelFollows
                .Where(f => f.SchoolChannelID == schoolChannelId && f.Status == "Followed")
                .ToListAsync();
        }

        public async Task<List<SchoolChannelFollow>> GetFollowersByChannelIdAsync(int channelId)
        {
            return await _context.SchoolChannelFollows
                .Where(f => f.SchoolChannelID == channelId)
                .ToListAsync();
        }

        public async Task AddFollowAsync(SchoolChannelFollow follow)
        {
            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.AccountID == follow.AccountID && f.SchoolChannelID == follow.SchoolChannelID);

            if (existingFollow == null)
            {
                _context.Follows.Add(follow);
            }
            else if (existingFollow.Status == "Unfollowed")
            {
                existingFollow.Status = "Followed";
                existingFollow.FollowedAt = System.DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateFollowStatusAsync(int accountId, int schoolChannelId, string status)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.AccountID == accountId && f.SchoolChannelID == schoolChannelId);

            if (follow != null)
            {
                follow.Status = status;
                follow.FollowedAt = System.DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetFollowCountAsync(int schoolChannelId)
        {
            return await _context.Follows
                .Where(f => f.SchoolChannelID == schoolChannelId && f.Status == "Followed")
                .CountAsync();
        }

        public async Task<List<SchoolChannelFollow>> GetAllFollowsAsync()
        {
            return await _context.Follows
                .Include(f => f.Account)
                .Include(f => f.SchoolChannel)
                .ToListAsync();
        }

        public async Task<SchoolChannelFollow> GetFollowAsync(int accountId, int schoolChannelId)
        {
            return await _context.Follows
                .FirstOrDefaultAsync(f => f.AccountID == accountId && f.SchoolChannelID == schoolChannelId);
        }

        public async Task<bool> IsFollowingAsync(int accountId, int schoolChannelId)
        {
            return await _context.Follows
                .AnyAsync(f => f.AccountID == accountId && f.SchoolChannelID == schoolChannelId && f.Status == "Followed");
        }
        public async Task<IEnumerable<SchoolChannel>> GetFollowedSchoolChannelsAsync(int accountId)
        {
            return await _context.Follows
                .Where(f => f.AccountID == accountId && f.Status == "Followed" && f.SchoolChannel.Status == true)
                .Include(f => f.SchoolChannel)
                .Select(f => f.SchoolChannel)
                .ToListAsync();
        }
        public async Task<List<object>> GetAllFollowedSchoolChannelsAsync()
        {
            return await _context.Follows
                .Where(f => f.Status == "Followed")
                .GroupBy(f => f.SchoolChannelID)
                .Select(g => new
                {
                    SchoolChannelID = g.Key,
                    FollowCount = g.Count()
                })
                .OrderByDescending(g => g.FollowCount)
                .Cast<object>()
                .ToListAsync();
        }
    }
}
