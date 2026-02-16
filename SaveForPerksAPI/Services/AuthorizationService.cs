using SaveForPerksAPI.Common;
using SaveForPerksAPI.Repositories;
using System.Security.Claims;

namespace SaveForPerksAPI.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISaveForPerksRepository _repository;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(
        IHttpContextAccessor httpContextAccessor,
        ISaveForPerksRepository repository,
        ILogger<AuthorizationService> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Result<string> GetAuthProviderIdFromToken()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            _logger.LogWarning("No authenticated user found in context");
            return Result<string>.Failure("User is not authenticated");
        }

        // JWT "sub" claim contains the auth provider ID
        var subClaim = user.FindFirst(ClaimTypes.NameIdentifier) 
                       ?? user.FindFirst("sub");

        if (subClaim == null || string.IsNullOrWhiteSpace(subClaim.Value))
        {
            _logger.LogWarning("No subject claim found in JWT token");
            return Result<string>.Failure("Invalid authentication token");
        }

        _logger.LogDebug("Auth provider ID extracted from token: {AuthProviderId}", subClaim.Value);
        return Result<string>.Success(subClaim.Value);
    }

    public Result<bool> ValidateAuthProviderIdMatch(string authProviderId)
    {
        if (string.IsNullOrWhiteSpace(authProviderId))
        {
            _logger.LogWarning("Validation failed: Auth provider ID is empty");
            return Result<bool>.Failure("Auth provider ID is required");
        }

        var tokenAuthProviderIdResult = GetAuthProviderIdFromToken();
        if (tokenAuthProviderIdResult.IsFailure)
        {
            return Result<bool>.Failure(tokenAuthProviderIdResult.Error!);
        }

        var tokenAuthProviderId = tokenAuthProviderIdResult.Value;

        if (!string.Equals(tokenAuthProviderId, authProviderId, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "Authorization failed: Token auth provider ID does not match. Token: {TokenAuthProviderId}, Provided: {ProvidedAuthProviderId}",
                tokenAuthProviderId, authProviderId);
            return Result<bool>.Failure("You are not authorized to perform this action");
        }

        _logger.LogDebug(
            "Authorization successful: Auth provider IDs match. AuthProviderId: {AuthProviderId}",
            authProviderId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ValidateCustomerAuthorizationAsync(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: Customer ID is empty");
            return Result<bool>.Failure("Customer ID is required");
        }

        // Get the customer to retrieve their auth provider ID
        var customer = await _repository.GetCustomerByIdAsync(customerId);
        if (customer == null)
        {
            _logger.LogWarning("Customer not found for authorization check. CustomerId: {CustomerId}", customerId);
            return Result<bool>.Failure("Customer not found");
        }

        // Validate the token matches the customer's auth provider ID
        var validationResult = ValidateAuthProviderIdMatch(customer.AuthProviderId);
        if (validationResult.IsFailure)
        {
            _logger.LogWarning(
                "Authorization failed for customer. CustomerId: {CustomerId}, AuthProviderId: {AuthProviderId}",
                customerId, customer.AuthProviderId);
            return Result<bool>.Failure(validationResult.Error!);
        }

        _logger.LogInformation(
            "Customer authorization successful. CustomerId: {CustomerId}, AuthProviderId: {AuthProviderId}",
            customerId, customer.AuthProviderId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ValidateBusinessUserAuthorizationAsync(Guid businessUserId)
    {
        if (businessUserId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: Business user ID is empty");
            return Result<bool>.Failure("Business user ID is required");
        }

        // Get the business user to retrieve their auth provider ID
        var businessUser = await _repository.GetBusinessUserByIdAsync(businessUserId);
        if (businessUser == null)
        {
            _logger.LogWarning("Business user not found for authorization check. BusinessUserId: {BusinessUserId}", businessUserId);
            return Result<bool>.Failure("Business user not found");
        }

        // Validate the token matches the business user's auth provider ID
        var validationResult = ValidateAuthProviderIdMatch(businessUser.AuthProviderId);
        if (validationResult.IsFailure)
        {
            _logger.LogWarning(
                "Authorization failed for business user. BusinessUserId: {BusinessUserId}, AuthProviderId: {AuthProviderId}",
                businessUserId, businessUser.AuthProviderId);
            return Result<bool>.Failure(validationResult.Error!);
        }

        _logger.LogInformation(
            "Business user authorization successful. BusinessUserId: {BusinessUserId}, AuthProviderId: {AuthProviderId}",
            businessUserId, businessUser.AuthProviderId);

        return Result<bool>.Success(true);
    }
}
