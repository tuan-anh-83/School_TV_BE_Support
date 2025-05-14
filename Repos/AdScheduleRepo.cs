using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class AdScheduleRepo : IAdScheduleRepo
    {
        public Task AddAsync(AdSchedule adSchedule)
        {
            return AdScheduleDAO.Instance.AddAsync(adSchedule); 
        }

        public void Delete(AdSchedule adSchedule)
        {
            AdScheduleDAO.Instance.Delete(adSchedule);
        }

        public async Task<bool> DeleteAdAsync(int adScheduleId)
        {
            return await AdScheduleDAO.Instance.DeleteAdAsync(adScheduleId);
        }

        public async Task<IEnumerable<AdSchedule>> FilterByDateRangeAsync(DateTime startTime, DateTime endTime)
        {
            return await AdScheduleDAO.Instance.FilterByDateRangeAsync(startTime, endTime);
        }

        public async Task<IEnumerable<AdSchedule>> GetAllAsync()
        {
            return await AdScheduleDAO.Instance.GetAllAsync();
        }

        public async Task<AdSchedule?> GetByIdAsync(int id)
        {
            return await AdScheduleDAO.Instance.GetAdScheduleByIdAsync(id);
        }

        public Task SaveAsync()
        {
            return AdScheduleDAO.Instance.SaveAsync();
        }

        public void Update(AdSchedule adSchedule)
        {
            AdScheduleDAO.Instance.Update(adSchedule);
        }

        public async Task<bool> UpdateAdAsync(AdSchedule adSchedule)
        {
            return await AdScheduleDAO.Instance.UpdateAdAsync(adSchedule);
        }
    }
}
