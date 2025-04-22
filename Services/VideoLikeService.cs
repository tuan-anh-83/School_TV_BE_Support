using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class VideoLikeService : IVideoLikeService
    {
        private readonly IVideoLikeRepo _videoLikeRepo;

        public VideoLikeService(IVideoLikeRepo videoLikeRepo)
        {
            _videoLikeRepo = videoLikeRepo;
        }
        public async Task<List<VideoLike>> GetAllVideoLikesAsync()
        {
            return await _videoLikeRepo.GetAllVideoLikesAsync();
        }

        public async Task<VideoLike?> GetVideoLikeByIdAsync(int videoLikeId)
        {
            return await _videoLikeRepo.GetVideoLikeByIdAsync(videoLikeId);
        }

        public async Task<bool> AddVideoLikeAsync(VideoLike videoLike)
        {
            return await _videoLikeRepo.AddVideoLikeAsync(videoLike);
        }

        public async Task<bool> UpdateVideoLikeAsync(VideoLike videoLike)
        {
            return await _videoLikeRepo.UpdateVideoLikeAsync(videoLike);
        }

        public async Task<bool> DeleteVideoLikeAsync(int videoLikeId)
        {
            return await _videoLikeRepo.DeleteVideoLikeAsync(videoLikeId);
        }
        public async Task<int> GetTotalLikesForVideoAsync(int videoHistoryId)
        {
            return await _videoLikeRepo.GetTotalLikesForVideoAsync(videoHistoryId);
        }

        public async Task<int> GetTotalLikesAsync()
        {
            return await _videoLikeRepo.CountTotalLikesAsync();
        }

        public async Task<int> GetLikesByVideoIdAsync(int videoHistoryId)
        {
            return await _videoLikeRepo.CountLikesByVideoIdAsync(videoHistoryId);
        }

        public async Task<Dictionary<int, int>> GetLikesPerVideoAsync()
        {
            return await _videoLikeRepo.GetLikesCountPerVideoAsync();
        }


    }
}
