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
            return await _context.VideoHistories
                .Where(v => v.ProgramID == programId && v.Status)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VideoHistory>> GetExpiredUploadedVideosAsync(DateTime currentTime)
        {
            return await _context.VideoHistories
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
                .Where(v => v.Status)
                .Include(v => v.Program)
                .ThenInclude(p => p.SchoolChannel)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<VideoHistory?> GetVideoByIdAsync(int id)
        {
            return await _context.VideoHistories
                .Include(v => v.Program)
                .ThenInclude(p => p.SchoolChannel)
                .FirstOrDefaultAsync(v => v.VideoHistoryID == id);
        }

        public async Task<VideoHistory?> GetLatestLiveStreamByProgramIdAsync(int programId)
        {
            return await _context.VideoHistories
                .Where(v => v.ProgramID == programId && v.Type == "Live" && v.Status)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> AddVideoAsync(VideoHistory video)
        {
            _context.VideoHistories.Add(video);
            return await _context.SaveChangesAsync() > 0;
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
            int views = await _context.VideoViews.CountAsync();
            int likes = await _context.VideoLikes.CountAsync();
            return (views, likes);
        }

        public async Task<int> CountByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.VideoHistories
                .CountAsync(v => v.CreatedAt >= start && v.CreatedAt <= end);
        }

        public async Task<List<VideoHistory>> GetVideosByDateAsync(DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);
            return await _context.VideoHistories
                .Include(v => v.Program)
                .Where(v => v.CreatedAt >= start && v.CreatedAt < end)
                .ToListAsync();
        }

        public async Task<VideoHistory?> GetReplayVideoByProgramAndTimeAsync(int programId, DateTime start, DateTime end)
        {
            return await _context.VideoHistories
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
            return await _context.VideoHistories
                .Include(v => v.Program)
                .Where(v => v.CreatedAt >= timestamp && v.Duration != null)
                .ToListAsync();
        }
    }
}
