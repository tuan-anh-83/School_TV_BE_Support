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
    public class LiveStreamDAO
    {
        private static LiveStreamDAO instance = null;
        private readonly DataContext _context;

        private LiveStreamDAO()
        {
            _context = new DataContext();
        }

        public static LiveStreamDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LiveStreamDAO();
                }
                return instance;
            }
        }

        public async Task<SchoolChannel?> GetSchoolChannelByIdAsync(int schoolChannelId)
        {
            return await _context.SchoolChannels.AsNoTracking().FirstOrDefaultAsync(s => s.SchoolChannelID == schoolChannelId);
        }
        public async Task<List<VideoHistory>> GetExpiredUploadedVideosAsync(DateTime currentTime)
        {
            return await _context.VideoHistories.AsNoTracking()
               .Where(v =>
                    v.Type != "Live" &&
                    v.Status == true &&
                    v.StreamAt.HasValue &&
                    v.Duration.HasValue &&
                    v.StreamAt.Value.AddMinutes(v.Duration.Value) <= currentTime)
                .ToListAsync();
        }

        public async Task<bool> AddVideoHistoryAsync(VideoHistory stream)
        {
            _context.VideoHistories.Add(stream);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateVideoHistoryAsync(VideoHistory stream)
        {
            _context.VideoHistories.Update(stream);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Schedule>> GetWaitingToStartStreamsAsync()
        {
            return await _context.Schedules.AsNoTracking()
              .Where(s => (s.Status == "Ready" || s.Status == "LateStart") && !s.LiveStreamStarted)
                .ToListAsync();
        }

        public async Task<Program> GetProgramByIdAsync(int id)
        {
            return await _context.Programs.AsNoTracking()
           .Include(p => p.SchoolChannel)
                .FirstOrDefaultAsync(p => p.ProgramID == id);
        }

        public async Task<bool> UpdateProgramAsync(Program program)
        {
            _context.Programs.Update(program);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddLikeAsync(VideoLike like)
        {
            _context.VideoLikes.Add(like);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddViewAsync(VideoView view)
        {
            _context.VideoViews.Add(view);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddShareAsync(Share share)
        {
            _context.Shares.Add(share);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<AdSchedule?> GetNextAvailableAdAsync()
        {
            return await _context.AdSchedules.AsNoTracking()
            .OrderBy(a => a.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesBySchoolChannelIdAsync(int schoolChannelId)
        {
            return await _context.Schedules
                .AsNoTracking()
                .Where(s => s.Program.SchoolChannel.SchoolChannelID == schoolChannelId)
                .ToListAsync();
        }

        public async Task<VideoHistory> GetVideoHistoryByStreamIdAsync(string cloudflareStreamId)
        {
            return await _context.VideoHistories.AsNoTracking()
            .FirstOrDefaultAsync(v => v.CloudflareStreamId == cloudflareStreamId);
        }

        public async Task<VideoHistory> GetLiveStreamByIdAsync(int id)
        {
            return await _context.VideoHistories.AsNoTracking()
            .Include(v => v.VideoViews)
                .Include(v => v.VideoLikes)
                .FirstOrDefaultAsync(v => v.VideoHistoryID == id);
        }

        public async Task<VideoHistory?> GetVideoHistoryByProgramIdAsync(int programId)
        {
            return await _context.VideoHistories.AsNoTracking()
            .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync(v => v.ProgramID == programId && v.Type == "Live");
        }

        public async Task<IEnumerable<VideoHistory>> GetActiveLiveStreamsAsync()
        {
            return await _context.VideoHistories
                .AsNoTracking()
                .Where(v => v.Status && v.Type == "Live")
                .ToListAsync();
        }

        public async Task<bool> CreateScheduleAsync(Schedule schedule)
        {
            Console.WriteLine($"[LiveStreamRepository] Creating schedule for ProgramID={schedule.ProgramID}, " +
                              $"IsReplay={schedule.IsReplay}, VideoHistoryID={schedule.VideoHistoryID}");

            _context.Schedules.Add(schedule);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> CreateProgramAsync(Program program)
        {
            _context.Programs.Add(program);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<VideoHistory> GetRecordedVideoByStreamIdAsync(string streamId)
        {
            return await _context.VideoHistories
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.CloudflareStreamId == streamId && v.Type == "Recorded");
        }

        public async Task<List<Schedule>> GetLateStartCandidatesAsync(DateTime currentTime)
        {
            return await _context.Schedules.AsNoTracking()
              .Where(s => s.Status == "Ready" && !s.LiveStreamStarted && s.StartTime.AddMinutes(2) <= currentTime)
                .ToListAsync();
        }

        public void UpdateSchedule(Schedule schedule)
        {
            var tracked = _context.Schedules.Local.FirstOrDefault(s => s.ScheduleID == schedule.ScheduleID);
            if (tracked != null)
            {
                tracked.Status = schedule.Status;
                tracked.LiveStreamStarted = schedule.LiveStreamStarted;
                tracked.LiveStreamEnded = schedule.LiveStreamEnded;
            }
            else
            {
                _context.Schedules.Attach(schedule);
                _context.Entry(schedule).State = EntityState.Modified;
            }
        }

        public async Task<List<Schedule>> GetLiveSchedulesAsync()
        {
            return await _context.Schedules.AsNoTracking()
              .Where(s => s.Status == "Live" && !s.LiveStreamEnded)
                .ToListAsync();
        }

        public async Task<List<Schedule>> GetOverdueSchedulesAsync(DateTime currentTime)
        {
            return await _context.Schedules.AsNoTracking()
              .Where(s => s.Status == "Live" && s.EndTime <= currentTime && !s.LiveStreamEnded)
                .ToListAsync();
        }

        public async Task<List<Schedule>> GetPendingSchedulesAsync(DateTime time)
        {
            return await _context.Schedules.AsNoTracking()
              .Where(s => s.Status == "Pending" && s.StartTime <= time)
                .ToListAsync();
        }

        public async Task<List<Schedule>> GetReadySchedulesAsync(DateTime time)
        {
            return await _context.Schedules.AsNoTracking()
              .Include(s => s.Program)
                .Where(s => s.Status == "Ready" && !s.LiveStreamStarted && s.StartTime <= time)
                .ToListAsync();
        }

        public async Task<List<Schedule>> GetEndingSchedulesAsync(DateTime time)
        {
            return await _context.Schedules.AsNoTracking()
              .Include(s => s.Program)
                .Where(s => s.LiveStreamStarted && !s.LiveStreamEnded && s.EndTime <= time)
                .ToListAsync();
        }

        public async Task<VideoHistory?> GetVideoHistoryByIdAsync(int id)
        {
            return await _context.VideoHistories.FindAsync(id);
        }

        public async Task<int?> GetFallbackAdVideoHistoryIdAsync()
        {
            return await _context.VideoHistories
                       .Where(v => v.Type == "Recorded" && v.Description.Contains("ad"))
                .Select(v => (int?)v.VideoHistoryID)
                .FirstOrDefaultAsync();
        }

        public async Task AddScheduleAsync(Schedule schedule)
        {
            await _context.Schedules.AddAsync(schedule);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<Schedule>> GetLateStartSchedulesPastEndTimeAsync(DateTime now)
        {
            return await _context.Schedules.AsNoTracking()
            .Where(s => s.Status == "LateStart" && s.EndTime < now && !s.LiveStreamEnded)
                .ToListAsync();
        }

        public async Task UpdateAsync(Schedule schedule)
        {
            _context.Schedules.Update(schedule);
            await Task.CompletedTask;
        }
    }
}
