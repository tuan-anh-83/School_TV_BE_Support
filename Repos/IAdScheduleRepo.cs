using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IAdScheduleRepo
    {
        Task<IEnumerable<AdSchedule>> GetAllAsync();
        Task<IEnumerable<AdSchedule>> GetAdsTodayAsync(DateTime start, DateTime end);
        Task<IEnumerable<AdSchedule>> GetListAdsInRangeAsync(DateTime start, DateTime end);
        Task<AdSchedule> GetByIdAsync(int id);
        Task AddAsync(AdSchedule adSchedule);
        void Update(AdSchedule adSchedule);
        void Delete(AdSchedule adSchedule);
        Task<IEnumerable<AdSchedule>> FilterByDateRangeAsync(DateTime startTime, DateTime endTime);
        Task SaveAsync();
    }
}
