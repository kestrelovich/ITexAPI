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
        private readonly IEmailService _emailService;
        private readonly IProductRepository _productRepo;

        public OrderService(IOrderRepository orderRepo, IMapper mapper, IEmailService emailService, IProductRepository productRepo)
        {
            _orderRepo = orderRepo;
            _mapper = mapper;
            _emailService = emailService;
            _productRepo = productRepo;
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
                Items = _mapper.Map<List<OrderSummaryDto>>(paged.Items),
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
            Console.WriteLine("=== ORDER SERVICE DEBUG ===");
            Console.WriteLine($"CreateAsync called at: {DateTime.Now}");
            Console.WriteLine($"UserId: {userId}");
            Console.WriteLine($"DTO Items count: {dto.Items?.Count ?? 0}");

            // Create order entity
            var order = _mapper.Map<Order>(dto);
            order.UserId = userId;

            Console.WriteLine($"Order entity created: {order.CustomerFirstName} {order.CustomerLastName}");
            Console.WriteLine($"Order address: {order.ShippingAddress}");
            Console.WriteLine($"Order notes: {order.Notes}");

            // Create order items with proper pricing
            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            Console.WriteLine("Processing order items...");
            foreach (var itemDto in dto.Items)
            {
                Console.WriteLine($"Processing item: ProductId={itemDto.ProductId}, Qty={itemDto.Quantity}, Price={itemDto.UnitPrice}");

                var product = await _productRepo.GetByIdAsync(itemDto.ProductId);
                if (product != null)
                {
                    Console.WriteLine($"Product found: {product.Name}, DB Price: {product.Price}");

                    // Validate that frontend price matches backend price (optional security check)
                    if (Math.Abs(product.Price - itemDto.UnitPrice) > 0.01m)
                    {
                        Console.WriteLine($"PRICE MISMATCH! Expected: {product.Price}, Received: {itemDto.UnitPrice}");
                        throw new InvalidOperationException($"Price mismatch for product {itemDto.ProductId}. Expected: {product.Price}, Received: {itemDto.UnitPrice}");
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice, // Use validated frontend price
                        TotalPrice = itemDto.UnitPrice * itemDto.Quantity
                    };

                    Console.WriteLine($"OrderItem created: Price={orderItem.UnitPrice}, Total={orderItem.TotalPrice}");
                    orderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;
                    Console.WriteLine($"Running total: {totalAmount}");
                }
                else
                {
                    Console.WriteLine($"ERROR: Product not found with ID: {itemDto.ProductId}");
                }
            }

            Console.WriteLine($"Final totals - Items: {orderItems.Count}, Amount: {totalAmount}");

            order.OrderItems = orderItems;
            order.TotalAmount = totalAmount;

            Console.WriteLine("Saving order to database...");
            // Create the order
            var created = await _orderRepo.AddAsync(order);
            Console.WriteLine($"Order saved with ID: {created.Id}");

            var createdDto = _mapper.Map<OrderDto>(created);
            Console.WriteLine($"Mapped result - ID: {createdDto.Id}, Total: {createdDto.TotalAmount}, Items: {createdDto.OrderItems?.Count ?? 0}");
            Console.WriteLine("=== END SERVICE DEBUG ===");

            // Send both admin and customer emails in background (don't wait for them to complete)
            _ = Task.Run(async () =>
            {
                // Send admin notification email
                try
                {
                    Console.WriteLine("Starting admin email notification...");
                    await _emailService.SendOrderNotificationAsync(createdDto);
                    Console.WriteLine("Admin email notification completed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Admin email notification failed: {ex.Message}");
                    // Log but continue - we still want to try sending customer email
                }

                // Send customer confirmation email
                try
                {
                    Console.WriteLine("Starting customer confirmation email...");
                    await _emailService.SendCustomerOrderConfirmationAsync(createdDto);
                    Console.WriteLine("Customer confirmation email completed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Customer confirmation email failed: {ex.Message}");
                    // Log but don't throw - order is already created
                }
            });

            return createdDto;
        }

        public async Task CancelAsync(int id)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order == null) throw new Exception("Order not found");
            order.Status = OrderStatus.Cancelled;
            await _orderRepo.UpdateAsync(order);
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order == null) throw new Exception("Order not found");

            if (Enum.TryParse<OrderStatus>(status, out var orderStatus))
            {
                order.Status = orderStatus;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepo.UpdateAsync(order);
            }
            else
            {
                throw new ArgumentException($"Invalid order status: {status}");
            }
        }
    }
}
