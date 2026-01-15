using Microsoft.EntityFrameworkCore;
using TapForPerksAPI.DbContexts;
using TapForPerksAPI.Entities;


namespace TapForPerksAPI.Repositories
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
    }
}
