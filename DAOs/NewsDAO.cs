using BOs.Data;
using BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAOs
{
    public class NewsDAO
    {
        private static NewsDAO instance = null;
        private readonly DataContext _context;

        private NewsDAO()
        {
            _context = new DataContext();
        }

        public static NewsDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NewsDAO();
                }
                return instance;
            }
        }

        public async Task<int> CreateNewsAsync(News news, List<NewsPicture> pictures)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.News.Add(news);
                    await _context.SaveChangesAsync();

                    foreach (var picture in pictures)
                    {
                        picture.NewsID = news.NewsID;
                        _context.NewsPictures.Add(picture);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return news.NewsID;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }


        public async Task<int> UpdateNewsAsync(News news, List<NewsPicture> pictures)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existingNews = await _context.News
                        .Include(n => n.NewsPictures)
                        .FirstOrDefaultAsync(n => n.NewsID == news.NewsID);

                    if (existingNews == null)
                    {
                        return 0;
                    }

                    existingNews.Title = news.Title ?? existingNews.Title;
                    existingNews.Content = news.Content ?? existingNews.Content;
                    existingNews.UpdatedAt = DateTime.UtcNow;
                    existingNews.Status = news.Status;
                    existingNews.CategoryNewsID = news.CategoryNewsID;

                    if (pictures != null && pictures.Count > 0)
                    {
                        _context.NewsPictures.RemoveRange(existingNews.NewsPictures);
                        foreach (var picture in pictures)
                        {
                            picture.NewsID = news.NewsID;
                            _context.NewsPictures.Add(picture);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return existingNews.NewsID;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }


        public async Task<bool> ValidateSchoolChannelOwnershipAsync(int schoolChannelId, int accountId)
        {
            return await _context.SchoolChannels
                .AnyAsync(sc => sc.SchoolChannelID == schoolChannelId && sc.AccountID == accountId);
        }

        public async Task<List<News>> GetNewsByChannelIdAsync(int schoolChannelId)
        {
            return await _context.News
                .Where(n => n.SchoolChannelID == schoolChannelId && n.Status)
                .ToListAsync();
        }

        public async Task<News> GetNewsByIdAsync(int id)
        {
            return await _context.News
                .Include(n => n.NewsPictures)
                .Include(n => n.SchoolChannel)
                .FirstOrDefaultAsync(n => n.NewsID == id && n.Status);
        }

        public async Task<bool> DeleteNewsAsync(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null) return false;

            news.Status = false;
            news.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<News>> GetAllNewsAsync(bool? active)
        {
            var query = _context.News
                .Include(n => n.NewsPictures)
                .Include(n => n.SchoolChannel)
                .AsQueryable();

            if (active.HasValue)
                query = query.Where(n => n.Status == active.Value);

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<News>> GetNewsBySchoolChannelAsync(int schoolChannelId, int? accountId, bool isFollowing)
        {
            var query = _context.News
                .Include(n => n.NewsPictures)
                .Include(n => n.SchoolChannel)
                .Where(n => n.SchoolChannelID == schoolChannelId && n.Status == true)
                .AsQueryable();

            query = query.Where(n => !n.FollowerMode || (n.FollowerMode && isFollowing));

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<News>> GetActiveNewsWithFollowCheckAndWithoutFollowingAsync(int? accountId)
        {
            var query = _context.News
                .Include(n => n.NewsPictures)
                .Include(n => n.SchoolChannel)
                .Where(n => n.Status)
                .AsQueryable();

            if (accountId != null)
            {
                var followedChannels = _context.Follows
                    .Where(f => f.AccountID == accountId && f.Status == "Followed")
                    .Select(f => f.SchoolChannelID);

                query = query.Where(n => followedChannels.Contains(n.SchoolChannelID) || !n.FollowerMode);
            }
            else
            {
                query = query.Where(n => !n.FollowerMode);
            }

            return await query.ToListAsync();
        }

        public async Task<Dictionary<DateTime, int>> GetDailyNewsStatisticsAsync()
        {
            return await _context.News
                .Where(n => n.Status)
                .GroupBy(n => n.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Date, g => g.Count);
        }

    }
}
