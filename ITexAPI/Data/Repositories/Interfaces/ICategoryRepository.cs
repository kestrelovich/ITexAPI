using ITexAPI.Models.Entities;

namespace ITexAPI.Data.Repositories.Interfaces
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<IEnumerable<Category>> GetCategoriesWithChildrenAsync();
        Task<Category?> GetCategoryWithChildrenAsync(int id);
        Task<bool> HasChildrenAsync(int categoryId);
        Task<bool> IsValidParentAsync(int categoryId, int? parentId);
    }
}