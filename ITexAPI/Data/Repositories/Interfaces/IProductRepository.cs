using ITexAPI.Models.Entities;
using ITexAPI.Models.DTOs.Common;

namespace ITexAPI.Data.Repositories.Interfaces
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<Product?> GetProductWithImagesAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<PaginatedResponse<Product>> GetProductsWithFiltersAsync(PaginationParams paginationParams,
            int? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null,
            string? fabricType = null, string? color = null, string? size = null);
        Task<bool> IsSkuUniqueAsync(string sku, int? excludeId = null);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
        Task<bool> IsInStockAsync(int productId, int quantity);
        Task UpdateStockAsync(int productId, int quantity);
    }
}