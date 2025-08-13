using System.Text.Json.Serialization;

namespace ITexAPI.Models.Entities
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsMain { get; set; } = false;

        // Keep this consistent with your AddImageUrlDto Range(1, ...)
        public int DisplayOrder { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Prevent Product -> Images -> Product -> ... cycles if entities get serialized
        [JsonIgnore]
        public virtual Product Product { get; set; } = null!;
    }
}
