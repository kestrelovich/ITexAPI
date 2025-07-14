namespace ITexAPI.Models.DTOs
{
    public class CreateOrderDto
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<CreateOrderItemDto> OrderItems { get; set; } = new();

    }
}
