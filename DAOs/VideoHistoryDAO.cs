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
    public class VideoHistoryDAO
    {
        private static VideoHistoryDAO instance = null;
        private readonly DataContext _context;

        private VideoHistoryDAO()
        {
            _context = new DataContext();
        }

        public static VideoHistoryDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new VideoHistoryDAO();
                }
                return instance;
            }
        }

        public async Task<List<VideoHistory>> GetVideosByProgramIdAsync(int programId)
        {
            return await _context.VideoHistories.AsNoTracking()
                .Where(v => v.ProgramID == programId && v.Status)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
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


        public async Task<List<VideoHistory>> GetAllVideosAsync()
        {
            return await _context.VideoHistories
                .Include(v => v.Schedules)
                .Where(v => v.Status)
                .Include(v => v.Program)
                    .ThenInclude(p => p.SchoolChannel)
                        .ThenInclude(sc => sc.Account)
                .AsNoTracking()
                .Where(v => v.Program != null && v.Program.SchoolChannel != null && v.Program.SchoolChannel.Account != null && v.Program.SchoolChannel.Account.Status.Trim().ToLower() == "active")
                .ToListAsync();
        }

        public async Task<VideoHistory?> GetVideoByIdAsync(int id)
        {
            return await _context.VideoHistories.AsNoTracking()
           .Include(v => v.Program)
                .ThenInclude(p => p.SchoolChannel)
                .FirstOrDefaultAsync(v => v.VideoHistoryID == id);
        }

        public async Task<VideoHistory?> GetLatestLiveStreamByProgramIdAsync(int programId)
        {
            return await _context.VideoHistories.AsNoTracking()
           .Where(v => v.ProgramID == programId && v.Type == "Live" && v.Status)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> AddVideoAsync(VideoHistory video)
        {
            await _context.VideoHistories.AddAsync(video);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<VideoHistory?> AddAndReturnVideoAsync(VideoHistory video)
        {
            await _context.VideoHistories.AddAsync(video);
            if (await _context.SaveChangesAsync() > 0)
            {
                return await _context.VideoHistories
                    .Include(v => v.Program)
                    .FirstOrDefaultAsync(v => v.VideoHistoryID == video.VideoHistoryID);
                }
            return null;
        }

        public async Task<bool> UpdateVideoAsync(VideoHistory video)
        {
            _context.VideoHistories.Update(video);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteVideoAsync(int id)
        {
            var video = await _context.VideoHistories.FindAsync(id);
            if (video == null) return false;

            video.Status = false;
            _context.VideoHistories.Update(video);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<VideoHistory>> GetAllVideoHistoriesAsync()
        {
            return await _context.VideoHistories
          .Include(v => v.Program)
                .ThenInclude(p => p.SchoolChannel)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountTotalVideosAsync()
        {
            return await _context.VideoHistories.CountAsync();
        }

        public async Task<int> CountByStatusAsync(bool status)
        {
            return await _context.VideoHistories.CountAsync(v => v.Status == status);
        }

        public async Task<(int, int)> GetTotalViewsAndLikesAsync()
        {
            int views = await _context.VideoViews.AsNoTracking().CountAsync();
            int likes = await _context.VideoLikes.AsNoTracking().CountAsync();
            return (views, likes);
        }

        public async Task<int> CountByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.VideoHistories.AsNoTracking()
                .CountAsync(v => v.CreatedAt >= start && v.CreatedAt <= end);
        }

        public async Task<List<VideoHistory>> GetVideosByDateAsync(DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);
            return await _context.VideoHistories.AsNoTracking()
        .Include(v => v.Program)
                .Where(v => v.CreatedAt >= start && v.CreatedAt < end)
                .ToListAsync();
        }

        public async Task<VideoHistory?> GetReplayVideoByProgramAndTimeAsync(int programId, DateTime start, DateTime end)
        {
            return await _context.VideoHistories.AsNoTracking()
           .Where(v =>
                    v.ProgramID == programId &&
                    v.Status == true &&
                    v.Type != "Live" &&
                    v.CreatedAt >= start &&
                    v.CreatedAt <= end)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<VideoHistory?> GetReplayVideoAsync(int programId, DateTime start, DateTime end)
        {
            return await GetReplayVideoByProgramAndTimeAsync(programId, start, end);
        }
        public async Task<List<VideoHistory>> GetVideosUploadedAfterAsync(DateTime timestamp)
        {
            return await _context.VideoHistories.AsNoTracking()
            .Include(v => v.Program)
                .Where(v => v.CreatedAt >= timestamp && v.Duration != null)
                .ToListAsync();
        }

        public Task<List<VideoHistory>> GetActiveUnconfirmedStreamsAsync()
        {
            return _context.VideoHistories
                .Where(v => v.Status && v.Type == "Ready" && v.CloudflareStreamId != null)
                .ToListAsync();
        }

        public Task<List<VideoHistory>> GetActiveStreamsAsync()
        {
            return _context.VideoHistories
                .Where(v => v.Status && v.Type == "Live" && v.CloudflareStreamId != null)
                .ToListAsync();
        }

        public async Task<double> GetTotalWatchTimeByChannelAsync(int channelId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            // Assuming VideoHistory table has WatchDurationSeconds field that tracks how long a user watched a video
            var totalWatchTimeSeconds = await _context.VideoHistories
                .AsNoTracking()
                .Where(vh => vh.Program.SchoolChannelID == channelId)
                .Where(vh => vh.StreamAt >= startDate && vh.StreamAt <= endDate)
                .Where(vh => vh.Duration > 0)
                .SumAsync(vh => vh.Duration);

            // Convert seconds to hours
            double totalWatchTimeHours = (totalWatchTimeSeconds ?? 0) / 3600.0;

            // Round to one decimal place
            return Math.Round(totalWatchTimeHours, 1);
        }

        public async Task<decimal> GetWatchTimeComparisonPercentAsync(int channelId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            // Calculate previous period of the same length
            var periodLength = (endDate - startDate).TotalDays;
            var previousStartDate = startDate.AddDays(-periodLength);
            var previousEndDate = startDate.AddSeconds(-1);

            var currentPeriodWatchTime = await GetTotalWatchTimeByChannelAsync(channelId, startDate, endDate);
            var previousPeriodWatchTime = await GetTotalWatchTimeByChannelAsync(channelId, previousStartDate, previousEndDate);

            // Handle zero division and calculate percentage change
            if (previousPeriodWatchTime == 0)
                return currentPeriodWatchTime > 0 ? 100 : 0;

            decimal percentChange = (decimal)((currentPeriodWatchTime - previousPeriodWatchTime) / previousPeriodWatchTime * 100);

            // Round to one decimal place
            return Math.Round(percentChange, 1);
        }

        public async Task<List<VideoHistory>> GetAllVideosByChannelAsync(int channelId)
        {
            return await _context.VideoHistories.AsNoTracking()
                .Include(v => v.Program)
                .Include(v => v.VideoLikes)
                .Include(v => v.Shares)
                .Include(v => v.VideoViews)
                .Include(v => v.Comments)
                .Where(v => v.Program.SchoolChannelID == channelId)
                .ToListAsync();
        }
    }
}
