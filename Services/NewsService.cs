using BOs.Models;
using Microsoft.AspNetCore.Http;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class NewsService : INewsService
    {
        private readonly INewsRepo _newsRepo;

        public NewsService(INewsRepo newsRepo)
        {
            _newsRepo = newsRepo;
        }

        public async Task<int> CreateNewsAsync(News news, List<IFormFile> imageFiles)
        {
            var pictures = new List<NewsPicture>();

            foreach (var file in imageFiles)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                pictures.Add(new NewsPicture
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileData = ms.ToArray()
                });
            }

            return await _newsRepo.CreateNewsAsync(news, pictures);
        }

        public async Task<int> UpdateNewsAsync(News news, List<IFormFile> imageFiles)
        {
            var pictures = new List<NewsPicture>();

            if (imageFiles != null && imageFiles.Count > 0)
            {
                foreach (var file in imageFiles)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    pictures.Add(new NewsPicture
                    {
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        FileData = ms.ToArray()
                    });
                }
            }

            return await _newsRepo.UpdateNewsAsync(news, pictures);
        }

        public async Task<List<News>> GetNewsByChannelAsync(int schoolChannelId)
        {
            return await _newsRepo.GetNewsByChannelIdAsync(schoolChannelId);
        }

        public async Task<bool> ValidateSchoolChannelOwnershipAsync(int schoolChannelId, int accountId)
        {
            return await _newsRepo.ValidateSchoolChannelOwnershipAsync(schoolChannelId, accountId);
        }

        public async Task<News> GetNewsByIdAsync(int id)
        {
            return await _newsRepo.GetNewsByIdAsync(id);
        }

        public async Task<bool> DeleteNewsAsync(int id)
        {
            return await _newsRepo.DeleteNewsAsync(id);
        }
        public async Task<IEnumerable<News>> GetAllNewsAsync(bool? active)
        {
            return await _newsRepo.GetAllNewsAsync(active);
        }
        public async Task<IEnumerable<News>> GetActiveNewsAsync()
        {
            return await _newsRepo.GetAllNewsAsync(true);
        }
        public async Task<IEnumerable<News>> GetAllNewsNoFilterAsync()
        {
            return await _newsRepo.GetAllNewsAsync(null);
        }

        public async Task<IEnumerable<News>> GetNewsBySchoolChannelAsync(int schoolChannelId, int? accountId, bool isFollowing)
        {
            return await _newsRepo.GetNewsBySchoolChannelAsync(schoolChannelId, accountId, isFollowing);
        }
        public async Task<IEnumerable<News>> GetActiveNewsWithFollowCheckAndWithoutFollowingAsync(int? accountId)
        {
            return await _newsRepo.GetActiveNewsWithFollowCheckAndWithoutFollowingAsync(accountId);
        }

        public async Task<Dictionary<DateTime, int>> GetDailyNewsStatisticsAsync()
        {
            return await _newsRepo.GetDailyNewsStatisticsAsync();
        }


    }
}
