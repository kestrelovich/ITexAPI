using AutoMapper;
using ITexAPI.Data.Repositories.Interfaces;
using ITexAPI.Exceptions;
using ITexAPI.Models.DTOs;
using ITexAPI.Models.Entities;
using ITexAPI.Services.Interfaces;

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

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException("Category", id);

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto createCategoryDto)
        {
            // Check if parent exists (if provided)
            if (createCategoryDto.ParentId.HasValue)
            {
                var parent = await _categoryRepository.GetByIdAsync(createCategoryDto.ParentId.Value);
                if (parent == null)
                    throw new NotFoundException("Parent category", createCategoryDto.ParentId.Value);
            }

            // Check for duplicate name at the same level
            var categories = await _categoryRepository.GetAllAsync();
            var existingCategory = categories.FirstOrDefault(c =>
                c.Name.Equals(createCategoryDto.Name, StringComparison.OrdinalIgnoreCase) &&
                c.ParentId == createCategoryDto.ParentId);

            if (existingCategory != null)
                throw new DuplicateException($"A category with name '{createCategoryDto.Name}' already exists at this level.");

            var category = _mapper.Map<Category>(createCategoryDto);
            var created = await _categoryRepository.AddAsync(category);
            return _mapper.Map<CategoryDto>(created);
        }

        public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
                throw new NotFoundException("Category", id);

            // Check if parent exists (if provided and different from current)
            if (updateCategoryDto.ParentId.HasValue && updateCategoryDto.ParentId != existingCategory.ParentId)
            {
                // Prevent circular references
                if (updateCategoryDto.ParentId == id)
                    throw new ValidationException("A category cannot be its own parent.");

                var parent = await _categoryRepository.GetByIdAsync(updateCategoryDto.ParentId.Value);
                if (parent == null)
                    throw new NotFoundException("Parent category", updateCategoryDto.ParentId.Value);
            }

            // Check for duplicate name at the same level (excluding current category)
            var categories = await _categoryRepository.GetAllAsync();
            var duplicateCategory = categories.FirstOrDefault(c =>
                c.Name.Equals(updateCategoryDto.Name, StringComparison.OrdinalIgnoreCase) &&
                c.ParentId == updateCategoryDto.ParentId &&
                c.Id != id);

            if (duplicateCategory != null)
                throw new DuplicateException($"A category with name '{updateCategoryDto.Name}' already exists at this level.");

            _mapper.Map(updateCategoryDto, existingCategory);
            existingCategory.UpdatedAt = DateTime.UtcNow;
            var updated = await _categoryRepository.UpdateAsync(existingCategory);
            return _mapper.Map<CategoryDto>(updated);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException("Category", id);

            // Check if category has children
            var categories = await _categoryRepository.GetAllAsync();
            var hasChildren = categories.Any(c => c.ParentId == id);
            if (hasChildren)
                throw new ValidationException("Cannot delete category that has subcategories. Delete subcategories first.");

            await _categoryRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<CategoryDto>> GetByParentAsync(int? parentId)
        {
            var categories = await _categoryRepository.GetAllAsync();
            var filteredCategories = categories.Where(c => c.ParentId == parentId);
            return _mapper.Map<IEnumerable<CategoryDto>>(filteredCategories);
        }
    }
}