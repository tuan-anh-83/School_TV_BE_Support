using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IVideoLikeService
    {
        Task<List<VideoLike>> GetAllVideoLikesAsync();
        Task<VideoLike?> GetVideoLikeByIdAsync(int videoLikeId);
        Task<bool> AddVideoLikeAsync(VideoLike videoLike);
        Task<bool> UpdateVideoLikeAsync(VideoLike videoLike);
        Task<bool> DeleteVideoLikeAsync(int videoLikeId);


        Task<int> GetTotalLikesForVideoAsync(int videoHistoryId);
        Task<int> GetTotalLikesAsync();
        Task<int> GetLikesByVideoIdAsync(int videoHistoryId);
        Task<Dictionary<int, int>> GetLikesPerVideoAsync();
    }
}
