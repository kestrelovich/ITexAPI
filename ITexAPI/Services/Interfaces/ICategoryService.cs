using ITexAPI.Models.DTOs;

namespace ITexAPI.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CreateCategoryDto createCategoryDto);
        Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto updateCategoryDto);
        Task DeleteAsync(int id);
        Task<IEnumerable<CategoryDto>> GetByParentAsync(int? parentId);
    }
}