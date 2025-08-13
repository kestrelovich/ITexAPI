using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITexAPI.Data;
using ITexAPI.Models.Entities;
using ITexAPI.Models.DTOs;

namespace ITexAPI.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Where(p => p.IsActive)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {id} not found" });
                }

                // Return clean response without circular references
                var response = new
                {
                    id = product.Id,
                    name = product.Name,
                    description = product.Description,
                    sku = product.SKU,
                    price = product.Price,
                    stockQuantity = product.StockQuantity,
                    categoryId = product.CategoryId,
                    categoryName = product.Category?.Name,
                    isActive = product.IsActive,
                    fabricType = product.FabricType,
                    color = product.Color,
                    pattern = product.Pattern,
                    size = product.Size,
                    weight = product.Weight,
                    careInstructions = product.CareInstructions,
                    composition = product.Composition,
                    season = product.Season,
                    createdAt = product.CreatedAt,
                    updatedAt = product.UpdatedAt,
                    images = product.Images?.Select(img => new
                    {
                        id = img.Id,
                        imageUrl = img.ImageUrl,
                        altText = img.AltText,
                        isMain = img.IsMain,
                        displayOrder = img.DisplayOrder
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST: api/admin/products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = new Product
                {
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    SKU = createProductDto.SKU,
                    Price = createProductDto.Price,
                    StockQuantity = createProductDto.StockQuantity,
                    CategoryId = createProductDto.CategoryId,
                    IsActive = createProductDto.IsActive,
                    FabricType = createProductDto.FabricType,
                    Color = createProductDto.Color,
                    Pattern = createProductDto.Pattern,
                    Size = createProductDto.Size,
                    Weight = createProductDto.Weight,
                    CareInstructions = createProductDto.CareInstructions,
                    Composition = createProductDto.Composition,
                    Season = createProductDto.Season,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Return clean response without circular references
                var response = new
                {
                    id = product.Id,
                    name = product.Name,
                    description = product.Description,
                    sku = product.SKU,
                    price = product.Price,
                    stockQuantity = product.StockQuantity,
                    categoryId = product.CategoryId,
                    isActive = product.IsActive,
                    message = "Product created successfully"
                };

                _logger.LogInformation("Product created successfully with ID {ProductId}", product.Id);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT: api/admin/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            _logger.LogInformation("=== UpdateProduct called with ID: {ProductId} ===", id);
            _logger.LogInformation("Request body received: {@UpdateProductDto}", updateProductDto);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                if (id != updateProductDto.Id)
                {
                    _logger.LogWarning("Product ID mismatch. URL ID: {UrlId}, DTO ID: {DtoId}", id, updateProductDto.Id);
                    return BadRequest(new { message = "Product ID mismatch" });
                }

                _logger.LogInformation("Looking for product with ID: {ProductId}", id);
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found in database", id);
                    return NotFound(new { message = $"Product with ID {id} not found" });
                }

                _logger.LogInformation("Found existing product: {@ExistingProduct}", existingProduct);

                // Update product properties (only if provided)
                if (updateProductDto.Name != null)
                {
                    _logger.LogInformation("Updating Name: {OldName} -> {NewName}", existingProduct.Name, updateProductDto.Name);
                    existingProduct.Name = updateProductDto.Name;
                }
                if (updateProductDto.Description != null) existingProduct.Description = updateProductDto.Description;
                if (updateProductDto.SKU != null) existingProduct.SKU = updateProductDto.SKU;
                if (updateProductDto.Price.HasValue) existingProduct.Price = updateProductDto.Price.Value;
                if (updateProductDto.StockQuantity.HasValue) existingProduct.StockQuantity = updateProductDto.StockQuantity.Value;

                // Validate CategoryId before updating
                if (updateProductDto.CategoryId.HasValue)
                {
                    _logger.LogInformation("Validating CategoryId: {CategoryId}", updateProductDto.CategoryId.Value);
                    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateProductDto.CategoryId.Value);
                    if (!categoryExists)
                    {
                        _logger.LogWarning("Category ID {CategoryId} does not exist", updateProductDto.CategoryId.Value);
                        return BadRequest(new { message = $"Category ID {updateProductDto.CategoryId.Value} does not exist" });
                    }
                    existingProduct.CategoryId = updateProductDto.CategoryId.Value;
                }

                if (updateProductDto.IsActive.HasValue) existingProduct.IsActive = updateProductDto.IsActive.Value;

                // Update textile properties (only if provided)
                if (updateProductDto.FabricType != null) existingProduct.FabricType = updateProductDto.FabricType;
                if (updateProductDto.Color != null) existingProduct.Color = updateProductDto.Color;
                if (updateProductDto.Pattern != null) existingProduct.Pattern = updateProductDto.Pattern;
                if (updateProductDto.Size != null) existingProduct.Size = updateProductDto.Size;
                if (updateProductDto.Weight.HasValue) existingProduct.Weight = updateProductDto.Weight;
                if (updateProductDto.CareInstructions != null) existingProduct.CareInstructions = updateProductDto.CareInstructions;
                if (updateProductDto.Composition != null) existingProduct.Composition = updateProductDto.Composition;
                if (updateProductDto.Season != null) existingProduct.Season = updateProductDto.Season;

                existingProduct.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Saving changes to database...");
                await _context.SaveChangesAsync();
                _logger.LogInformation("Changes saved successfully");

                // Return a clean response without circular references
                var response = new
                {
                    id = existingProduct.Id,
                    name = existingProduct.Name,
                    description = existingProduct.Description,
                    sku = existingProduct.SKU,
                    price = existingProduct.Price,
                    stockQuantity = existingProduct.StockQuantity,
                    categoryId = existingProduct.CategoryId,
                    isActive = existingProduct.IsActive,
                    fabricType = existingProduct.FabricType,
                    color = existingProduct.Color,
                    pattern = existingProduct.Pattern,
                    size = existingProduct.Size,
                    weight = existingProduct.Weight,
                    careInstructions = existingProduct.CareInstructions,
                    composition = existingProduct.Composition,
                    season = existingProduct.Season,
                    createdAt = existingProduct.CreatedAt,
                    updatedAt = existingProduct.UpdatedAt,
                    message = "Product updated successfully"
                };

                _logger.LogInformation("Product updated successfully with ID {ProductId}", id);
                return Ok(response);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating product {ProductId}", id);
                return Conflict(new { message = "Product was modified by another user" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}", details = ex.ToString() });
            }
        }

        // Removed test endpoint to avoid routing conflicts

        // DELETE: api/admin/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {id} not found" });
                }

                // Soft delete - just mark as inactive
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product soft deleted with ID {ProductId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}