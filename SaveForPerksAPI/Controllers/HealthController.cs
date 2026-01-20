using Microsoft.AspNetCore.Mvc;

namespace SaveForPerksAPI.Controllers;

[Route("api/[controller]")]
public class HealthController : BaseApiController
{
    private readonly IConfiguration _configuration;

    public HealthController(IConfiguration configuration, ILogger<HealthController> logger)
        : base(logger)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var keyVaultName = _configuration["Azure:KeyVaultName"];
        var connectionString = _configuration.GetConnectionString("SaveForPerksDBConnectionString");
        
        // Mask the connection string for security (show only if it exists and first/last few chars)
        var maskedConnectionString = connectionString != null 
            ? $"{connectionString[..Math.Min(20, connectionString.Length)]}...{(connectionString.Length > 40 ? connectionString[^10..] : "")}" 
            : "NOT CONFIGURED";
        var nonMaskedConnectionString = connectionString != null
            ? $"{connectionString}"
            : "NOT CONFIGURED";

        Logger.LogInformation("Health check accessed");

        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            keyVault = keyVaultName ?? "NOT CONFIGURED",
            connectionStringConfigured = connectionString != null,
            // connectionStringPreview = maskedConnectionString,
            connectionStringPreview = nonMaskedConnectionString,
            machineName = Environment.MachineName
        });
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { message = "pong", timestamp = DateTime.UtcNow });
    }
}