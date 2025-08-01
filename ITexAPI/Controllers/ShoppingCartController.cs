using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;
using ITexAPI.Services.Interfaces;
using ITexAPI.Exceptions;
using System.Security.Claims;

namespace ITexAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartService _cartService;
        public ShoppingCartController(IShoppingCartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<ShoppingCartDto>>> Get()
        {
            var userId = GetCurrentUserId();
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return Ok(ApiResponse<ShoppingCartDto>.SuccessResponse(cart));
        }

        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<object>>> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = GetCurrentUserId();
            await _cartService.AddToCartAsync(userId, dto);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Item added to cart successfully"));
        }

        [HttpPut("{productId}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateCartItem(int productId, [FromBody] UpdateCartItemDto dto)
        {
            var userId = GetCurrentUserId();
            await _cartService.UpdateCartItemAsync(userId, productId, dto);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Cart updated successfully"));
        }

        [HttpDelete("{productId}")]
        public async Task<ActionResult<ApiResponse<object>>> RemoveFromCart(int productId)
        {
            var userId = GetCurrentUserId();
            await _cartService.RemoveFromCartAsync(userId, productId);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Item removed from cart"));
        }

        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponse<object>>> ClearCart()
        {
            var userId = GetCurrentUserId();
            await _cartService.ClearCartAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Cart cleared"));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new ValidationException("User ID not found in token");
            return userId;
        }
    }
}
