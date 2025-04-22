using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class ScheduleRepo : IScheduleRepo
    {

        public Task<Schedule> CreateScheduleAsync(Schedule schedule)
        {
            return ScheduleDAO.Instance.CreateScheduleAsync(schedule);
        }

        public Task<bool> DeleteScheduleAsync(int scheduleId)
        {
            return ScheduleDAO.Instance.DeleteScheduleAsync(scheduleId);
        }

        public async Task<IEnumerable<Schedule>> GetActiveSchedulesAsync()
        {
            return await ScheduleDAO.Instance.GetActiveSchedulesAsync();    
        }

        public Task<IEnumerable<Schedule>> GetAllSchedulesAsync()
        {
            return ScheduleDAO.Instance.GetAllSchedulesAsync();
        }

        public async Task<IEnumerable<Schedule>> GetLiveNowSchedulesAsync()
        {
            return await ScheduleDAO.Instance.GetLiveNowSchedulesAsync();
        }

       

        public async Task<Program?> GetProgramByVideoHistoryIdAsync(int videoHistoryId)
        {
            return await ScheduleDAO.Instance.GetProgramByVideoHistoryIdAsync(videoHistoryId);
        }

        public Task<Schedule> GetScheduleByIdAsync(int scheduleId)
        {
            return ScheduleDAO.Instance.GetScheduleByIdAsync(scheduleId);
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByChannelAndDateAsync(int channelId, DateTime date)
        {
            return await ScheduleDAO.Instance.GetSchedulesByChannelAndDateAsync(channelId, date);
        }

        public async Task<List<Schedule>> GetSchedulesByDateAsync(DateTime date)
        {
            return await ScheduleDAO.Instance.GetSchedulesByDateAsync(date);
        }

        public async Task<List<Schedule>> GetSchedulesByProgramIdAsync(int programId)
        {
            return await ScheduleDAO.Instance.GetSchedulesByProgramIdAsync(programId);
        }

        public async Task<Dictionary<string, List<Schedule>>> GetSchedulesGroupedTimelineAsync()
        {
            return await ScheduleDAO.Instance.GetSchedulesGroupedTimelineAsync();
        }

        public async Task<IEnumerable<Schedule>> GetUpcomingSchedulesAsync()
        {
            return await ScheduleDAO.Instance.GetUpcomingSchedulesAsync();
        }

        public async Task<bool> IsScheduleOverlappingAsync(int schoolChannelId, DateTime startTime, DateTime endTime)
        {
            return await ScheduleDAO.Instance.IsScheduleOverlappingAsync(schoolChannelId, startTime, endTime);
        }

        public Task<bool> UpdateScheduleAsync(Schedule schedule)
        {
            return ScheduleDAO.Instance.UpdateScheduleAsync(schedule);
        }


    }
}
