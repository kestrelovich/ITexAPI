using ITexAPI.Models.Entities;

namespace ITexAPI.Data.Repositories.Interfaces
{
    public interface IShoppingCartRepository : IBaseRepository<ShoppingCartItem>
    {
        Task<IEnumerable<ShoppingCartItem>> GetCartItemsByUserIdAsync(int userId);
        Task<ShoppingCartItem?> GetCartItemAsync(int userId, int productId);
        Task DeleteCartItemAsync(int userId, int productId);
        Task ClearCartAsync(int userId);
        Task<decimal> GetCartTotalAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
    }
}