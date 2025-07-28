using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;

namespace ITexAPI.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync();
        Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id);
        Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto);
        Task<ApiResponse<bool>> DeleteCategoryAsync(int id);
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategoriesByParentAsync(int? parentId);
    }
}