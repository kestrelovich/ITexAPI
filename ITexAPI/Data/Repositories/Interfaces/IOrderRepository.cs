using ITexAPI.Models.Entities;
using ITexAPI.Models.DTOs.Common;

namespace ITexAPI.Data.Repositories.Interfaces
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<Order?> GetOrderWithItemsAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
        Task<PaginatedResponse<Order>> GetOrdersWithFiltersAsync(PaginationParams paginationParams,
            int? userId = null, OrderStatus? status = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetOrderCountAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}