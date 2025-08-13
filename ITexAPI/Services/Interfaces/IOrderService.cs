using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;

namespace ITexAPI.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> GetByIdAsync(int id);
        Task<IEnumerable<OrderSummaryDto>> GetAllAsync();
        Task<PaginatedResponse<OrderSummaryDto>> GetPaginatedAsync(PaginationParams paginationParams);
        Task<OrderDto> CreateAsync(int? userId, CreateOrderDto dto);
        Task CancelAsync(int id);
        Task UpdateStatusAsync(int id, string status);
    }
}
