namespace ITexAPI.Models.DTOs
{
    public class ProductSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? MainImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Textile-specific properties
        public string? FabricType { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
    }
}
