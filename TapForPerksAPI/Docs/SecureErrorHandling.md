# Secure Error Handling - Implementation Guide

## Overview
Implemented secure error handling that balances user experience with security, preventing information disclosure while maintaining helpful messages for legitimate users.

## Security Principles

### ? What We Did Right

#### 1. **Generic Messages for Resource Lookups**
**Why:** Prevents enumeration attacks and information disclosure.

**Before (? Insecure):**
```json
{
  "error": "Reward not found with ID: 33333333-3333-3333-3333-333333333333"
}
```
- Attacker learns this reward ID doesn't exist
- Can enumerate valid vs invalid IDs
- Reveals system structure

**After (? Secure):**
```json
{
  "error": "Invalid QR code or reward"
}
```
- Doesn't reveal which resource failed
- Can't enumerate valid IDs
- Full details logged server-side

#### 2. **Detailed Messages for User's Own Data**
**Why:** Improves UX without security risk since user owns the data.

**? Safe to Show:**
```json
{
  "error": "Insufficient points. Required: 10, Available: 5"
}
```
- User is viewing their own balance
- Already authenticated and authorized
- Helps user understand what they need

#### 3. **Comprehensive Logging**
**Why:** Developers get details they need without exposing them to potential attackers.

**Server Logs:**
```
[Warning] User not found. QrCode: QR001-INVALID, RewardId: 33333333-...
[Warning] Reward not found. RewardId: 33333333-..., UserId: 99999999-..., QrCode: QR001
[Information] Insufficient points. User: 99999999-... (Alice), Required: 10, Available: 5
[Information] Rewards claimed. User: Alice, Reward: Free Coffee, Count: 2, PointsDeducted: 10
```

## Error Message Strategy

### Resource Not Found Errors

| Scenario | User Sees | Logs Contain | Reasoning |
|----------|-----------|--------------|-----------|
| Invalid QR Code | "Invalid QR code or reward" | QR code, reward ID, attempt details | Prevents QR code enumeration |
| Invalid Reward ID | "Invalid QR code or reward" | Reward ID, user ID, QR code | Prevents reward ID enumeration |
| Scan Event Not Found | "Scan event not found" | Scan event ID, reward ID | Generic - don't reveal IDs |

**Security Benefit:** Attacker can't determine:
- Which resource doesn't exist
- Valid QR codes or reward IDs
- System structure or data relationships

### User Data Errors

| Scenario | User Sees | Logs Contain | Reasoning |
|----------|-----------|--------------|-----------|
| Insufficient Points | "Insufficient points. Required: 10, Available: 5" | User ID, name, reward details | User's own data - safe to show |
| No Balance Yet | "You don't have any points for this reward yet" | User ID, reward ID, attempt context | Helpful guidance |
| Invalid Claim Count | "Must be between 0 and 100" | User ID, requested count | Input validation feedback |

**UX Benefit:** Users understand:
- Exactly what they need
- Why their action failed
- How to resolve the issue

### System Errors

| Scenario | User Sees | Logs Contain | Reasoning |
|----------|-----------|--------------|-----------|
| Database Exception | "An error occurred while processing your request" | Full exception, stack trace, user context | Hide internal errors |
| Transaction Failed | "An error occurred while processing your request" | Full transaction details, involved entities | Prevent disclosure |

**Security Benefit:** System internals remain hidden.

## Implementation Details

### Code Structure

```csharp
// Pattern: Log detailed info, return generic message
var user = await _repository.GetUserByQrCodeValueAsync(qrCode);
if (user == null)
{
    // ? Log everything for debugging
    _logger.LogWarning(
        "User not found. QrCode: {QrCode}, RewardId: {RewardId}", 
        qrCode, rewardId);
    
    // ? Return generic message to client
    return Result.Failure("Invalid QR code or reward");
}
```

### Logging Levels

| Level | Use Case | Example |
|-------|----------|---------|
| **LogError** | System failures, exceptions | Transaction failed, database error |
| **LogWarning** | Resource not found, invalid access | Invalid QR code, reward not found |
| **LogInformation** | Successful operations, business events | Scan created, rewards claimed, points added |
| **LogDebug** | Detailed flow for debugging | (Not in production) |

### Log Structured Data

? **Good:**
```csharp
_logger.LogWarning(
    "Reward not found. RewardId: {RewardId}, UserId: {UserId}", 
    rewardId, userId);
```
- Searchable
- Queryable in log aggregation tools
- Consistent format

? **Bad:**
```csharp
_logger.LogWarning($"Reward {rewardId} not found for user {userId}");
```
- Hard to query
- String interpolation issues
- Not structured

## Attack Scenarios Prevented

