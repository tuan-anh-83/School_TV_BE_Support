using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class VideoViewRepo : IVideoViewRepo
    {
        public async Task<bool> AddVideoViewAsync(VideoView videoView)
        {
            return await VideoViewDAO.Instance.AddVideoViewAsync(videoView);
        }

        public async Task<int> CountTotalViewsAsync()
        {
            return await VideoViewDAO.Instance.CountTotalViewsAsync();
        }

        public async Task<bool> DeleteVideoViewAsync(int videoViewId)
        {
            return await VideoViewDAO.Instance.DeleteVideoViewAsync(videoViewId);   
        }

        public async Task<List<VideoView>> GetAllVideoViewsAsync()
        {
            return await VideoViewDAO.Instance.GetAllVideoViewsAsync();
        }

        public async Task<int> GetTotalViewsForVideoAsync(int videoHistoryId)
        {
            return await VideoViewDAO.Instance.GetTotalViewsForVideoAsync(videoHistoryId);
        }

        public async Task<VideoView?> GetVideoViewByIdAsync(int videoViewId)
        {
            return await VideoViewDAO.Instance.GetVideoViewByIdAsync(videoViewId);
        }

        public async Task<Dictionary<int, int>> GetViewsCountPerVideoAsync()
        {
            return await VideoViewDAO.Instance.GetViewsCountPerVideoAsync();
        }

        public async Task<bool> UpdateVideoViewAsync(VideoView videoView)
        {
            return await VideoViewDAO.Instance.AddVideoViewAsync(videoView);
        }
    }
}
