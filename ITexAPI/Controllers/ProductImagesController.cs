using ITexAPI.Data;
using ITexAPI.DTOs;
using ITexAPI.Models.DTOs;
using ITexAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITexAPI.Controllers
{
    [ApiController]
    [Route("api/admin/products/{productId}/images")]
    public class ProductImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductImagesController> _logger;

        public ProductImagesController(ApplicationDbContext context, ILogger<ProductImagesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Removed test endpoint to avoid routing conflicts

        // POST: api/admin/products/{productId}/images/url
        [HttpPost("url")]
        public async Task<ActionResult<ProductImage>> AddProductImageUrl(int productId, [FromBody] AddImageUrlDto imageUrlDto)
        {
            _logger.LogInformation("=== AddProductImageUrl called for product ID: {ProductId} ===", productId);
            _logger.LogInformation("Image URL data received: {@ImageUrlDto}", imageUrlDto);

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {productId} not found" });
                }

                // If this is set as main image, unset other main images for this product
                if (imageUrlDto.IsMain)
                {
                    var existingMainImages = await _context.ProductImages
                        .Where(img => img.ProductId == productId && img.IsMain)
                        .ToListAsync();

                    foreach (var img in existingMainImages)
                    {
                        img.IsMain = false;
                    }
                }

                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = imageUrlDto.ImageUrl,
                    AltText = imageUrlDto.AltText,
                    IsMain = imageUrlDto.IsMain,
                    DisplayOrder = imageUrlDto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ProductImages.Add(productImage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Image URL added to product {ProductId}: {ImageUrl}", productId, imageUrlDto.ImageUrl);
                return Ok(productImage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding image URL to product {ProductId}", productId);
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        // GET: api/admin/products/{productId}/images
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductImage>>> GetProductImages(int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {productId} not found" });
                }

                var images = await _context.ProductImages
                    .Where(img => img.ProductId == productId)
                    .OrderBy(img => img.DisplayOrder)
                    .ToListAsync();

                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting images for product {ProductId}", productId);
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        // DELETE: api/admin/products/{productId}/images/{imageId}
        [HttpDelete("{imageId}")]
        public async Task<IActionResult> DeleteProductImage(int productId, int imageId)
        {
            try
            {
                var image = await _context.ProductImages
                    .FirstOrDefaultAsync(img => img.Id == imageId && img.ProductId == productId);

                if (image == null)
                {
                    return NotFound(new { message = $"Image with ID {imageId} not found for product {productId}" });
                }

                _context.ProductImages.Remove(image);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Image deleted for product {ProductId}, image {ImageId}", productId, imageId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image {ImageId} for product {ProductId}", imageId, productId);
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}