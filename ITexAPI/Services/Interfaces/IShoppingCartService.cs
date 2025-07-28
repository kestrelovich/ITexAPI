using ITexAPI.Models.DTOs;

namespace ITexAPI.Services.Interfaces
{
    public interface IShoppingCartService
    {
        Task<ShoppingCartDto> GetCartByUserIdAsync(int userId);
        Task AddToCartAsync(int userId, AddToCartDto dto);
        Task UpdateCartItemAsync(int userId, int productId, UpdateCartItemDto dto);
        Task RemoveFromCartAsync(int userId, int productId);
        Task ClearCartAsync(int userId);
    }
}
