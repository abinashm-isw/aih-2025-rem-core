using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REM.Core.Api.Data;

namespace REM.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly OracleDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(OracleDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabaseConnection()
    {
        try
        {
            _logger.LogInformation("Testing database connection...");
            
            // Simple connection test
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (canConnect)
            {
                _logger.LogInformation("Database connection successful");
                return Ok(new { status = "healthy", message = "Database connection successful" });
            }
            else
            {
                _logger.LogWarning("Database connection failed");
                return StatusCode(503, new { status = "unhealthy", message = "Cannot connect to database" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return StatusCode(503, new { 
                status = "unhealthy", 
                message = "Database connection failed", 
                error = ex.Message 
            });
        }
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
