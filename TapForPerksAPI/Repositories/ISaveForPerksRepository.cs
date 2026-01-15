using Microsoft.AspNetCore.Identity;
using TapForPerksAPI.Entities;

namespace TapForPerksAPI.Repositories
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

    }
}
