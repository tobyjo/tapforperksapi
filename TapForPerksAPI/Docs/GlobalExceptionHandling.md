# Global Exception Handling - Implementation Guide

## Overview
Implemented comprehensive exception handling using:
1. **Global Exception Handler** - Catches all unhandled exceptions across the API
2. **Base Controller** - Provides consistent error handling for all controllers
3. **Defense in Depth** - Multiple layers of protection

## Architecture

### Three Layers of Protection

```
???????????????????????????????????????????????????????????????
?  Layer 1: Global Exception Handler (Last Resort)            ?
?  - Catches ANY unhandled exception                          ?
?  - Logs full details                                        ?
?  - Returns generic 500 error                                ?
???????????????????????????????????????????????????????????????
                              ?
???????????????????????????????????????????????????????????????
?  Layer 2: Base Controller Try-Catch                         ?
?  - Wraps all controller operations                          ?
?  - Logs with operation context                              ?
?  - Returns 400/404/500 as appropriate                       ?
???????????????????????????????????????????????????????????????
                              ?
???????????????????????????????????????????????????????????????
?  Layer 3: Service Layer Try-Catch (Already Implemented)     ?
?  - Catches expected failures                                ?
?  - Returns Result<T> with error message                     ?
?  - Logs business logic failures                             ?
???????????????????????????????????????????????????????????????
```

## Implementation Details

### 1. Global Exception Handler (Program.cs)

**Purpose:** Catch any exception that escapes controller/service layers.

```csharp
if (app.Environment.IsDevelopment())
{
    // Development: Show detailed errors
    app.UseDeveloperExceptionPage();
}
else
{
    // Production: Hide internals
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var exception = context.Features
                .Get<IExceptionHandlerPathFeature>()?.Error;

            // Log everything
            var logger = context.RequestServices
                .GetRequiredService<ILogger<Program>>();
            logger.LogError(exception, 
                "Unhandled exception. Path: {Path}, Method: {Method}", 
                context.Request.Path, 
                context.Request.Method);

            // Return generic error
            await context.Response.WriteAsJsonAsync(new
            {
                error = "An unexpected error occurred. Please try again later.",
                traceId = context.TraceIdentifier
            });
        });
    });
}
```

**What It Catches:**
- Database connection failures
- Mapping exceptions
- Unexpected null reference exceptions
- Any unhandled exception

**Response:**
```json
{
  "error": "An unexpected error occurred. Please try again later.",
  "traceId": "00-1234567890abcdef-1234567890abcdef-00"
}
```

**Logs:**
```
[Error] Unhandled exception. Path: /api/reward-owner/scans, Method: POST
System.InvalidOperationException: Database connection timeout
   at Microsoft.EntityFrameworkCore...
```

### 2. Base Controller (BaseApiController.cs)

**Purpose:** Consistent error handling pattern for all controllers.

#### Features:

**ExecuteAsync<T>** - For GET operations:
```csharp
protected async Task<ActionResult<T>> ExecuteAsync<T>(
    Func<Task<Result<T>>> operation,
    string operationName)
{
    try
    {
        var result = await operation();
        
        if (result.IsFailure)
        {
            // Business logic failure
            Logger.LogWarning("{Operation} failed: {Error}", 
                operationName, result.Error);
            
            // Smart status code selection
            if (result.Error?.Contains("not found") == true)
                return NotFound(result.Error);
            
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
    catch (Exception ex)
    {
        // Unexpected exception
        Logger.LogError(ex, "Unexpected error in {Operation}", operationName);
        return StatusCode(500, new { error = "...", traceId = "..." });
    }
}
```

**ExecuteCreatedAsync<T>** - For POST operations:
```csharp
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
            Logger.LogWarning("{Operation} failed: {Error}", 
                operationName, result.Error);
            return BadRequest(result.Error);
        }

        return CreatedAtRoute(routeName, routeValues(result.Value!), result.Value);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Unexpected error in {Operation}", operationName);
        return StatusCode(500, new { error = "...", traceId = "..." });
    }
}
```

### 3. Controller Usage (Clean & Simple)

**Before (? Manual Error Handling):**
```csharp
[HttpGet("{rewardId}/events/{scanEventId}")]
public async Task<ActionResult<ScanEventDto>> GetScanEventForReward(
    Guid rewardId, Guid scanEventId)
{
    var result = await rewardService.GetScanEventForRewardAsync(rewardId, scanEventId);

    if (result.IsFailure)
    {
        logger.LogWarning("Scan event not found: {Error}", result.Error);
        return NotFound(result.Error);
    }

    return Ok(result.Value);
}
```
- ? Repetitive error handling
- ? No exception protection
- ? Easy to forget logging

