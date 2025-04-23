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
            return await _context.AdSchedules.ToListAsync();
        }

        public async Task<AdSchedule> GetByIdAsync(int id)
        {
            return await _context.AdSchedules.FindAsync(id);
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
        public async Task<IEnumerable<AdSchedule>> FilterByDateRangeAsync(DateTime startTime, DateTime endTime)
        {
            return await _context.AdSchedules
                .Where(ad => ad.StartTime >= startTime && ad.EndTime <= endTime)
                .ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
