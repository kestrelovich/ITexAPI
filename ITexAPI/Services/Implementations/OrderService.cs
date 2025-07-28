using ITexAPI.Data.Repositories.Interfaces;
using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;
using ITexAPI.Models.Entities;
using ITexAPI.Services.Interfaces;
using AutoMapper;

namespace ITexAPI.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepo, IMapper mapper)
        {
            _orderRepo = orderRepo;
            _mapper = mapper;
        }

        public async Task<OrderDto> GetByIdAsync(int id)
        {
            var order = await _orderRepo.GetOrderWithItemsAsync(id);
            if (order == null) throw new Exception("Order not found");
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetAllAsync()
        {
            var orders = await _orderRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
        }

        public async Task<PaginatedResponse<OrderSummaryDto>> GetPaginatedAsync(PaginationParams paginationParams)
        {
            var paged = await _orderRepo.GetOrdersWithFiltersAsync(paginationParams);
            return new PaginatedResponse<OrderSummaryDto>
            {
                Data = _mapper.Map<List<OrderSummaryDto>>(paged.Data),
                TotalCount = paged.TotalCount,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                HasPrevious = paged.HasPrevious,
                HasNext = paged.HasNext
            };
        }

        public async Task<OrderDto> CreateAsync(int? userId, CreateOrderDto dto)
        {
            // Map CreateOrderDto to Order, handle items manually if needed
            var order = _mapper.Map<Order>(dto);
            order.UserId = userId;
            // Set OrderItems from dto if not handled by automapper
            // Calculate totals here if needed
            var created = await _orderRepo.AddAsync(order);
            return _mapper.Map<OrderDto>(created);
        }

        public async Task CancelAsync(int id)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order == null) throw new Exception("Order not found");
            order.Status = OrderStatus.Cancelled;
            await _orderRepo.UpdateAsync(order);
        }
    }
}
