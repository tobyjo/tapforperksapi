using SaveForPerksAPI.Common;
using SaveForPerksAPI.Models;

namespace SaveForPerksAPI.Services;

public interface IRewardManagementService
{
    Task<Result<RewardDto>> CreateRewardAsync(RewardForCreationDto request, Guid rewardOwnerUserId);

    Task<Result<IEnumerable<RewardDto>>> GetRewardsByRewardOwnerIdAsync(Guid rewardOwnerId, Guid rewardOwnerUserId);
}
