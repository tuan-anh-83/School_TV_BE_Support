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
            return await _repository.UpdateAdAsync(adSchedule);
        }

        public async Task<bool> DeleteAdScheduleAsync(int id)
        {
            return await _repository.DeleteAdAsync(id);
        }

        public async Task<IEnumerable<AdSchedule>> FilterAdSchedulesAsync(DateTime start, DateTime end)
        {
            var all = await _repository.GetAllAsync();
            return all;
        }
        public async Task<AdSchedule?> GetLatestAdAsync()
        {
            var allAds = await _repository.GetAllAsync();
            return allAds.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
        }
    }
}
