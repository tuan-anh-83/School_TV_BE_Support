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
                                 .Where(s => s.Status == true)
                                 .ToListAsync();
        }

        public async Task<SchoolChannel?> GetByIdAsync(int id)
        {
            return await _context.SchoolChannels
                                 .Include(sc => sc.Account)
                                 .FirstOrDefaultAsync(sc => sc.SchoolChannelID == id);
        }

        public async Task<IEnumerable<SchoolChannel>> SearchAsync(string? keyword, string? address, int? accountId)
        {
            var query = _context.SchoolChannels
                                .Include(sc => sc.Account)
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
            _context.SchoolChannels.Update(schoolChannel);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        public async Task<bool> DeleteByNameAsync(string name)
        {
            var schoolChannel = await _context.SchoolChannels.FirstOrDefaultAsync(s => s.Name == name);
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

    }
}
