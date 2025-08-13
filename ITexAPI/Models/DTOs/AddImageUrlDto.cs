using System.ComponentModel.DataAnnotations;

namespace ITexAPI.DTOs
{
    public class AddImageUrlDto
    {
        [Required]
        [Url]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        [StringLength(200)]
        public string? AltText { get; set; }

        public bool IsMain { get; set; } = false;

        [Range(1, int.MaxValue)]
        public int DisplayOrder { get; set; } = 1;
    }
}