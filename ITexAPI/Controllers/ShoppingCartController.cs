using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ITexAPI.Models.DTOs;
using ITexAPI.Services.Interfaces;

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
        public async Task<ActionResult<ShoppingCartDto>> Get()
        {
            var userId = int.Parse(User.FindFirst("sub")?.Value ?? throw new Exception("No user id claim"));
            return Ok(await _cartService.GetCartByUserIdAsync(userId));
        }

        [HttpPost("add")]
        public async Task<ActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = int.Parse(User.FindFirst("sub")?.Value ?? throw new Exception("No user id claim"));
            await _cartService.AddToCartAsync(userId, dto);
            return NoContent();
        }

        [HttpPut("{productId}")]
        public async Task<ActionResult> UpdateCartItem(int productId, [FromBody] UpdateCartItemDto dto)
        {
            var userId = int.Parse(User.FindFirst("sub")?.Value ?? throw new Exception("No user id claim"));
            await _cartService.UpdateCartItemAsync(userId, productId, dto);
            return NoContent();
        }

        [HttpDelete("{productId}")]
        public async Task<ActionResult> RemoveFromCart(int productId)
        {
            var userId = int.Parse(User.FindFirst("sub")?.Value ?? throw new Exception("No user id claim"));
            await _cartService.RemoveFromCartAsync(userId, productId);
            return NoContent();
        }

        [HttpDelete("clear")]
        public async Task<ActionResult> ClearCart()
        {
            var userId = int.Parse(User.FindFirst("sub")?.Value ?? throw new Exception("No user id claim"));
            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }
    }
}
