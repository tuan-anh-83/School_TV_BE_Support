using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ICategoryNewsService
    {
        Task<IEnumerable<CategoryNews>> GetAllAsync();
        Task<CategoryNews?> GetByIdAsync(int id);
        Task<CategoryNews> AddAsync(CategoryNews categoryNews);
        Task<bool> UpdateAsync(CategoryNews categoryNews);
        Task<bool> DeleteAsync(int id);
    }
}
