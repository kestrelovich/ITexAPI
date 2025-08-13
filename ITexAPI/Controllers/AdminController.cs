using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ITexAPI.Models.DTOs.Common;
using ITexAPI.Services.Interfaces;

namespace ITexAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IOrderService _orderService;

        public AdminController(IProductService productService, ICategoryService categoryService, IOrderService orderService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _orderService = orderService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<object>>> GetDashboard()
        {
            var products = await _productService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();

            var dashboard = new
            {
                TotalProducts = products.Count(),
                TotalCategories = categories.Count(),
                ActiveProducts = products.Count(p => p.IsActive),
                RecentProducts = products.OrderByDescending(p => p.CreatedAt).Take(5)
            };

            return Ok(ApiResponse<object>.SuccessResponse(dashboard));
        }

        [HttpGet("products/low-stock")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetLowStockProducts([FromQuery] int threshold = 10)
        {
            var products = await _productService.GetAllAsync();
            var lowStockProducts = products
                .Where(p => p.StockQuantity <= threshold && p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.SKU,
                    p.StockQuantity,
                    p.CategoryName
                });

            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(lowStockProducts));
        }

        [HttpGet("categories/hierarchy")]
        public async Task<ActionResult<ApiResponse<object>>> GetCategoryHierarchy()
        {
            var categories = await _categoryService.GetAllAsync();
            var rootCategories = categories.Where(c => c.ParentId == null);

            var hierarchy = rootCategories.Select(root => new
            {
                root.Id,
                root.Name,
                root.Description,
                Children = BuildCategoryTree(categories, root.Id)
            });

            return Ok(ApiResponse<object>.SuccessResponse(hierarchy));
        }

        [HttpGet("orders")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Models.DTOs.OrderSummaryDto>>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<Models.DTOs.OrderSummaryDto>>.SuccessResponse(orders));
        }

        [HttpGet("orders/paginated")]
        public async Task<ActionResult<ApiResponse<Models.DTOs.Common.PaginatedResponse<Models.DTOs.OrderSummaryDto>>>> GetOrdersPaginated([FromQuery] Models.DTOs.Common.PaginationParams paginationParams)
        {
            var orders = await _orderService.GetPaginatedAsync(paginationParams);
            return Ok(ApiResponse<Models.DTOs.Common.PaginatedResponse<Models.DTOs.OrderSummaryDto>>.SuccessResponse(orders));
        }

        [HttpGet("orders/{id}")]
        public async Task<ActionResult<ApiResponse<Models.DTOs.OrderDto>>> GetOrder(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            return Ok(ApiResponse<Models.DTOs.OrderDto>.SuccessResponse(order));
        }

        [HttpPut("orders/{id}/status")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound(ApiResponse<string>.ErrorResponse("Order not found"));

            // Update order status (you'll need to implement this in OrderService)
            await _orderService.UpdateStatusAsync(id, request.Status);

            return Ok(ApiResponse<string>.SuccessResponse("Order status updated successfully"));
        }

        private object BuildCategoryTree(IEnumerable<Models.DTOs.CategoryDto> allCategories, int parentId)
        {
            return allCategories
                .Where(c => c.ParentId == parentId)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    Children = BuildCategoryTree(allCategories, c.Id)
                });
        }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}