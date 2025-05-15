using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class VideoViewService : IVideoViewService
    {
        private readonly IVideoViewRepo _videoViewRepo;

        public VideoViewService(IVideoViewRepo videoViewRepo)
        {
            _videoViewRepo = videoViewRepo;
        }

        public async Task<List<VideoView>> GetAllVideoViewsAsync()
        {
            return await _videoViewRepo.GetAllVideoViewsAsync();
        }

        public async Task<VideoView?> GetVideoViewByIdAsync(int videoViewId)
        {
            return await _videoViewRepo.GetVideoViewByIdAsync(videoViewId);
        }

        public async Task<bool> AddVideoViewAsync(VideoView videoView)
        {
            return await _videoViewRepo.AddVideoViewAsync(videoView);
        }

        public async Task<bool> UpdateVideoViewAsync(VideoView videoView)
        {
            return await _videoViewRepo.UpdateVideoViewAsync(videoView);
        }

        public async Task<bool> DeleteVideoViewAsync(int videoViewId)
        {
            return await _videoViewRepo.DeleteVideoViewAsync(videoViewId);
        }

        public async Task<int> GetTotalViewsForVideoAsync(int videoHistoryId)
        {
            return await _videoViewRepo.GetTotalViewsForVideoAsync(videoHistoryId);
        }

        public async Task<int> GetTotalViewsAsync()
        {
            return await _videoViewRepo.CountTotalViewsAsync();
        }

        public async Task<Dictionary<int, int>> GetViewsPerVideoAsync()
        {
            return await _videoViewRepo.GetViewsCountPerVideoAsync();
        }

        public async Task<int> GetTotalViewsByChannelAsync(int channelId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return await _videoViewRepo.GetTotalViewsByChannelAsync(channelId, startDate, endDate);
        }

        public async Task<decimal> GetViewsComparisonPercentAsync(int channelId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return await _videoViewRepo.GetViewsComparisonPercentAsync(channelId, startDate, endDate);
        }
    }
}
