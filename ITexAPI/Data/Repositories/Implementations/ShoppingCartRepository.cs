using Microsoft.EntityFrameworkCore;
using ITexAPI.Data.Repositories.Interfaces;
using ITexAPI.Models.Entities;

namespace ITexAPI.Data.Repositories.Implementations
{
    public class ShoppingCartRepository : BaseRepository<ShoppingCartItem>, IShoppingCartRepository
    {
        public ShoppingCartRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ShoppingCartItem>> GetCartItemsByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Images.Where(img => img.IsMain))
                .Where(ci => ci.UserId == userId)
                .OrderBy(ci => ci.CreatedAt)
                .ToListAsync();
        }

        public async Task<ShoppingCartItem?> GetCartItemAsync(int userId, int productId)
        {
            return await _dbSet
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
        }

        public async Task DeleteCartItemAsync(int userId, int productId)
        {
            var item = await _dbSet.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
            if (item != null)
            {
                _dbSet.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(int userId)
        {
            var items = await _dbSet.Where(ci => ci.UserId == userId).ToListAsync();
            if (items.Any())
            {
                _dbSet.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetCartTotalAsync(int userId)
        {
            return await _dbSet
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .SumAsync(ci => ci.Quantity * ci.Product.Price);
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            return await _dbSet
                .Where(ci => ci.UserId == userId)
                .SumAsync(ci => ci.Quantity);
        }
    }
}