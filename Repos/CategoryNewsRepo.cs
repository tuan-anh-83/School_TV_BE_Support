using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class CategoryNewsRepo : ICategoryNewsRepo
    {
        public async Task<CategoryNews> AddAsync(CategoryNews categoryNews)
        {
            return await CategoryNewsDAO.Instance.AddAsync(categoryNews);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await CategoryNewsDAO.Instance.DeleteAsync(id);
        }

        public async Task<IEnumerable<CategoryNews>> GetAllAsync()
        {
            return await CategoryNewsDAO.Instance.GetAllAsync();
        }

        public async Task<CategoryNews?> GetByIdAsync(int id)
        {
            return await CategoryNewsDAO.Instance.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(CategoryNews categoryNews)
        {
            return await CategoryNewsDAO.Instance.UpdateAsync(categoryNews);    
        }
    }
}
