using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IVideoViewService
    {
        Task<List<VideoView>> GetAllVideoViewsAsync();
        Task<VideoView?> GetVideoViewByIdAsync(int videoViewId);
        Task<bool> AddVideoViewAsync(VideoView videoView);
        Task<bool> UpdateVideoViewAsync(VideoView videoView);
        Task<bool> DeleteVideoViewAsync(int videoViewId);
        Task<int> GetTotalViewsForVideoAsync(int videoHistoryId);

        Task<int> GetTotalViewsAsync();
        Task<Dictionary<int, int>> GetViewsPerVideoAsync();
    }
}
