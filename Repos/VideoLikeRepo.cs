using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class VideoLikeRepo : IVideoLikeRepo
    {
        public async Task<bool> AddVideoLikeAsync(VideoLike videoLike)
        {
            return await VideoLikeDAO.Instance.AddVideoLikeAsync(videoLike);
        }

        public async Task<int> CountLikesByVideoIdAsync(int videoHistoryId)
        {
            return await VideoLikeDAO.Instance.CountLikesByVideoIdAsync(videoHistoryId);
        }

        public async Task<int> CountTotalLikesAsync()
        {
            return await VideoLikeDAO.Instance.CountTotalLikesAsync();
        }

        public async Task<bool> DeleteVideoLikeAsync(int videoLikeId)
        {
            return await VideoLikeDAO.Instance.DeleteVideoLikeAsync(videoLikeId);   
        }

        public async Task<List<VideoLike>> GetAllVideoLikesAsync()
        {
            return await VideoLikeDAO.Instance.GetAllVideoLikesAsync();
        }

        public async Task<Dictionary<int, int>> GetLikesCountPerVideoAsync()
        {
            return await VideoLikeDAO.Instance.GetLikesCountPerVideoAsync();
        }

        public async Task<int> GetTotalLikesForVideoAsync(int videoHistoryId)
        {
            return await VideoLikeDAO.Instance.GetTotalLikesForVideoAsync(videoHistoryId);   
        }

        public async Task<VideoLike?> GetVideoLikeByIdAsync(int videoLikeId)
        {
            return await VideoLikeDAO.Instance.GetVideoLikeByIdAsync(videoLikeId);
        }

        public async Task<bool> UpdateVideoLikeAsync(VideoLike videoLike)
        {
            return await VideoLikeDAO.Instance.UpdateVideoLikeAsync(videoLike);
        }
    }
}
