using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IVideoLikeRepo
    {
        Task<List<VideoLike>> GetAllVideoLikesAsync();
        Task<VideoLike?> GetVideoLikeByIdAsync(int videoLikeId);
        Task<bool> AddVideoLikeAsync(VideoLike videoLike);
        Task<bool> UpdateVideoLikeAsync(VideoLike videoLike);
        Task<bool> DeleteVideoLikeAsync(int videoLikeId);
        Task<int> GetTotalLikesForVideoAsync(int videoHistoryId);

        Task<int> CountTotalLikesAsync();
        Task<int> CountLikesByVideoIdAsync(int videoHistoryId);
        Task<Dictionary<int, int>> GetLikesCountPerVideoAsync();
    }
}
