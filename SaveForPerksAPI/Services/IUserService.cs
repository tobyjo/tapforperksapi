using SaveForPerksAPI.Common;
using SaveForPerksAPI.Models;

namespace SaveForPerksAPI.Services;

public interface IUserService
{
    Task<Result<UserDto>> GetUserByAuthProviderIdAsync(string authProviderId);

    Task<Result<UserDto>> CreateUserAsync(UserForCreationDto request);
}
