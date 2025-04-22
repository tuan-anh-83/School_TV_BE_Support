using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface INewsRepo
    {
        Task<int> CreateNewsAsync(News news, List<NewsPicture> pictures);
        Task<int> UpdateNewsAsync(News news, List<NewsPicture> pictures);
        Task<bool> ValidateSchoolChannelOwnershipAsync(int schoolChannelId, int accountId);
        Task<List<News>> GetNewsByChannelIdAsync(int schoolChannelId);
        Task<News> GetNewsByIdAsync(int id);
        Task<bool> DeleteNewsAsync(int id);
        Task<IEnumerable<News>> GetAllNewsAsync(bool? active);
        Task<IEnumerable<News>> GetActiveNewsWithFollowCheckAndWithoutFollowingAsync(int? accountId);
        Task<IEnumerable<News>> GetNewsBySchoolChannelAsync(int schoolChannelId, int? accountId, bool isFollowing);
        Task<Dictionary<DateTime, int>> GetDailyNewsStatisticsAsync();
    }
}
