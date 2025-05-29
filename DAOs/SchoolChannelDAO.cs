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
    public class SchoolChannelDAO
    {
        private static SchoolChannelDAO instance = null;
        private readonly DataContext _context;

        private SchoolChannelDAO()
        {
            _context = new DataContext();
        }

        public static SchoolChannelDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SchoolChannelDAO();
                }
                return instance;
            }
        }

        public async Task<IEnumerable<SchoolChannel>> GetAllAsync()
        {
            return await _context.SchoolChannels
                                 .AsNoTracking()
                                 .Include(s => s.News)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<SchoolChannel>> GetAllActiveAsync()
        {
            return await _context.SchoolChannels
                                 .AsNoTracking()
                                 .Include(s => s.News)
                                 .Include(s => s.Account)
                                 .Where(s => s.Status == true && s.Account != null && !string.IsNullOrWhiteSpace(s.Account.Status) && s.Account.Status.Trim().ToLower() == "active")
                                 .ToListAsync();
        }

        public async Task<SchoolChannel?> GetByIdAsync(int id)
        {
            return await _context.SchoolChannels.AsNoTracking()
                                 .Include(sc => sc.Programs)
                                 .Include(sc => sc.Account)
                                    .ThenInclude(a => a.AccountPackages)
                                 .FirstOrDefaultAsync(sc => sc.SchoolChannelID == id);
        }

        public async Task<IEnumerable<SchoolChannel>> SearchAsync(string? keyword, string? address, int? accountId)
        {
            var query = _context.SchoolChannels.AsNoTracking()
                          .Include(sc => sc.Followers)
                          .Include(sc => sc.Account)
                            .ThenInclude(a => a.AccountPackages)
                          .Where(sc => sc.Status == true)
                          .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(sc => sc.Name.Contains(keyword) || sc.Description.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(address))
            {
                query = query.Where(sc => sc.Address.Contains(address));
            }

            if (accountId.HasValue)
            {
                query = query.Where(sc => sc.AccountID == accountId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task AddAsync(SchoolChannel schoolChannel)
        {
            await _context.SchoolChannels.AddAsync(schoolChannel);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SchoolChannel schoolChannel)
        {
            var tracked = _context.ChangeTracker.Entries<SchoolChannel>()
                .FirstOrDefault(e => e.Entity.SchoolChannelID == schoolChannel.SchoolChannelID);

            if (tracked != null)
            {
                tracked.State = EntityState.Detached;
            }

            if (schoolChannel.Account != null)
            {
                var trackedAccount = _context.ChangeTracker.Entries<Account>()
                    .FirstOrDefault(e => e.Entity.AccountID == schoolChannel.Account.AccountID);
                if (trackedAccount != null)
                {
                    trackedAccount.State = EntityState.Detached;
                }
            }

            _context.SchoolChannels.Attach(schoolChannel);
            _context.Entry(schoolChannel).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteByNameAsync(string name)
        {
            var schoolChannel = await _context.SchoolChannels.AsNoTracking().FirstOrDefaultAsync(s => s.Name == name);

            if (schoolChannel == null)
            {
                return false;
            }
            schoolChannel.Status = false;
            schoolChannel.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DoesAccountHaveSchoolChannelAsync(int accountId)
        {
            return await _context.SchoolChannels.AnyAsync(sc => sc.AccountID == accountId);
        }

        public async Task<bool> IsOwner(int accountId, int schoolChannelId)
        {
            return await _context.SchoolChannels.AnyAsync(sc => sc.AccountID == accountId && sc.SchoolChannelID == schoolChannelId);
        }
    }
}
