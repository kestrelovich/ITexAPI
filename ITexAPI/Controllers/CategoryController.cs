using Microsoft.AspNetCore.Mvc;
using ITexAPI.Models.DTOs;
using ITexAPI.Services.Interfaces;

namespace ITexAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> Get(int id)
            => Ok(await _categoryService.GetByIdAsync(id));

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
            => Ok(await _categoryService.GetAllAsync());

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
        {
            var category = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] UpdateCategoryDto dto)
            => Ok(await _categoryService.UpdateAsync(id, dto));

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _categoryService.DeleteAsync(id);
            return NoContent();
        }
    }
}
