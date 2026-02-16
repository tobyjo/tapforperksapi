# Authorization Service Usage Guide

## Overview
The `AuthorizationService` validates that JWT tokens match the user making the request. It prevents users from accessing or modifying data belonging to other users.

## How JWT Validation Works
1. JWT token contains a "sub" (subject) claim with the user's Auth Provider ID (e.g., from Auth0)
2. This Auth Provider ID is stored in the database for each Customer and BusinessUser
3. The service validates that the JWT "sub" matches the auth_provider_id in the database

## Methods Available

### 1. `ValidateAuthProviderIdMatch(string authProviderId)`
**Use when:** Request body contains the auth provider ID directly

**Example:**
```csharp
// In CreateCustomerAsync - auth provider ID is in the request
var authCheck = _authorizationService.ValidateAuthProviderIdMatch(request.AuthProviderId);
if (authCheck.IsFailure)
    return Result<CustomerDto>.Failure(authCheck.Error!);
```

### 2. `ValidateCustomerAuthorizationAsync(Guid customerId)`
**Use when:** Request has a customer ID (looks up auth provider ID automatically)

**Example:**
```csharp
// In GetDashboardAsync - only have customer ID
var authCheck = await _authorizationService.ValidateCustomerAuthorizationAsync(customerId);
if (authCheck.IsFailure)
    return Result<CustomerDashboardDto>.Failure(authCheck.Error!);
```

### 3. `ValidateBusinessUserAuthorizationAsync(Guid businessUserId)`
**Use when:** Request has a business user ID (for future business user endpoints)

**Example:**
```csharp
// In business user operations
var authCheck = await _authorizationService.ValidateBusinessUserAuthorizationAsync(businessUserId);
if (authCheck.IsFailure)
    return Result<BusinessUserDto>.Failure(authCheck.Error!);
```

### 4. `GetAuthProviderIdFromToken()`
**Use when:** You need to extract the auth provider ID from the JWT

**Example:**
```csharp
var authProviderIdResult = _authorizationService.GetAuthProviderIdFromToken();
if (authProviderIdResult.IsFailure)
    return Result<CustomerDto>.Failure(authProviderIdResult.Error!);

var authProviderId = authProviderIdResult.Value;
```

## Implementation Pattern

### Step 1: Inject the service
```csharp
public class CustomerService : ICustomerService
{
    private readonly IAuthorizationService _authorizationService;

    public CustomerService(IAuthorizationService authorizationService, ...)
    {
        _authorizationService = authorizationService;
    }
}
```

### Step 2: Add validation as first step
```csharp
public async Task<Result<SomeDto>> SomeMethod(Guid customerId)
{
    // ALWAYS validate authorization FIRST
    var authCheck = await _authorizationService.ValidateCustomerAuthorizationAsync(customerId);
    if (authCheck.IsFailure)
        return Result<SomeDto>.Failure(authCheck.Error!);

    // Continue with business logic...
}
```

## When to Use Each Method

| Scenario | Method to Use | Why |
|----------|---------------|-----|
| Request body has `authProviderId` | `ValidateAuthProviderIdMatch()` | Direct comparison |
| Request has `customerId` | `ValidateCustomerAuthorizationAsync()` | Auto-lookup auth ID |
| Request has `businessUserId` | `ValidateBusinessUserAuthorizationAsync()` | Auto-lookup auth ID |
| Need the auth ID value | `GetAuthProviderIdFromToken()` | Extract from JWT |

## Example: Complete Customer Endpoint

```csharp
[HttpDelete("{customerId}")]
public async Task<ActionResult> DeleteCustomer(Guid customerId)
{
    // Controller just passes to service
    var result = await _customerService.DeleteCustomerAsync(customerId);
    return result.IsSuccess ? NoContent() : BadRequest(result.Error);
}

// In CustomerService
public async Task<Result<bool>> DeleteCustomerAsync(Guid customerId)
{
    // Step 1: ALWAYS validate authorization first
    var authCheck = await _authorizationService.ValidateCustomerAuthorizationAsync(customerId);
    if (authCheck.IsFailure)
        return Result<bool>.Failure(authCheck.Error!);

    // Step 2: Continue with business logic
    // User is now authorized to delete this customer
    // ...
}
```

## Security Benefits

✅ **Prevents impersonation** - User A can't access User B's data  
✅ **Centralized validation** - One place for all auth logic  
✅ **Reusable** - Works for Customers and BusinessUsers  
✅ **Consistent errors** - Standard "You are not authorized" messages  
✅ **Logged** - All authorization attempts are logged

## Error Messages

- `"User is not authenticated"` - No JWT token present
- `"Invalid authentication token"` - JWT doesn't have sub claim
- `"You are not authorized to perform this action"` - JWT sub doesn't match
- `"Customer not found"` - Customer ID doesn't exist
- `"Business user not found"` - Business user ID doesn't exist

## Next Steps for Business Users

When implementing business user endpoints, follow the same pattern:
```csharp
var authCheck = await _authorizationService.ValidateBusinessUserAuthorizationAsync(businessUserId);
if (authCheck.IsFailure)
    return Result<SomeDto>.Failure(authCheck.Error!);
```
