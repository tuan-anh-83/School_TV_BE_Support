using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class VideoHistoryRepo : IVideoHistoryRepo
    {
        public async Task<bool> AddVideoAsync(VideoHistory videoHistory)
        {
            return await VideoHistoryDAO.Instance.AddVideoAsync(videoHistory);
        }

        public async Task<int> CountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await VideoHistoryDAO.Instance.CountByDateRangeAsync(startDate, endDate);
        }

        public async Task<int> CountByStatusAsync(bool status)
        {
            return await VideoHistoryDAO.Instance.CountByStatusAsync(status);
        }

        public async Task<int> CountTotalVideosAsync()
        {
            return await VideoHistoryDAO.Instance.CountTotalVideosAsync();
        }

        public async Task<bool> DeleteVideoAsync(int videoHistoryId)
        {
            return await VideoHistoryDAO.Instance.DeleteVideoAsync(videoHistoryId);
        }

        public async Task<List<VideoHistory>> GetAllVideoHistoriesAsync()
        {
            return await VideoHistoryDAO.Instance.GetAllVideoHistoriesAsync();
        }

        public async Task<List<VideoHistory>> GetAllVideosAsync()
        {
            return await VideoHistoryDAO.Instance.GetAllVideosAsync();
        }

        public async Task<List<VideoHistory>> GetExpiredUploadedVideosAsync(DateTime currentTime)
        {
            return await VideoHistoryDAO.Instance.GetExpiredUploadedVideosAsync(currentTime);
        }

        public async Task<VideoHistory?> GetLatestLiveStreamByProgramIdAsync(int programId)
        {
            return await VideoHistoryDAO.Instance.GetLatestLiveStreamByProgramIdAsync(programId);  
        }

        public async Task<VideoHistory?> GetReplayVideoAsync(int programId, DateTime startTime, DateTime endTime)
        {
            return await VideoHistoryDAO.Instance.GetReplayVideoAsync(programId, startTime, endTime);
        }

        public async Task<VideoHistory?> GetReplayVideoByProgramAndTimeAsync(int programId, DateTime start, DateTime end)
        {
            return await VideoHistoryDAO.Instance.GetReplayVideoByProgramAndTimeAsync(programId, start, end);
        }

        public async Task<(int totalViews, int totalLikes)> GetTotalViewsAndLikesAsync()
        {
            return await VideoHistoryDAO.Instance.GetTotalViewsAndLikesAsync();
        }

        public async Task<VideoHistory?> GetVideoByIdAsync(int videoHistoryId)
        {
            return await VideoHistoryDAO.Instance.GetVideoByIdAsync(videoHistoryId);
        }

        public async Task<List<VideoHistory>> GetVideosByDateAsync(DateTime date)
        {
            return await VideoHistoryDAO.Instance.GetVideosByDateAsync(date);
        }

        public async Task<List<VideoHistory>> GetVideosByProgramIdAsync(int programId)
        {
            return await VideoHistoryDAO.Instance.GetVideosByProgramIdAsync(programId);
        }

        public async Task<List<VideoHistory>> GetVideosUploadedAfterAsync(DateTime timestamp)
        {
            return await VideoHistoryDAO.Instance.GetVideosUploadedAfterAsync(timestamp);
        }

        public async Task<bool> UpdateVideoAsync(VideoHistory videoHistory)
        {
            return await VideoHistoryDAO.Instance.UpdateVideoAsync(videoHistory);
        }
    }
}
