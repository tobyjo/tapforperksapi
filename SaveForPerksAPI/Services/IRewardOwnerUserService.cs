using SaveForPerksAPI.Common;
using SaveForPerksAPI.Models;

namespace SaveForPerksAPI.Services;

public interface IRewardOwnerUserService
{
    Task<Result<IEnumerable<RewardOwnerDto>>> GetRewardOwnersByAuthProviderIdAsync(string authProviderId);
}
