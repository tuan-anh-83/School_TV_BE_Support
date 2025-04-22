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
    public class VideoLikeDAO
    {
        private static VideoLikeDAO instance = null;
        private readonly DataContext _context;

        private VideoLikeDAO()
        {
            _context = new DataContext();
        }

        public static VideoLikeDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new VideoLikeDAO();
                }
                return instance;
            }
        }

        public async Task<List<VideoLike>> GetAllVideoLikesAsync()
        {
            return await _context.VideoLikes
                .Include(vl => vl.VideoHistory)
                .ToListAsync();
        }

        public async Task<VideoLike?> GetVideoLikeByIdAsync(int videoLikeId)
        {
            return await _context.VideoLikes
                .Include(vl => vl.VideoHistory)
                .FirstOrDefaultAsync(vl => vl.LikeID == videoLikeId);
        }

        public async Task<bool> AddVideoLikeAsync(VideoLike videoLike)
        {
            videoLike.Quantity = 1;
            bool vhExists = await _context.VideoHistories
                .AnyAsync(v => v.VideoHistoryID == videoLike.VideoHistoryID);
            if (!vhExists) return false;

            bool accountExists = await _context.Accounts
                .AnyAsync(a => a.AccountID == videoLike.AccountID);
            if (!accountExists) return false;

            await _context.VideoLikes.AddAsync(videoLike);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateVideoLikeAsync(VideoLike videoLike)
        {
            var existingVideoLike = await GetVideoLikeByIdAsync(videoLike.LikeID);
            if (existingVideoLike == null)
                return false;

            _context.Entry(existingVideoLike).CurrentValues.SetValues(videoLike);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVideoLikeAsync(int videoLikeId)
        {
            var videoLike = await GetVideoLikeByIdAsync(videoLikeId);
            if (videoLike == null)
                return false;

            videoLike.Quantity = 0;
            _context.VideoLikes.Update(videoLike);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalLikesForVideoAsync(int videoHistoryId)
        {
            return await _context.VideoLikes
                 .Where(s => s.VideoHistoryID == videoHistoryId && s.Quantity > 0)
                 .SumAsync(s => s.Quantity);
        }

        public async Task<int> CountTotalLikesAsync()
        {
            return await _context.VideoLikes
              .Where(v => v.Quantity > 0)
              .SumAsync(v => v.Quantity);
        }

        public async Task<int> CountLikesByVideoIdAsync(int videoHistoryId)
        {
            return await _context.VideoLikes
                .Where(v => v.VideoHistoryID == videoHistoryId && v.Quantity > 0)
                .SumAsync(v => v.Quantity);
        }

        public async Task<Dictionary<int, int>> GetLikesCountPerVideoAsync()
        {
            return await _context.VideoLikes
                .Where(v => v.Quantity > 0)
                .GroupBy(v => v.VideoHistoryID)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Sum(v => v.Quantity)
                );
        }
    }
}
