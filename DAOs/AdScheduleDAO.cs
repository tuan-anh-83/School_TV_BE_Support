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
    public class AdScheduleDAO
    {
        private static AdScheduleDAO instance = null;
        private readonly DataContext _context;

        private AdScheduleDAO()
        {
            _context = new DataContext();
        }

        public static AdScheduleDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AdScheduleDAO();
                }
                return instance;
            }
        }

        public async Task<IEnumerable<AdSchedule>> GetAllAsync()
        {
            return await _context.AdSchedules.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<AdSchedule>> GetAllForAdvertiserAsync(int accountId)
        {
            return await _context.AdSchedules.AsNoTracking().Where(a => a.AccountID == accountId).ToListAsync();
        }

        public async Task AddAsync(AdSchedule adSchedule)
        {
            await _context.AdSchedules.AddAsync(adSchedule);
        }

        public void Update(AdSchedule adSchedule)
        {
            _context.AdSchedules.Update(adSchedule);
        }

        public void Delete(AdSchedule adSchedule)
        {
            _context.AdSchedules.Remove(adSchedule);
        }

        public async Task<AdSchedule?> GetAdScheduleByIdAsync(int adScheduleId)
        {
            return await _context.AdSchedules.AsNoTracking()
         .FirstOrDefaultAsync(p => p.AdScheduleID == adScheduleId);
        }

        public async Task<bool> UpdateAdAsync(AdSchedule adSchedule)
        {
            var tracked = _context.ChangeTracker.Entries<AdSchedule>()
                .FirstOrDefault(e => e.Entity.AdScheduleID == adSchedule.AdScheduleID);

            if (tracked != null)
            {
                tracked.State = EntityState.Detached;
            }

            _context.AdSchedules.Attach(adSchedule);
            _context.Entry(adSchedule).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAdAsync(int adScheduleId)
        {
            var adSchedule = await GetAdScheduleByIdAsync(adScheduleId);
            if (adSchedule == null) return false;

            // Đơn giản hơn: Làm sạch ChangeTracker trước khi attach
            _context.ChangeTracker.Clear();

            // Sau đó thực hiện xóa
            _context.Remove(adSchedule);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AdSchedule>> FilterByDateRangeAsync(DateTime startTime, DateTime endTime)
        {
            return await _context.AdSchedules.AsNoTracking()
                .ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
