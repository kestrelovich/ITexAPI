using AutoMapper;
using ITexAPI.Data.Repositories.Interfaces;
using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;
using ITexAPI.Models.Entities;
using ITexAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITexAPI.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllWithChildrenAsync();
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
                return ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categoryDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<CategoryDto>>.ErrorResponse($"Error retrieving categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdWithChildrenAsync(id);
                if (category == null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Category not found");
                }

                var categoryDto = _mapper.Map<CategoryDto>(category);
                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDto>.ErrorResponse($"Error retrieving category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            try
            {
                // Check if parent exists (if provided)
                if (createCategoryDto.ParentId.HasValue)
                {
                    var parentExists = await _categoryRepository.ExistsAsync(createCategoryDto.ParentId.Value);
                    if (!parentExists)
                    {
                        return ApiResponse<CategoryDto>.ErrorResponse("Parent category not found");
                    }
                }

                // Check if category name already exists at the same level
                var existingCategory = await _categoryRepository.FirstOrDefaultAsync(c =>
                    c.Name == createCategoryDto.Name && c.ParentId == createCategoryDto.ParentId);

                if (existingCategory != null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("A category with this name already exists at this level");
                }

                var category = _mapper.Map<Category>(createCategoryDto);
                var createdCategory = await _categoryRepository.AddAsync(category);

                // Reload with parent information
                var categoryWithParent = await _categoryRepository.GetByIdWithParentAsync(createdCategory.Id);
                var categoryDto = _mapper.Map<CategoryDto>(categoryWithParent);

                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDto>.ErrorResponse($"Error creating category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Category not found");
                }

                // Check if parent exists (if provided and different from current)
                if (updateCategoryDto.ParentId.HasValue && updateCategoryDto.ParentId != existingCategory.ParentId)
                {
                    var parentExists = await _categoryRepository.ExistsAsync(updateCategoryDto.ParentId.Value);
                    if (!parentExists)
                    {
                        return ApiResponse<CategoryDto>.ErrorResponse("Parent category not found");
                    }

                    // Prevent circular references
                    if (updateCategoryDto.ParentId == id)
                    {
                        return ApiResponse<CategoryDto>.ErrorResponse("A category cannot be its own parent");
                    }
                }

                // Check if name already exists at the same level (excluding current category)
                var duplicateCategory = await _categoryRepository.FirstOrDefaultAsync(c =>
                    c.Name == updateCategoryDto.Name &&
                    c.ParentId == updateCategoryDto.ParentId &&
                    c.Id != id);

                if (duplicateCategory != null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("A category with this name already exists at this level");
                }

                _mapper.Map(updateCategoryDto, existingCategory);
                var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);

                // Reload with parent information
                var categoryWithParent = await _categoryRepository.GetByIdWithParentAsync(updatedCategory.Id);
                var categoryDto = _mapper.Map<CategoryDto>(categoryWithParent);

                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDto>.ErrorResponse($"Error updating category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Category not found");
                }

                // Check if category has children
                var hasChildren = await _categoryRepository.ExistsAsync(c => c.ParentId == id);
                if (hasChildren)
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot delete category that has subcategories");
                }

                // Check if category has products
                var hasProducts = await _categoryRepository.HasProductsAsync(id);
                if (hasProducts)
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot delete category that has products");
                }

                await _categoryRepository.DeleteAsync(id);
                return ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error deleting category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategoriesByParentAsync(int? parentId)
        {
            try
            {
                var categories = await _categoryRepository.GetByParentIdAsync(parentId);
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
                return ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categoryDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<CategoryDto>>.ErrorResponse($"Error retrieving categories: {ex.Message}");
            }
        }
    }
}