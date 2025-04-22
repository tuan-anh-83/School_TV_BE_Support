using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class CategoryNewsService : ICategoryNewsService
    {
        private readonly ICategoryNewsRepo _categoryNewsRepository;

        public CategoryNewsService(ICategoryNewsRepo categoryNewsRepository)
        {
            _categoryNewsRepository = categoryNewsRepository;
        }

        public async Task<IEnumerable<CategoryNews>> GetAllAsync()
        {
            return await _categoryNewsRepository.GetAllAsync();
        }

        public async Task<CategoryNews?> GetByIdAsync(int id)
        {
            return await _categoryNewsRepository.GetByIdAsync(id);
        }

        public async Task<CategoryNews> AddAsync(CategoryNews categoryNews)
        {
            return await _categoryNewsRepository.AddAsync(categoryNews);
        }

        public async Task<bool> UpdateAsync(CategoryNews categoryNews)
        {
            return await _categoryNewsRepository.UpdateAsync(categoryNews);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _categoryNewsRepository.DeleteAsync(id);
        }
    }
}
