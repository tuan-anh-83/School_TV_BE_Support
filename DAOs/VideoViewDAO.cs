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
    public class VideoViewDAO
    {
        private static VideoViewDAO instance = null;
        private readonly DataContext _context;

        private VideoViewDAO()
        {
            _context = new DataContext();
        }

        public static VideoViewDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new VideoViewDAO();
                }
                return instance;
            }
        }

        public async Task<List<VideoView>> GetAllVideoViewsAsync()
        {
            return await _context.VideoViews.AsNoTracking()
                .Include(v => v.VideoHistory)
                .ToListAsync();
        }

        public async Task<VideoView?> GetVideoViewByIdAsync(int videoViewId)
        {
                .Include(v => v.VideoHistory)
                .FirstOrDefaultAsync(v => v.ViewID == videoViewId);
        }

        public async Task<bool> AddVideoViewAsync(VideoView videoView)
        {
                .AnyAsync(v => v.VideoHistoryID == videoView.VideoHistoryID);
            if (!vhExists) return false;

                .AnyAsync(a => a.AccountID == videoView.AccountID);
            if (!accountExists) return false;

            _context.VideoViews.Add(videoView);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateVideoViewAsync(VideoView videoView)
        {
            var existingVideoView = await GetVideoViewByIdAsync(videoView.ViewID);
            if (existingVideoView == null)
                return false;

            _context.Entry(existingVideoView).CurrentValues.SetValues(videoView);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVideoViewAsync(int videoViewId)
        {
            var videoView = await GetVideoViewByIdAsync(videoViewId);
            if (videoView == null)
                return false;

            videoView.Quantity = 0;
            _context.VideoViews.Update(videoView);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalViewsForVideoAsync(int videoHistoryId)
        {
                 .Where(v => v.VideoHistoryID == videoHistoryId && v.Quantity > 0)
                 .SumAsync(v => v.Quantity);
        }

        public async Task<int> CountTotalViewsAsync()
        {
                .Where(v => v.Quantity > 0)
                .SumAsync(v => v.Quantity);
        }

        public async Task<Dictionary<int, int>> GetViewsCountPerVideoAsync()
        {
                .Where(v => v.Quantity > 0)
                .GroupBy(v => v.VideoHistoryID)
                .Select(g => new { VideoId = g.Key, TotalViews = g.Sum(v => v.Quantity) })
                .ToDictionaryAsync(g => g.VideoId, g => g.TotalViews);
        }
    }
}
