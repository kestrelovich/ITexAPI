namespace ITexAPI.Models.DTOs
{
    public class CreateOrderDto
    {
        // Customer Information
        public string CustomerFirstName { get; set; } = string.Empty;
        public string CustomerLastName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        // Shipping Information
        public string ShippingAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }

        // Payment Method
        public string PaymentMethod { get; set; } = "COD";

        // Order Items - Changed from OrderItems to Items to match frontend
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
