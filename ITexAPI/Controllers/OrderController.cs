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
        [Authorize]
        public async Task<ActionResult<ApiResponse<OrderDto>>> Get(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            return Ok(ApiResponse<OrderDto>.SuccessResponse(order));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderDto dto)
        {
            Console.WriteLine("=== ORDER CONTROLLER DEBUG ===");
            Console.WriteLine($"Received CreateOrder request at: {DateTime.Now}");
            Console.WriteLine($"Request body: {System.Text.Json.JsonSerializer.Serialize(dto, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");
            Console.WriteLine($"Customer: {dto.CustomerFirstName} {dto.CustomerLastName}");
            Console.WriteLine($"Phone: {dto.CustomerPhone}");
            Console.WriteLine($"Email: {dto.CustomerEmail}");
            Console.WriteLine($"PaymentMethod: {dto.PaymentMethod}");
            Console.WriteLine($"Items count: {dto.Items?.Count ?? 0}");

            if (dto.Items != null)
            {
                for (int i = 0; i < dto.Items.Count; i++)
                {
                    var item = dto.Items[i];
                    Console.WriteLine($"Item {i + 1}: ProductId={item.ProductId}, Quantity={item.Quantity}, UnitPrice={item.UnitPrice}");
                }
            }
            Console.WriteLine("=== END CONTROLLER DEBUG ===");

            try
            {
                int? userId = null;
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                ?? User.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(userIdStr))
                    userId = int.Parse(userIdStr);

                var order = await _orderService.CreateAsync(userId, dto);

                Console.WriteLine("=== ORDER CONTROLLER RESPONSE ===");
                Console.WriteLine($"Order created successfully with ID: {order.Id}");
                Console.WriteLine($"Total Amount: {order.TotalAmount}");
                Console.WriteLine($"Order Items Count: {order.OrderItems?.Count ?? 0}");
                Console.WriteLine("=== END RESPONSE DEBUG ===");

                return Ok(ApiResponse<OrderDto>.SuccessResponse(order));
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== ORDER CONTROLLER ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.WriteLine("=== END ERROR DEBUG ===");
                throw;
            }
        }

        [HttpPost("{id}/cancel")]
        [Authorize]
        public async Task<ActionResult> Cancel(int id)
        {
            await _orderService.CancelAsync(id);
            return NoContent();
        }
    }
}
