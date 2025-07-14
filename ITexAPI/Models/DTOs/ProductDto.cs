namespace ITexAPI.Models.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Textile-specific properties
        public string? FabricType { get; set; }
        public string? Color { get; set; }
        public string? Pattern { get; set; }
        public string? Size { get; set; }
        public decimal? Weight { get; set; }
        public string? CareInstructions { get; set; }
        public string? Composition { get; set; }
        public string? Season { get; set; }

        public List<ProductImageDto> Images { get; set; } = new();
    }
}
