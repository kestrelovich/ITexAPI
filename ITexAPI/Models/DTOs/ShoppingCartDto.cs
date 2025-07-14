namespace ITexAPI.Models.DTOs
{
    public class ShoppingCartDto
    {
        public List<ShoppingCartItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }
}
