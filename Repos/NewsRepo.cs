using BOs.Models;
using DAOs;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class NewsRepo : INewsRepo
    {
        public async Task<int> CreateNewsAsync(News news, List<NewsPicture> pictures)
        {
            return await NewsDAO.Instance.CreateNewsAsync(news, pictures);
        }

        public async Task<bool> DeleteNewsAsync(int id)
        {
            return await NewsDAO.Instance.DeleteNewsAsync(id);
        }

        public async Task<IEnumerable<News>> GetActiveNewsWithFollowCheckAndWithoutFollowingAsync(int? accountId)
        {
            return await NewsDAO.Instance.GetActiveNewsWithFollowCheckAndWithoutFollowingAsync(accountId);
        }

        public async Task<IEnumerable<News>> GetAllNewsAsync(bool? active)
        {
            return await NewsDAO.Instance.GetAllNewsAsync(active);
        }

        public async Task<Dictionary<DateTime, int>> GetDailyNewsStatisticsAsync()
        {
            return await NewsDAO.Instance.GetDailyNewsStatisticsAsync();
        }

        public async Task<List<News>> GetNewsByChannelIdAsync(int schoolChannelId)
        {
            return await NewsDAO.Instance.GetNewsByChannelIdAsync(schoolChannelId);
        }

        public async Task<News> GetNewsByIdAsync(int id)
        {
            return await NewsDAO.Instance.GetNewsByIdAsync(id);
        }

        public async Task<IEnumerable<News>> GetNewsBySchoolChannelAsync(int schoolChannelId, int? accountId, bool isFollowing)
        {
            return await NewsDAO.Instance.GetNewsBySchoolChannelAsync(schoolChannelId, accountId, isFollowing);
        }

        public async Task<int> UpdateNewsAsync(News news, List<NewsPicture> pictures)
        {
            return await NewsDAO.Instance.UpdateNewsAsync(news, pictures);
        }

        public async Task<bool> ValidateSchoolChannelOwnershipAsync(int schoolChannelId, int accountId)
        {
            return await NewsDAO.Instance.ValidateSchoolChannelOwnershipAsync(schoolChannelId, accountId);
        }
    }
}
