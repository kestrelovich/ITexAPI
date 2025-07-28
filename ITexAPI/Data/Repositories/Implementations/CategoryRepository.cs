using Microsoft.EntityFrameworkCore;
using ITexAPI.Data.Repositories.Interfaces;
using ITexAPI.Models.Entities;

namespace ITexAPI.Data.Repositories.Implementations
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.ParentId == null && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithChildrenAsync()
        {
            return await _dbSet
                .Include(c => c.Children.Where(child => child.IsActive))
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryWithChildrenAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Children.Where(child => child.IsActive))
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> HasChildrenAsync(int categoryId)
        {
            return await _dbSet.AnyAsync(c => c.ParentId == categoryId && c.IsActive);
        }

        public async Task<bool> IsValidParentAsync(int categoryId, int? parentId)
        {
            if (parentId == null) return true;

            // Check if the parent exists and is active
            var parent = await _dbSet.FirstOrDefaultAsync(c => c.Id == parentId && c.IsActive);
            if (parent == null) return false;

            // Check if setting this parent would create a circular reference
            var currentCategory = await _dbSet.FirstOrDefaultAsync(c => c.Id == categoryId);
            if (currentCategory == null) return true;

            // Traverse up the parent chain to check for circular reference
            var checkParentId = parentId;
            while (checkParentId != null)
            {
                if (checkParentId == categoryId) return false;

                var checkParent = await _dbSet.FirstOrDefaultAsync(c => c.Id == checkParentId);
                checkParentId = checkParent?.ParentId;
            }

            return true;
        }

        public async Task<IEnumerable<Category>> GetAllWithChildrenAsync()
        {
            return await _dbSet
                .Include(c => c.Children.Where(child => child.IsActive))
                .Include(c => c.Parent)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdWithChildrenAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Children.Where(child => child.IsActive))
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category?> GetByIdWithParentAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetByParentIdAsync(int? parentId)
        {
            return await _dbSet
                .Where(c => c.ParentId == parentId && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == categoryId && p.IsActive);
        }
    }
}