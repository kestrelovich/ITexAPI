using ITexAPI.Data.Repositories.Interfaces;
using ITexAPI.Models.DTOs;
using ITexAPI.Models.Entities;
using ITexAPI.Services.Interfaces;
using AutoMapper;

namespace ITexAPI.Services.Implementations
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _cartRepo;
        private readonly IProductRepository _productRepo;
        private readonly IMapper _mapper;

        public ShoppingCartService(
            IShoppingCartRepository cartRepo,
            IProductRepository productRepo,
            IMapper mapper)
        {
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _mapper = mapper;
        }

        public async Task<ShoppingCartDto> GetCartByUserIdAsync(int userId)
        {
            var items = await _cartRepo.GetCartItemsByUserIdAsync(userId);
            var cart = new ShoppingCartDto
            {
                Items = _mapper.Map<List<ShoppingCartItemDto>>(items),
                TotalAmount = await _cartRepo.GetCartTotalAsync(userId),
                TotalItems = await _cartRepo.GetCartItemCountAsync(userId)
            };
            return cart;
        }

        public async Task AddToCartAsync(int userId, AddToCartDto dto)
        {
            // 1. Validate product existence and active status
            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null || !product.IsActive)
                throw new Exception("Product not found or inactive");

            // 2. Validate stock availability
            if (!await _productRepo.IsInStockAsync(dto.ProductId, dto.Quantity))
                throw new Exception("Not enough stock for this product.");

            // 3. Check if the item is already in the cart
            var existingItem = await _cartRepo.GetCartItemAsync(userId, dto.ProductId);

            if (existingItem != null)
            {
                int newQuantity = existingItem.Quantity + dto.Quantity;
                if (newQuantity > product.StockQuantity)
                    throw new Exception($"You cannot add more than {product.StockQuantity} of this product to your cart.");

                existingItem.Quantity = newQuantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                await _cartRepo.UpdateAsync(existingItem); // You may need to add UpdateAsync to your repository if not present
            }
            else
            {
                var cartItem = new ShoppingCartItem
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    CreatedAt = DateTime.UtcNow
                };
                await _cartRepo.AddAsync(cartItem);
            }
        }


        public async Task UpdateCartItemAsync(int userId, int productId, UpdateCartItemDto dto)
        {
            // 1. Validate product existence and active status
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null || !product.IsActive)
                throw new Exception("Product not found or inactive");

            // 2. Validate the desired quantity is not more than stock
            if (dto.Quantity > product.StockQuantity)
                throw new Exception($"Cannot set quantity above available stock ({product.StockQuantity}).");

            // 3. Get the cart item
            var existingItem = await _cartRepo.GetCartItemAsync(userId, productId);
            if (existingItem == null)
                throw new Exception("Cart item not found.");

            existingItem.Quantity = dto.Quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;

            await _cartRepo.UpdateAsync(existingItem); // As above, implement if needed
        }


        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            await _cartRepo.DeleteCartItemAsync(userId, productId);
        }

        public async Task ClearCartAsync(int userId)
        {
            await _cartRepo.ClearCartAsync(userId);
        }
    }
}
