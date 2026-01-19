using Microsoft.AspNetCore.Mvc;
using SaveForPerksAPI.Common;

namespace SaveForPerksAPI.Controllers;

/// <summary>
/// Base controller providing common exception handling and result processing for all API controllers.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected readonly ILogger Logger;

    protected BaseApiController(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes an operation that returns a Result, handling success and failure cases.
    /// Returns 200 OK on success, 400 BadRequest on business logic failure, 500 on unexpected errors.
    /// </summary>
    protected async Task<ActionResult<T>> ExecuteAsync<T>(
        Func<Task<Result<T>>> operation,
        string operationName)
    {
        try
        {
            var result = await operation();
            
            if (result.IsFailure)
            {
                Logger.LogWarning("{Operation} failed: {Error}", operationName, result.Error);
                
                // Determine appropriate status code based on error message
                if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return NotFound(result.Error);
                }
                
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, 
                "Unexpected error in {Operation}. TraceId: {TraceId}", 
                operationName, 
                HttpContext.TraceIdentifier);
            
            return StatusCode(500, new 
            { 
                error = "An unexpected error occurred. Please try again later.",
                traceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Executes an operation that creates a resource, returning 201 Created with Location header.
    /// Returns 400 BadRequest on business logic failure, 500 on unexpected errors.
    /// </summary>
    protected async Task<ActionResult<T>> ExecuteCreatedAsync<T>(
        Func<Task<Result<T>>> operation,
        string routeName,
        Func<T, object> routeValues,
        string operationName)
    {
        try
        {
            var result = await operation();
            
            if (result.IsFailure)
            {
                Logger.LogWarning("{Operation} failed: {Error}", operationName, result.Error);
                return BadRequest(result.Error);
            }

            return CreatedAtRoute(routeName, routeValues(result.Value!), result.Value);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, 
                "Unexpected error in {Operation}. TraceId: {TraceId}", 
                operationName, 
                HttpContext.TraceIdentifier);
            
            return StatusCode(500, new 
            { 
                error = "An unexpected error occurred. Please try again later.",
                traceId = HttpContext.TraceIdentifier
            });
        }
    }
}