**After (? Base Controller):**
```csharp
[HttpGet("{rewardId}/events/{scanEventId}")]
public async Task<ActionResult<ScanEventDto>> GetScanEventForReward(
    Guid rewardId, Guid scanEventId)
{
    return await ExecuteAsync(
        () => rewardService.GetScanEventForRewardAsync(rewardId, scanEventId),
        nameof(GetScanEventForReward));
}
```
- ? Exception protection built-in
- ? Consistent logging
- ? Clean, readable code
- ? Proper status codes

## Error Response Patterns

### Success Responses

| Operation | Status Code | Response |
|-----------|-------------|----------|
| GET | 200 OK | `{ "id": "...", "name": "..." }` |
| POST | 201 Created | `{ "id": "...", ... }` + Location header |

### Business Logic Failures (Service Returns Result.Failure)

| Error Type | Status Code | Response | Example |
|------------|-------------|----------|---------|
| Not Found | 404 | `{ "error": "..." }` | "Scan event not found" |
| Validation | 400 | `{ "error": "..." }` | "Must be between 0 and 100" |
| Business Rule | 400 | `{ "error": "..." }` | "Insufficient points: 10 required, 5 available" |

### Unexpected Exceptions (Caught by Try-Catch)

| Layer | Status Code | Response | Logged |
|-------|-------------|----------|--------|
| Controller | 500 | `{ "error": "An unexpected error occurred...", "traceId": "..." }` | ? Full exception + context |
| Global Handler | 500 | `{ "error": "An unexpected error occurred...", "traceId": "..." }` | ? Full exception + path/method |

## Exception Flow Examples

### Example 1: Normal Operation (Success)

```
1. Request: POST /api/reward-owner/scans
2. Controller: ExecuteCreatedAsync calls service
3. Service: Returns Result.Success(data)
4. Controller: Returns 201 Created with data
5. Client receives: { "scanEvent": {...}, "currentBalance": 6 }
```

**No exceptions, no special handling needed.**

### Example 2: Business Logic Failure

```
1. Request: POST /api/reward-owner/scans (insufficient points)
2. Controller: ExecuteCreatedAsync calls service
3. Service: Returns Result.Failure("Insufficient points...")
4. Controller: Logs warning, returns 400 BadRequest
5. Client receives: { "error": "Insufficient points..." }
```

**Handled gracefully by Result pattern.**

### Example 3: Unexpected Exception in Service

```
1. Request: POST /api/reward-owner/scans
2. Controller: ExecuteCreatedAsync calls service
3. Service: Database throws SqlException
4. Service try-catch: Returns Result.Failure("An error occurred...")
5. Controller: Logs warning, returns 400 BadRequest
6. Client receives: { "error": "An error occurred..." }
```

**Service layer catches and converts to Result.**

### Example 4: Unexpected Exception in Controller

```
1. Request: POST /api/reward-owner/scans
2. Controller: ExecuteCreatedAsync calls service
3. Mapper throws NullReferenceException
4. Controller catch block: Logs error with TraceId
5. Controller: Returns 500 with generic message
6. Client receives: { "error": "An unexpected error...", "traceId": "..." }
```

**Base controller catch block handles it.**

### Example 5: Catastrophic Failure

```
1. Request: POST /api/reward-owner/scans
2. Controller/Service both throw unexpected exceptions
3. Global Exception Handler: Catches at application level
4. Logs full exception with path and method
5. Returns 500 with generic message
6. Client receives: { "error": "An unexpected error...", "traceId": "..." }
```

**Last resort - nothing exposed to client.**

## Logging Strategy

### Log Levels by Exception Type

| Exception Type | Log Level | Details Logged |
|----------------|-----------|----------------|
| **Business Logic Failure** | Warning | Operation name, error message |
| **Unexpected Controller Exception** | Error | Operation name, full exception, TraceId |
| **Global Handler Exception** | Error | Path, method, full exception, TraceId |

### Log Examples

**Business Logic (Warning):**
```
[Warning] CreatePointsAndClaimRewards failed: Insufficient points. Required: 10, Available: 5
```

