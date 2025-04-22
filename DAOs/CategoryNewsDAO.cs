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
    public class CategoryNewsDAO
    {
        private static CategoryNewsDAO instance = null;
        private readonly DataContext _context;

        private CategoryNewsDAO()
        {
            _context = new DataContext();
        }

        public static CategoryNewsDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CategoryNewsDAO();
                }
                return instance;
            }
        }

        public async Task<IEnumerable<CategoryNews>> GetAllAsync()
        {
            return await _context.CategoryNews.Include(c => c.News).ToListAsync();
        }

        public async Task<CategoryNews?> GetByIdAsync(int id)
        {
            return await _context.CategoryNews.Include(c => c.News).FirstOrDefaultAsync(c => c.CategoryNewsID == id);
        }

        public async Task<CategoryNews> AddAsync(CategoryNews categoryNews)
        {
            if (categoryNews == null)
                throw new ArgumentNullException(nameof(categoryNews));

            categoryNews.CategoryNewsID = 0;
            _context.CategoryNews.Add(categoryNews);
            await _context.SaveChangesAsync();
            return categoryNews;
        }


        public async Task<bool> UpdateAsync(CategoryNews categoryNews)
        {
            var existingCategory = await _context.CategoryNews.FindAsync(categoryNews.CategoryNewsID);
            if (existingCategory == null) return false;

            _context.Entry(existingCategory).CurrentValues.SetValues(categoryNews);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var categoryNews = await _context.CategoryNews.FindAsync(id);
            if (categoryNews == null) return false;

            _context.CategoryNews.Remove(categoryNews);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