### 1. Enumeration Attack
**Before:**
```
Attacker: POST /api/scans with QR: TEST-001
Response: "User not found with QR code: TEST-001"

Attacker: POST /api/scans with QR: QR001-ALICE-9999
Response: "Reward not found with ID: 33333333-..."
         
# Attacker now knows QR001-ALICE-9999 is valid!
```

**After:**
```
Attacker: POST /api/scans with QR: TEST-001
Response: "Invalid QR code or reward"

Attacker: POST /api/scans with QR: QR001-ALICE-9999
Response: "Invalid QR code or reward"

# Attacker learns nothing - both responses identical
```

### 2. Information Disclosure
**Before:**
```
Attacker: GET /api/scans/{reward-id}/events/{scan-id}
Response: "Scan event not found with ID: 12345678-... for reward: 33333333-..."

# Attacker learns:
# - Reward ID format
# - Scan event ID format  
# - These IDs don't exist (can enumerate to find valid ones)
```

**After:**
```
Attacker: GET /api/scans/{reward-id}/events/{scan-id}
Response: "Scan event not found"

# Attacker learns nothing useful
# Server logs contain full context for debugging
```

## Monitoring & Alerting

### Alert on Suspicious Patterns

```csharp
// Set up monitoring for:
// - Multiple 404s from same IP (enumeration attempt)
// - Rapid-fire requests with different IDs (scanning)
// - Access to many different rewards (reconnaissance)

// Example Application Insights query:
// requests
// | where resultCode == 404
// | where url contains "/api/scans"
// | summarize count() by client_IP, bin(timestamp, 5m)
// | where count_ > 20  // More than 20 404s in 5 minutes
```

### Useful Log Queries

**Find enumeration attempts:**
```kusto
traces
| where message contains "not found"
| where customDimensions.QrCode != ""
| summarize attempts = count() by QrCode, bin(timestamp, 1h)
| where attempts > 10
```

**Find invalid reward access:**
```kusto
traces
| where message contains "Reward not found"
| summarize count() by RewardId, UserId
| order by count_ desc
```

## Best Practices Summary

### ? DO:

1. **Log detailed information** - Include all context (IDs, values, user info)
2. **Use structured logging** - Use templates with parameters, not string interpolation
3. **Return generic messages** for resource lookups - Don't reveal what failed
4. **Return specific messages** for user's own data - Help them succeed
5. **Monitor for patterns** - Set up alerts for suspicious activity
6. **Use appropriate log levels** - Error/Warning/Information consistently

### ? DON'T:

1. **Don't reveal resource IDs** in error messages to unauthenticated users
2. **Don't expose stack traces** in production API responses
3. **Don't use string interpolation** in log messages (use structured logging)
4. **Don't log sensitive data** - passwords, tokens, credit cards
5. **Don't give different errors** that reveal information (e.g., "invalid password" vs "user not found")
6. **Don't skip logging** - Every error should be logged with context

## Testing

### Security Tests (Manual)

```bash
# Test 1: Invalid QR code
curl -X POST http://localhost:5040/api/reward-owner/scans \
  -H "Content-Type: application/json" \
  -d '{"qrCodeValue":"INVALID","rewardId":"33333333-3333-3333-3333-333333333333"}'

# Should return: "Invalid QR code or reward" (generic)
# Should log: Full details including QR code and reward ID

# Test 2: Invalid reward ID  
curl -X POST http://localhost:5040/api/reward-owner/scans \
  -H "Content-Type: application/json" \
  -d '{"qrCodeValue":"QR001-ALICE-9999","rewardId":"99999999-9999-9999-9999-999999999999"}'

# Should return: "Invalid QR code or reward" (generic - same as Test 1!)
# Should log: Full details

# Test 3: Insufficient points (user's own data)
# Should return: Detailed message with point counts
```

### Unit Test Coverage

? All scenarios tested:
- Generic error messages for resource lookups
- Detailed messages for user data
- Logging occurs for all errors
- No information disclosure in responses

## Future Enhancements

1. **Rate Limiting** - Prevent brute force enumeration
2. **CAPTCHA** - For repeated failed attempts
3. **Error Codes** - Structured error codes for client handling
4. **Geo-blocking** - Block suspicious regions if applicable
5. **Honeypot Endpoints** - Detect and track attackers

## Conclusion

This implementation provides:
- ?? **Security** - No information disclosure to potential attackers
- ?? **Usability** - Helpful messages for legitimate users
- ?? **Observability** - Rich logging for debugging and monitoring
- ?? **Balance** - The right message for the right audience

**The key principle:** Users see helpful messages for their own data, generic messages for everything else, and developers see everything in logs.