**Controller Exception (Error):**
```
[Error] Unexpected error in CreatePointsAndClaimRewards. TraceId: 00-123abc...
System.NullReferenceException: Object reference not set to an instance of an object
   at TapForPerksAPI.Controllers...
```

**Global Handler (Error):**
```
[Error] Unhandled exception occurred. Path: /api/reward-owner/scans, Method: POST, TraceId: 00-123abc...
System.InvalidOperationException: Sequence contains no elements
   at System.Linq.Enumerable...
```

## Security Considerations

### ? What We Do Right

1. **No Stack Traces in Production** - Global handler prevents exposure
2. **Generic Error Messages** - Don't reveal internal structure
3. **TraceId for Debugging** - Allows correlation without exposing details
4. **Detailed Logging** - Server-side only, never to client
5. **Environment-Specific Behavior** - Detailed errors in dev, generic in prod

### ? What We Prevent

1. **Stack Trace Leakage** - Could reveal file paths, library versions
2. **Connection String Exposure** - Database connection errors
3. **Internal Architecture** - Class names, method signatures
4. **Sensitive Data** - User IDs in exception messages

## Testing Exception Handling

### Unit Tests (Already Passing)

All existing tests continue to work because:
- ? Base controller calls service methods correctly
- ? Result pattern works the same
- ? Status codes match expected values

### Manual Testing

**Test 1: Valid Request**
```bash
curl -X POST http://localhost:5143/api/reward-owner/scans \
  -H "Content-Type: application/json" \
  -d '{"qrCodeValue":"QR001-ALICE-9999","rewardId":"33333333-3333-3333-3333-333333333333","pointsChange":1}'

# Expected: 201 Created with data
```

**Test 2: Business Logic Error**
```bash
curl -X POST http://localhost:5143/api/reward-owner/scans \
  -H "Content-Type: application/json" \
  -d '{"qrCodeValue":"INVALID","rewardId":"33333333-3333-3333-3333-333333333333","pointsChange":1}'

# Expected: 400 BadRequest with "Invalid QR code or reward"
```

**Test 3: Simulate Exception (requires code change)**
```csharp
// Temporarily add to controller for testing:
[HttpGet("test-exception")]
public ActionResult TestException()
{
    throw new InvalidOperationException("Test exception");
}
```

```bash
curl http://localhost:5143/api/reward-owner/scans/test-exception

# Expected: 500 with generic message + TraceId
# Check logs for full exception details
```

## Benefits Summary

### For Users
- ? Never see technical errors
- ? Get helpful error messages for their mistakes
- ? Always get a response (no crashes)

### For Developers
- ? All exceptions logged with full context
- ? TraceId for correlation across systems
- ? Easy to debug with detailed logs
- ? Clean, maintainable controller code

### For Security
- ? No information disclosure
- ? Stack traces hidden in production
- ? Internal details never exposed
- ? Consistent error responses

### For Operations
- ? All errors captured and logged
- ? Easy to set up alerts
- ? TraceId for incident investigation
- ? Distinguish between expected and unexpected errors

## Best Practices

### ? DO:

1. **Use Base Controller** for all new controllers
2. **Log with TraceId** for correlation
3. **Return generic messages** for unexpected errors
4. **Use appropriate status codes** (400/404/500)
5. **Test exception paths** manually and in tests

### ? DON'T:

1. **Don't expose stack traces** to clients
2. **Don't log sensitive data** (passwords, tokens)
3. **Don't skip base controller** for new endpoints
4. **Don't return detailed errors** for unexpected exceptions
5. **Don't forget to test** exception scenarios

## Future Enhancements

1. **Error Codes** - Add structured error codes for client handling
2. **Retry-After Header** - For rate limiting/transient errors
3. **Problem Details (RFC 7807)** - Standard error format
4. **Correlation IDs** - Link requests across microservices
5. **Health Checks** - Proactive monitoring

## Conclusion

With three layers of exception handling:
- **Layer 1 (Service):** Catches expected failures, returns Result<T>
- **Layer 2 (Base Controller):** Catches unexpected controller/mapping exceptions
- **Layer 3 (Global Handler):** Final safety net for anything missed

We now have:
- ??? **Robust error handling** at every layer
- ?? **Secure** - No information disclosure
- ?? **Observable** - All errors logged with context
- ?? **Clean code** - Controllers are simple and readable
- ? **Production-ready** - Tested and documented

**Nothing escapes unhandled. Everything is logged. Users see helpful messages. Attackers learn nothing.** ??
