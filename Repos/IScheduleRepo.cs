using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IScheduleRepo
    {
        Task<Schedule> CreateScheduleAsync(Schedule schedule);
        Task<Schedule> GetScheduleByIdAsync(int scheduleId);
        Task<IEnumerable<Schedule>> GetAllSchedulesAsync();
        Task<IEnumerable<Schedule>> GetActiveSchedulesAsync();
        Task<bool> UpdateScheduleAsync(Schedule schedule);
        Task<bool> DeleteScheduleAsync(int scheduleId);
        Task<IEnumerable<Schedule>> GetLiveNowSchedulesAsync();
        Task<IEnumerable<Schedule>> GetUpcomingSchedulesAsync();
        Task<Dictionary<string, List<Schedule>>> GetSchedulesGroupedTimelineAsync();
        Task<IEnumerable<Schedule>> GetSchedulesByChannelAndDateAsync(int channelId, DateTime date);
        Task<List<Schedule>> GetSchedulesByDateAsync(DateTime date);
        Task<Program?> GetProgramByVideoHistoryIdAsync(int videoHistoryId);
        Task<List<Schedule>> GetSchedulesByProgramIdAsync(int programId);
        Task<bool> IsScheduleOverlappingAsync(int schoolChannelId, DateTime startTime, DateTime endTime);
       
    }
}
