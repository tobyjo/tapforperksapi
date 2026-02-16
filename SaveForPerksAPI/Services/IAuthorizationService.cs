using SaveForPerksAPI.Common;

namespace SaveForPerksAPI.Services;

public interface IAuthorizationService
{
    /// <summary>
    /// Validates that the JWT subject claim matches the given auth provider ID
    /// </summary>
    Result<bool> ValidateAuthProviderIdMatch(string authProviderId);

    /// <summary>
    /// Validates that the JWT subject claim matches the auth provider ID of the given customer
    /// </summary>
    Task<Result<bool>> ValidateCustomerAuthorizationAsync(Guid customerId);

    /// <summary>
    /// Validates that the JWT subject claim matches the auth provider ID of the given business user
    /// </summary>
    Task<Result<bool>> ValidateBusinessUserAuthorizationAsync(Guid businessUserId);

    /// <summary>
    /// Gets the auth provider ID (subject claim) from the current JWT token
    /// </summary>
    Result<string> GetAuthProviderIdFromToken();
}
