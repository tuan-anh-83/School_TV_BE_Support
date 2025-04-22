using BOs.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IVideoHistoryService
    {
        Task<List<VideoHistory>> GetAllVideosAsync();
        Task<VideoHistory?> GetVideoByIdAsync(int videoHistoryId);
        Task<VideoHistory?> GetLatestLiveStreamByProgramIdAsync(int programId);
        Task<bool> UpdateVideoAsync(VideoHistory videoHistory);
        Task<bool> DeleteVideoAsync(int videoHistoryId);
        Task<int> GetTotalVideosAsync();
        Task<int> GetTotalVideosByStatusAsync(bool status);
        Task<(int totalViews, int totalLikes)> GetTotalViewsAndLikesAsync();
        Task<int> GetVideosByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<VideoHistory>> GetAllVideoHistoriesAsync();
        Task<List<VideoHistory>> GetVideosByDateAsync(DateTime date);
        Task<bool> AddVideoWithCloudflareAsync(IFormFile videoFile, VideoHistory videoHistory);
        Task<VideoHistory?> GetReplayVideoByProgramAndTimeAsync(int programId, DateTime start, DateTime end);
        Task<VideoHistory?> GetReplayVideoAsync(int programId, DateTime startTime, DateTime endTime);
        Task<List<VideoHistory>> GetVideosByProgramIdAsync(int programId);
    }
}
