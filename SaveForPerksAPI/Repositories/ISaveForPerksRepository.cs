using Microsoft.AspNetCore.Identity;
using SaveForPerksAPI.Entities;

namespace SaveForPerksAPI.Repositories
{
    public interface ISaveForPerksRepository
    {
        Task<bool> SaveChangesAsync();

        Task<IEnumerable<RewardOwner>> GetRewardOwnersAsync();

        Task<ScanEvent?> GetScanEventAsync(Guid rewardId, Guid scanEventId);

        Task CreateScanEvent(ScanEvent scanEvent);

        Task<User?> GetUserByQrCodeValueAsync(string qrCodeValue);

        Task<Reward?> GetRewardAsync(Guid rewardId);

        Task<UserBalance?> GetUserBalanceForRewardAsync(Guid userId,  Guid rewardId);

        Task CreateUserBalance(UserBalance userBalance);

        Task<RewardRedemption?> GetRewardRedemptionAsync(Guid rewardId, Guid rewardRedemptionId);

        Task UpdateUserBalance(UserBalance userBalance);

        Task CreateRewardRedemption(RewardRedemption rewardRedemption);

        Task<RewardOwnerUser?> GetRewardOwnerUserByEmailAsync(string email);

        Task CreateRewardOwnerAsync(RewardOwner rewardOwner);

        Task CreateRewardOwnerUserAsync(RewardOwnerUser rewardOwnerUser);

        Task<RewardOwnerUser?> GetRewardOwnerUserByAuthProviderIdAsync(string authProviderId);

        Task<RewardOwner?> GetRewardOwnerByIdAsync(Guid rewardOwnerId);

        Task<Reward?> GetRewardByRewardOwnerIdAsync(Guid rewardOwnerId);

        Task CreateRewardAsync(Reward reward);

    }
}
