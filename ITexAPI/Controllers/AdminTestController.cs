using ITexAPI.Data;
using ITexAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITexAPI.Controllers
{
    [ApiController]
    [Route("api/admin/test")]
    public class AdminTestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminTestController> _logger;

        public AdminTestController(ApplicationDbContext context, ILogger<AdminTestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            _logger.LogInformation("=== ADMIN TEST PING ===");
            return Ok(new { message = "Admin test controller working", timestamp = DateTime.UtcNow });
        }

        [HttpGet("database")]
        public async Task<IActionResult> TestDatabase()
        {
            _logger.LogInformation("=== TESTING DATABASE CONNECTION ===");
            try
            {
                var productCount = await _context.Products.CountAsync();
                _logger.LogInformation("Database connected successfully. Product count: {Count}", productCount);
                return Ok(new { message = "Database connected", productCount = productCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection failed: {Message}", ex.Message);
                return StatusCode(500, new { message = "Database error", error = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> TestUpdate(int id, [FromBody] SimpleUpdateDto dto)
        {
            _logger.LogInformation("=== TESTING UPDATE with ID {Id} ===", id);
            _logger.LogInformation("Received DTO: {@Dto}", dto);

            try
            {
                if (dto == null)
                {
                    return BadRequest("DTO is null");
                }

                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound($"Product {id} not found");
                }

                if (!string.IsNullOrEmpty(dto.Name))
                {
                    product.Name = dto.Name;
                }

                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Update successful", id = id, newName = product.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update failed: {Message}", ex.Message);
                return StatusCode(500, new { message = "Update failed", error = ex.Message });
            }
        }
    }

    public class SimpleUpdateDto
    {
        public string? Name { get; set; }
    }
}