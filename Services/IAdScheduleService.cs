using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAdScheduleService
    {
        Task<IEnumerable<AdSchedule>> GetAllAdSchedulesAsync();
        Task<AdSchedule?> GetAdScheduleByIdAsync(int id);
        Task<bool> CreateAdScheduleAsync(AdSchedule adSchedule);
        Task<bool> UpdateAdScheduleAsync(AdSchedule adSchedule);
        Task<bool> DeleteAdScheduleAsync(int id);
        Task<IEnumerable<AdSchedule>> FilterAdSchedulesAsync(DateTime start, DateTime end);
        Task<AdSchedule?> GetLatestAdAsync();

    }
}
