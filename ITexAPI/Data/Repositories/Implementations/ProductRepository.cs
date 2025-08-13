using Microsoft.EntityFrameworkCore;
using ITexAPI.Data.Repositories.Interfaces;
using ITexAPI.Models.Entities;
using ITexAPI.Models.DTOs.Common;

namespace ITexAPI.Data.Repositories.Implementations
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetProductWithImagesAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Images.OrderBy(img => img.DisplayOrder))
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<PaginatedResponse<Product>> GetProductsWithFiltersAsync(PaginationParams paginationParams,
            int? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null,
            string? fabricType = null, string? color = null, string? size = null)
        {
            var query = _dbSet
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            // Apply filters
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (!string.IsNullOrEmpty(fabricType))
                query = query.Where(p => p.FabricType != null && p.FabricType.Contains(fabricType));

            if (!string.IsNullOrEmpty(color))
                query = query.Where(p => p.Color != null && p.Color.Contains(color));

            if (!string.IsNullOrEmpty(size))
                query = query.Where(p => p.Size != null && p.Size.Contains(size));

            if (!string.IsNullOrEmpty(paginationParams.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(paginationParams.SearchTerm) ||
                                        p.Description.Contains(paginationParams.SearchTerm) ||
                                        p.SKU.Contains(paginationParams.SearchTerm));
            }

            // Apply sorting
            query = paginationParams.SortBy?.ToLower() switch
            {
                "name" => paginationParams.SortOrder == "desc" ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => paginationParams.SortOrder == "desc" ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "createdat" => paginationParams.SortOrder == "desc" ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PaginatedResponse<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalPages = totalPages,
                HasPrevious = paginationParams.PageNumber > 1,
                HasNext = paginationParams.PageNumber < totalPages
            };
        }

        public async Task<bool> IsSkuUniqueAsync(string sku, int? excludeId = null)
        {
            var query = _dbSet.Where(p => p.SKU == sku);

            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= threshold && p.IsActive)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }

        public async Task<bool> IsInStockAsync(int productId, int quantity)
        {
            var product = await _dbSet.FirstOrDefaultAsync(p => p.Id == productId);
            return product != null && product.StockQuantity >= quantity;
        }

        public async Task UpdateStockAsync(int productId, int quantity)
        {
            var product = await _dbSet.FirstOrDefaultAsync(p => p.Id == productId);
            if (product != null)
            {
                product.StockQuantity -= quantity;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}