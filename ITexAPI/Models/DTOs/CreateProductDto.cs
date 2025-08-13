namespace ITexAPI.Models.DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public bool IsActive { get; set; } = true;

        // Textile-specific properties
        public string? FabricType { get; set; }
        public string? Color { get; set; }
        public string? Pattern { get; set; }
        public string? Size { get; set; }
        public decimal? Weight { get; set; }
        public string? CareInstructions { get; set; }
        public string? Composition { get; set; }
        public string? Season { get; set; }
    }
}
