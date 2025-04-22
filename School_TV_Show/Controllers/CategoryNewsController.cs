using BOs.Models;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace School_TV_Show.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryNewsController : ControllerBase
    {
        private readonly ICategoryNewsService _categoryNewsService;

        public CategoryNewsController(ICategoryNewsService categoryNewsService)
        {
            _categoryNewsService = categoryNewsService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryNews>>> GetAll()
        {
            return Ok(await _categoryNewsService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryNews>> GetById(int id)
        {
            var categoryNews = await _categoryNewsService.GetByIdAsync(id);
            if (categoryNews == null) return NotFound();
            return Ok(categoryNews);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryNews>> Create([FromBody] CategoryNews categoryNews)
        {
            if (categoryNews == null)
                return BadRequest("CategoryNews data is required.");

            // Đảm bảo ID không được sử dụng khi tạo mới
            var newCategory = new CategoryNews
            {
                CategoryName = categoryNews.CategoryName,
                Description = categoryNews.Description
            };

            var createdCategory = await _categoryNewsService.AddAsync(newCategory);
            return CreatedAtAction(nameof(GetById), new { id = createdCategory.CategoryNewsID }, createdCategory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, CategoryNews categoryNews)
        {
            categoryNews.CategoryNewsID = id;
            var success = await _categoryNewsService.UpdateAsync(categoryNews);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryNewsService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
