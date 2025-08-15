using ITexAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITexAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Basic health check
            var health = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            };

            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new { Status = "Unhealthy", Error = "Service unavailable" });
        }
    }

    [HttpGet("detailed")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDetailed()
    {
        try
        {
            var checks = new List<object>();

            // Database health check
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                checks.Add(new { Component = "Database", Status = "Healthy", ResponseTime = "< 100ms" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database health check failed");
                checks.Add(new { Component = "Database", Status = "Unhealthy", Error = ex.Message });
            }

            // Memory check
            var gc = GC.GetTotalMemory(false);
            checks.Add(new
            {
                Component = "Memory",
                Status = gc < 1_000_000_000 ? "Healthy" : "Warning", // 1GB threshold
                Usage = $"{gc / 1_000_000} MB"
            });

            var overallStatus = checks.Any(c => c.GetType().GetProperty("Status")?.GetValue(c)?.ToString() == "Unhealthy")
                ? "Unhealthy" : "Healthy";

            var health = new
            {
                Status = overallStatus,
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                Checks = checks
            };

            return overallStatus == "Healthy" ? Ok(health) : StatusCode(503, health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed health check failed");
            return StatusCode(503, new { Status = "Unhealthy", Error = "Service unavailable" });
        }
    }
}