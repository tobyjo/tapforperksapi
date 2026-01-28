using Microsoft.EntityFrameworkCore;
using SaveForPerksAPI.DbContexts;
using SaveForPerksAPI.Entities;


namespace SaveForPerksAPI.Repositories
{
    public class SaveForPerksRepository : ISaveForPerksRepository
    {

        private readonly TapForPerksContext _context;
        public SaveForPerksRepository(TapForPerksContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }


        public async Task<IEnumerable<RewardOwner>> GetRewardOwnersAsync()
        {
            return await _context.RewardOwners.OrderBy(lo => lo.Name).ToListAsync();
        }
  

        public async Task<ScanEvent?> GetScanEventAsync(Guid rewardId, Guid scanEventId)
        {
            return await _context.ScanEvents
                .Where(se => se.RewardId == rewardId && se.Id == scanEventId)
                .FirstOrDefaultAsync();
        }

        public async Task CreateScanEvent(ScanEvent scanEvent)
        {
            if (scanEvent == null)
            {
                throw new ArgumentNullException(nameof(scanEvent));
            }
            await _context.ScanEvents.AddAsync(scanEvent);
        }


        public async Task<User?> GetUserByQrCodeValueAsync(string qrCodeValue)
            {
            return await _context.Users
                .Where(u => u.QrCodeValue == qrCodeValue)
                .FirstOrDefaultAsync();
        }

        public async Task<Reward?> GetRewardAsync(Guid rewardId)
        {
            return await _context.Rewards
                .Where(lp => lp.Id == rewardId)
                .FirstOrDefaultAsync();
        } 

        public async Task<UserBalance?> GetUserBalanceForRewardAsync(Guid userId, Guid rewardId)
        {
            return await _context.UserBalances
                .Where(ub => ub.UserId == userId && ub.RewardId == rewardId)
                .FirstOrDefaultAsync();
        }

        public async Task CreateUserBalance(UserBalance userBalance)
        {
            if (userBalance == null)
            {
                throw new ArgumentNullException(nameof(userBalance));
            }
            await _context.UserBalances.AddAsync(userBalance);
        }

        public async Task<RewardRedemption?> GetRewardRedemptionAsync(Guid rewardId, Guid rewardRedemptionId)
        {
            return await _context.RewardRedemptions
                .Where(rr => rr.RewardId == rewardId && rr.Id == rewardRedemptionId)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateUserBalance(UserBalance userBalance)
        {
            // No implementation needed for EF Core as it tracks changes automatically
            await Task.CompletedTask;
        }

        public async Task CreateRewardRedemption(RewardRedemption rewardRedemption)
        {
            if (rewardRedemption == null)
            {
                throw new ArgumentNullException(nameof(rewardRedemption));
            }
            await _context.RewardRedemptions.AddAsync(rewardRedemption);
        }

        public async Task<RewardOwnerUser?> GetRewardOwnerUserByEmailAsync(string email)
        {
            return await _context.RewardOwnerUsers
                .Where(rou => rou.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task CreateRewardOwnerAsync(RewardOwner rewardOwner)
        {
            if (rewardOwner == null)
            {
                throw new ArgumentNullException(nameof(rewardOwner));
            }
            await _context.RewardOwners.AddAsync(rewardOwner);
        }

        public async Task CreateRewardOwnerUserAsync(RewardOwnerUser rewardOwnerUser)
        {
            if (rewardOwnerUser == null)
            {
                throw new ArgumentNullException(nameof(rewardOwnerUser));
            }
            await _context.RewardOwnerUsers.AddAsync(rewardOwnerUser);
        }

        public async Task<RewardOwnerUser?> GetRewardOwnerUserByAuthProviderIdAsync(string authProviderId)
        {
            return await _context.RewardOwnerUsers
                .Where(rou => rou.AuthProviderId == authProviderId)
                .FirstOrDefaultAsync();
        }

        public async Task<RewardOwnerUser?> GetRewardOwnerUserByIdAsync(Guid rewardOwnerUserId)
        {
            return await _context.RewardOwnerUsers
                .Where(rou => rou.Id == rewardOwnerUserId)
                .FirstOrDefaultAsync();
        }

        public async Task<RewardOwner?> GetRewardOwnerByIdAsync(Guid rewardOwnerId)
        {
            return await _context.RewardOwners
                .Where(ro => ro.Id == rewardOwnerId)
                .FirstOrDefaultAsync();
        }

        public async Task<Reward?> GetRewardByRewardOwnerIdAsync(Guid rewardOwnerId)
        {
            return await _context.Rewards
                .Where(r => r.RewardOwnerId == rewardOwnerId)
                .FirstOrDefaultAsync();
        }

        public async Task CreateRewardAsync(Reward reward)
        {
            if (reward == null)
            {
                throw new ArgumentNullException(nameof(reward));
            }
            await _context.Rewards.AddAsync(reward);
        }
    }
}
