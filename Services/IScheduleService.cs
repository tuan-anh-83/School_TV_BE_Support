using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IScheduleService
    {
        Task<Schedule> CreateScheduleAsync(Schedule schedule);
        Task<Schedule> GetScheduleByIdAsync(int scheduleId);
        Task<IEnumerable<Schedule>> GetAllSchedulesAsync();
        Task<bool> UpdateScheduleAsync(Schedule schedule);
        Task<bool> DeleteScheduleAsync(int scheduleId);

        Task<Dictionary<string, List<Schedule>>> GetSchedulesGroupedTimelineAsync();
        Task<IEnumerable<Schedule>> GetSchedulesByChannelAndDateAsync(int channelId, DateTime date);
        Task<IEnumerable<Schedule>> GetLiveNowSchedulesAsync();
        Task<IEnumerable<Schedule>> GetUpcomingSchedulesAsync();
        Task<List<Schedule>> GetSchedulesByDateAsync(DateTime date);
        Task<Schedule> CreateReplayScheduleFromVideoAsync(int videoHistoryId, DateTime start, DateTime end);
        Task<List<Schedule>> GetSchedulesByProgramIdAsync(int programId);
        Task<bool> IsScheduleOverlappingAsync(int schoolChannelId, DateTime startTime, DateTime endTime);
    }
}
