using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class AdScheduleService : IAdScheduleService
    {
        private readonly IAdScheduleRepo _repository;

        public AdScheduleService(IAdScheduleRepo repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AdSchedule>> GetAllAdSchedulesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<AdSchedule?> GetAdScheduleByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAdScheduleAsync(AdSchedule adSchedule)
        {
            await _repository.AddAsync(adSchedule);
            await _repository.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateAdScheduleAsync(AdSchedule adSchedule)
        {
            _repository.Update(adSchedule);
            await _repository.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAdScheduleAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            _repository.Delete(existing);
            await _repository.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<AdSchedule>> FilterAdSchedulesAsync(DateTime start, DateTime end)
        {
            var all = await _repository.GetAllAsync();
            return all.Where(a => a.StartTime >= start && a.EndTime <= end);
        }
        public async Task<AdSchedule?> GetLatestAdAsync()
        {
            var allAds = await _repository.GetAllAsync();
            return allAds.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
        }

    }
}
