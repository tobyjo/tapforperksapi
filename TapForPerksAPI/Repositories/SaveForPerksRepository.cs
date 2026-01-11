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


        public async Task<IEnumerable<LoyaltyOwner>> GetLoyaltyOwnersAsync()
        {
            return await _context.LoyaltyOwners.OrderBy(lo => lo.Name).ToListAsync();
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

        public async Task<UserBalance?> GetUserBalanceAsync(Guid userId, Guid rewardId)
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

    }
}
