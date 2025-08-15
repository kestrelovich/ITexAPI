using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;
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
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Get(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return Ok(ApiResponse<CategoryDto>.SuccessResponse(category));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categories));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto dto)
        {
            var category = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = category.Id }, ApiResponse<CategoryDto>.SuccessResponse(category));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            var category = await _categoryService.UpdateAsync(id, dto);
            return Ok(ApiResponse<CategoryDto>.SuccessResponse(category));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            await _categoryService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("by-parent")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetByParent([FromQuery] int? parentId = null)
        {
            var categories = await _categoryService.GetByParentAsync(parentId);
            return Ok(ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categories));
        }
    }
}
