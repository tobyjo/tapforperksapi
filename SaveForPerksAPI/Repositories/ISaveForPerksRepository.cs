using Microsoft.AspNetCore.Identity;
using SaveForPerksAPI.Entities;

namespace SaveForPerksAPI.Repositories
{
    public interface ISaveForPerksRepository
    {
        Task<bool> SaveChangesAsync();

        Task<IEnumerable<Business>> GetBusinessesAsync();

        Task<ScanEvent?> GetScanEventAsync(Guid rewardId, Guid scanEventId);

        Task CreateScanEvent(ScanEvent scanEvent);

        Task<Customer?> GetCustomerByQrCodeValueAsync(string qrCodeValue);

        Task<Customer?> GetCustomerByAuthProviderIdAsync(string authProviderId);

        Task<Customer?> GetCustomerByEmailAsync(string email);

        Task CreateCustomerAsync(Customer customer);

        Task<Reward?> GetRewardAsync(Guid rewardId);

        Task<CustomerBalance?> GetCustomerBalanceForRewardAsync(Guid customerId,  Guid rewardId);

        Task CreateUserBalance(CustomerBalance customerBalance);

        Task<RewardRedemption?> GetRewardRedemptionAsync(Guid rewardId, Guid rewardRedemptionId);

        Task UpdateCustomerBalance(CustomerBalance customerBalance);

        Task CreateRewardRedemption(RewardRedemption rewardRedemption);

        Task<BusinessUser?> GetBusinessUserByEmailAsync(string email);

        Task CreateBusinessAsync(Business business);

        Task CreateBusinessUserAsync(BusinessUser businessUser);

        Task<BusinessUser?> GetBusinessUserByAuthProviderIdAsync(string authProviderId);

        Task<BusinessUser?> GetBusinessUserByIdAsync(Guid businessUserId);

        Task<Business?> GetBusinessByIdAsync(Guid businessId);

        Task<Reward?> GetRewardByBusinessIdAsync(Guid businessId);

        Task CreateRewardAsync(Reward reward);

        Task<IEnumerable<BusinessCategory>> GetAllBusinessCategoriesAsync();

        Task<BusinessCategory?> GetBusinessCategoryByIdAsync(Guid categoryId);

        Task<IEnumerable<CustomerBalance>> GetCustomerBalancesWithDetailsAsync(Guid customerId);

        Task<int> GetLifetimeRewardsClaimedCountAsync(Guid customerId);

        Task<int> GetLifetimePointsEarnedAsync(Guid customerId);

        Task<int> GetLast30DaysPointsEarnedAsync(Guid customerId);

        Task<int> GetLast30DaysScansCountAsync(Guid customerId);

        Task<int> GetLast30DaysRewardsClaimedCountAsync(Guid customerId);

        Task<DateTime?> GetMostRecentScanDateForBusinessAsync(Guid customerId, Guid businessId);

    }
}
