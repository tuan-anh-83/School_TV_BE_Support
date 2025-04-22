using BOs.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface INewsService
    {
        Task<int> CreateNewsAsync(News news, List<IFormFile> imageFiles);
        Task<int> UpdateNewsAsync(News news, List<IFormFile> imageFiles);
        Task<List<News>> GetNewsByChannelAsync(int schoolChannelId);
        Task<bool> ValidateSchoolChannelOwnershipAsync(int schoolChannelId, int accountId);
        Task<News> GetNewsByIdAsync(int id);
        Task<bool> DeleteNewsAsync(int id);
        Task<IEnumerable<News>> GetAllNewsAsync(bool? active);
        Task<IEnumerable<News>> GetActiveNewsAsync();
        Task<IEnumerable<News>> GetAllNewsNoFilterAsync();
        Task<IEnumerable<News>> GetNewsBySchoolChannelAsync(int schoolChannelId, int? accountId, bool isFollowing);
        Task<IEnumerable<News>> GetActiveNewsWithFollowCheckAndWithoutFollowingAsync(int? accountId);
        Task<Dictionary<DateTime, int>> GetDailyNewsStatisticsAsync();
    }
}
