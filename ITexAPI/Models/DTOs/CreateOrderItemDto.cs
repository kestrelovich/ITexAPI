namespace ITexAPI.Models.DTOs
{
    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Added to match frontend data and enable price validation
    }
}
