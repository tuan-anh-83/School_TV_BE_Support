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
    public class ScheduleDAO
    {
        private static ScheduleDAO instance = null;
        private readonly DataContext _context;

        private ScheduleDAO()
        {
            _context = new DataContext();
        }

        public static ScheduleDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ScheduleDAO();
                }
                return instance;
            }
        }

        public async Task<bool> IsScheduleOverlappingAsync(int schoolChannelId, DateTime startTime, DateTime endTime)
        {
            return await _context.Schedules
                .AsNoTracking()  // Thêm AsNoTracking() để tránh cache
                .AnyAsync(s => s.Program.SchoolChannelID == schoolChannelId &&
                               ((startTime >= s.StartTime && startTime < s.EndTime) ||
                                (endTime > s.StartTime && endTime <= s.EndTime) ||
                                (startTime <= s.StartTime && endTime >= s.EndTime)));
        }

        public async Task<List<Schedule>> GetSchedulesByProgramIdAsync(int programId)
        {
            return await _context.Schedules
                .Where(s => s.ProgramID == programId)
                .AsNoTracking()  // Thêm AsNoTracking() để tránh cache
                .ToListAsync();
        }

        public async Task<Schedule?> GetScheduleByProgramIdAsync(int programId)
        {
            return await _context.Schedules
                .AsNoTracking()
                .Where(s => s.ProgramID == programId && !s.LiveStreamEnded)
                .OrderByDescending(s => s.StartTime)
                .FirstOrDefaultAsync();
        }

        public async Task<Schedule> CreateScheduleAsync(Schedule schedule)
        {
            if (schedule.StartTime >= schedule.EndTime)
                throw new Exception("StartTime must be earlier than EndTime.");

            schedule.Status = string.IsNullOrWhiteSpace(schedule.Status) ? "Pending" : schedule.Status;

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<Schedule> GetScheduleByIdAsync(int scheduleId)
        {
            var schedule = await _context.Schedules.AsNoTracking()
                .Include(s => s.Program)
                .AsNoTracking()  // Thêm AsNoTracking() để tránh cache
                .FirstOrDefaultAsync(s => s.ScheduleID == scheduleId);

            if (schedule == null)
                throw new Exception("Schedule not found.");

            return schedule;
        }

        public async Task<IEnumerable<Schedule>> GetAllSchedulesAsync()
        {
            return await _context.Schedules
              .Include(s => s.Program)
                .ThenInclude(p => p.SchoolChannel)
                    .ThenInclude(sc => sc.Account)
                .AsNoTracking()
                .Where(s => s.Program != null && s.Program.SchoolChannel != null && s.Program.SchoolChannel.Account != null && !string.IsNullOrWhiteSpace(s.Program.SchoolChannel.Account.Status) && s.Program.SchoolChannel.Account.Status.Trim().ToLower() == "active")
                .ToListAsync();
        }

        public async Task<bool> UpdateScheduleAsync(Schedule schedule)
        {
            var existingSchedule = await _context.Schedules.FindAsync(schedule.ScheduleID);
            if (existingSchedule == null)
                throw new Exception("Schedule not found.");

            if (schedule.StartTime >= schedule.EndTime)
                throw new Exception("StartTime must be earlier than EndTime.");

            existingSchedule.StartTime = schedule.StartTime;
            existingSchedule.EndTime = schedule.EndTime;
            existingSchedule.Status = schedule.Status;
            existingSchedule.LiveStreamStarted = schedule.LiveStreamStarted;
            existingSchedule.LiveStreamEnded = schedule.LiveStreamEnded;
            existingSchedule.VideoHistoryID = schedule.VideoHistoryID;
            existingSchedule.Thumbnail = schedule.Thumbnail;

            _context.Schedules.Update(existingSchedule);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteScheduleAsync(int scheduleId)
        {
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null)
                throw new Exception("Schedule not found.");

            schedule.Status = "Inactive";

            _context.Schedules.Update(schedule);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Schedule>> GetActiveSchedulesAsync()
        {
            return await _context.Schedules.AsNoTracking()
          .Where(s => s.Status == "Active" || s.Status == "Ready" || s.Status == "Live")
                .Include(s => s.Program)
                .AsNoTracking()  // Thêm AsNoTracking() để tránh cache
                .ToListAsync();
        }

        public async Task<Schedule?> GetActiveScheduleByProgramIdAsync(int programId)
        {
            return await _context.Schedules.AsNoTracking()
                .Where(s => s.Status == "Live" && !s.LiveStreamEnded)
                .OrderByDescending(s => s.StartTime)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Schedule>> GetLiveNowSchedulesAsync()
        {
            return await _context.Schedules.AsNoTracking()
          .Where(s => s.Status == "Live")
                .Include(s => s.Program)
                    .ThenInclude(p => p.SchoolChannel)
                .AsNoTracking()  // Thêm AsNoTracking() để tránh cache
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetUpcomingSchedulesAsync()
        {
            return await _context.Schedules.AsNoTracking()
            .Where(s => s.Status == "Pending" || s.Status == "Ready")
                .Include(s => s.Program)
                .AsNoTracking()  // Thêm AsNoTracking() để tránh cache
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSuitableSchedulesAsync(DateTime now)
        {
            return await _context.Schedules.AsNoTracking()
            .Where(s => ((s.Status == "Pending" || s.Status == "Ready") && s.StartTime > now) || s.Status == "Live" || s.Status == "LateStart")
                .Include(s => s.Program)
                    .ThenInclude(p => p.SchoolChannel)
                .Include(s => s.AdLiveStreams)
                .AsNoTracking()  // Thêm AsNoTracking() để tránh cache
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByChannelAndDateAsync(int channelId, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            return await _context.Schedules.AsNoTracking()
                .Where(s =>
                    s.Program.SchoolChannelID == channelId &&
                    s.StartTime >= start &&
                    s.StartTime < end)
                .Include(s => s.Program)
                .AsNoTracking()  // Thêm AsNoTracking() để tránh cache
                .ToListAsync();
        }

        public async Task<Dictionary<string, List<Schedule>>> GetSchedulesGroupedTimelineAsync()
        {
            var all = await GetAllSchedulesAsync();
            var result = new Dictionary<string, List<Schedule>>
            {
                ["LiveNow"] = all.Where(s => s.Status == "Live" || s.Status == "LateStart").ToList(),
                ["Upcoming"] = all.Where(s => s.Status == "Pending" || s.Status == "Ready").ToList(),
                ["Replay"] = all.Where(s => s.Status == "Ended" || s.Status == "EndedEarly").ToList()
            };

            return result;
        }

        public async Task<List<Schedule>> GetSchedulesByDateAsync(DateTime date)
        {
            return await _context.Schedules
          .Include(s => s.Program)
                    .ThenInclude(p => p.SchoolChannel)
                .Where(s => s.StartTime.Date == date.Date)
                .AsNoTracking()  // Thêm AsNoTracking() để tránh cache
                .ToListAsync();
        }

        public async Task<Program?> GetProgramByVideoHistoryIdAsync(int videoHistoryId)
        {
            var video = await _context.VideoHistories
          .Include(v => v.Program)
                .AsNoTracking()  // Thêm AsNoTracking() để tránh cache
                .FirstOrDefaultAsync(v => v.VideoHistoryID == videoHistoryId);

            return video?.Program;
        }

        public async Task<bool> CheckIsInSchedule(DateTime streamAt)
        {
            return await _context.Schedules.AsNoTracking().AnyAsync(s => streamAt >= s.StartTime && streamAt < s.EndTime);
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesBySchoolIdAsync(int channelId)
        {
            return await _context.Schedules.AsNoTracking()
                .Include(s => s.Program)
                .Where(s => s.Program.SchoolChannelID == channelId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }
    }
}
