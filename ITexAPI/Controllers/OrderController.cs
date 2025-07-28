using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;
using ITexAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITexAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> Get(int id)
            => Ok(await _orderService.GetByIdAsync(id));

        [HttpPost]
        public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto)
        {
            int? userId = null;
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userIdStr))
                userId = int.Parse(userIdStr);

            var order = await _orderService.CreateAsync(userId, dto);
            return Ok(order);
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> Cancel(int id)
        {
            await _orderService.CancelAsync(id);
            return NoContent();
        }
    }
}
