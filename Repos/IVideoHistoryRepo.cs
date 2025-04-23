using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IVideoHistoryRepo
    {
        Task<List<VideoHistory>> GetAllVideosAsync();
        Task<VideoHistory?> GetVideoByIdAsync(int videoHistoryId);
        Task<bool> AddVideoAsync(VideoHistory videoHistory);
        Task<bool> UpdateVideoAsync(VideoHistory videoHistory);
        Task<bool> DeleteVideoAsync(int videoHistoryId);
        Task<int> CountByStatusAsync(bool status);
        Task<(int totalViews, int totalLikes)> GetTotalViewsAndLikesAsync();
        Task<int> CountByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> CountTotalVideosAsync();
        Task<VideoHistory?> GetLatestLiveStreamByProgramIdAsync(int programId);
        Task<List<VideoHistory>> GetAllVideoHistoriesAsync();
        Task<List<VideoHistory>> GetVideosByDateAsync(DateTime date);
        Task<VideoHistory?> GetReplayVideoByProgramAndTimeAsync(int programId, DateTime start, DateTime end);
        Task<VideoHistory?> GetReplayVideoAsync(int programId, DateTime startTime, DateTime endTime);
        Task<List<VideoHistory>> GetExpiredUploadedVideosAsync(DateTime currentTime);
        Task<List<VideoHistory>> GetVideosByProgramIdAsync(int programId);
        Task<List<VideoHistory>> GetVideosUploadedAfterAsync(DateTime timestamp);
    }
}
