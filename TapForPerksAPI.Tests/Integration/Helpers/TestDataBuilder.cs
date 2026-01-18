using TapForPerksAPI.DbContexts;
using TapForPerksAPI.Entities;

namespace TapForPerksAPI.Tests.Integration.Helpers;

/// <summary>
/// Fluent builder for creating test data in the database.
/// Makes it easy to set up test scenarios with realistic data.
/// </summary>
public class TestDataBuilder
{
    private readonly TapForPerksContext _context;

    public TestDataBuilder(TapForPerksContext context)
    {
        _context = context;
    }

    #region User Builders

    public async Task<User> CreateUser(
        string name = "Test User",
        string qrCodeValue = "QR001-TEST-9999",
        string? email = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email ?? $"{name.Replace(" ", "").ToLower()}@test.com",
            QrCodeValue = qrCodeValue,
            AuthProviderId = $"auth-{Guid.NewGuid()}",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    #endregion

    #region Reward Owner Builders

    public async Task<RewardOwnerUser> CreateRewardOwner(
        string name = "Test Coffee Shop",
        string? email = null)
    {
        var rewardOwner = new RewardOwner
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"{name} - Test Business",
            CreatedAt = DateTime.UtcNow
        };
        _context.RewardOwners.Add(rewardOwner);
        await _context.SaveChangesAsync();

        var rewardOwnerUser = new RewardOwnerUser
        {
            Id = Guid.NewGuid(),
            RewardOwnerId = rewardOwner.Id,
            Name = name,
            Email = email ?? $"{name.Replace(" ", "").ToLower()}@business.com",
            AuthProviderId = $"auth-{Guid.NewGuid()}",
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.RewardOwnerUsers.Add(rewardOwnerUser);
        await _context.SaveChangesAsync();
        return rewardOwnerUser;
    }

    #endregion

    #region Reward Builders

    public async Task<Reward> CreateReward(
        string name = "Free Coffee",
        int costPoints = 5,
        RewardType rewardType = RewardType.IncrementalPoints,
        RewardOwnerUser? rewardOwner = null)
    {
        // Create reward owner if not provided
        if (rewardOwner == null)
        {
            rewardOwner = await CreateRewardOwner();
        }

        var reward = new Reward
        {
            Id = Guid.NewGuid(),
            Name = name,
            CostPoints = costPoints,
            RewardType = rewardType,
            RewardOwnerId = rewardOwner.RewardOwnerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Rewards.Add(reward);
        await _context.SaveChangesAsync();
        return reward;
    }

    #endregion

    #region User Balance Builders

    public async Task<UserBalance> CreateUserBalance(
        User user,
        Reward reward,
        int balance = 0)
    {
        var userBalance = new UserBalance
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RewardId = reward.Id,
            Balance = balance,
            LastUpdated = DateTime.UtcNow
        };

        _context.UserBalances.Add(userBalance);
        await _context.SaveChangesAsync();
        return userBalance;
    }

    #endregion

    #region Scan Event Builders

    public async Task<ScanEvent> CreateScanEvent(
        User user,
        Reward reward,
        int pointsChange = 1,
        RewardOwnerUser? rewardOwner = null)
    {
        var scanEvent = new ScanEvent
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RewardId = reward.Id,
            RewardOwnerUserId = rewardOwner?.Id,
            QrCodeValue = user.QrCodeValue,
            PointsChange = pointsChange,
            ScannedAt = DateTime.UtcNow
        };

        _context.ScanEvents.Add(scanEvent);
        await _context.SaveChangesAsync();
        return scanEvent;
    }

    #endregion

    #region Reward Redemption Builders

    public async Task<RewardRedemption> CreateRewardRedemption(
        User user,
        Reward reward,
        RewardOwnerUser? rewardOwner = null)
    {
        var redemption = new RewardRedemption
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RewardId = reward.Id,
            RewardOwnerUserId = rewardOwner?.Id,
            RedeemedAt = DateTime.UtcNow
        };

        _context.RewardRedemptions.Add(redemption);
        await _context.SaveChangesAsync();
        return redemption;
    }

    #endregion

    #region Scenario Builders (Common Test Scenarios)

    /// <summary>
    /// Creates a complete scenario: User with points ready to claim a reward
    /// </summary>
    public async Task<(User user, Reward reward, UserBalance balance)> CreateUserReadyToClaimReward(
        int currentPoints = 10,
        int rewardCost = 5)
    {
        var user = await CreateUser("Alice", "QR001-ALICE-9999");
        var reward = await CreateReward("Free Coffee", rewardCost);
        var balance = await CreateUserBalance(user, reward, currentPoints);

        return (user, reward, balance);
    }

    /// <summary>
    /// Creates a scenario: New user with no points yet
    /// </summary>
    public async Task<(User user, Reward reward)> CreateNewUserScenario()
    {
        var user = await CreateUser("Bob", "QR002-BOB-8888");
        var reward = await CreateReward("Free Coffee", 5);

        return (user, reward);
    }

    /// <summary>
    /// Creates a scenario: User with insufficient points
    /// </summary>
    public async Task<(User user, Reward reward, UserBalance balance)> CreateUserWithInsufficientPoints(
        int currentPoints = 3,
        int rewardCost = 5)
    {
        var user = await CreateUser("Charlie", "QR003-CHARLIE-7777");
        var reward = await CreateReward("Free Coffee", rewardCost);
        var balance = await CreateUserBalance(user, reward, currentPoints);

        return (user, reward, balance);
    }

    /// <summary>
    /// Creates a scenario: User with scan history
    /// </summary>
    public async Task<(User user, Reward reward, List<ScanEvent> scanEvents)> CreateUserWithScanHistory(
        int numberOfScans = 5)
    {
        var user = await CreateUser("Diana", "QR004-DIANA-6666");
        var reward = await CreateReward("Free Coffee", 5);
        var rewardOwner = await CreateRewardOwner("Test Cafe");

        var scanEvents = new List<ScanEvent>();
        for (int i = 0; i < numberOfScans; i++)
        {
            var scanEvent = await CreateScanEvent(user, reward, 1, rewardOwner);
            scanEvents.Add(scanEvent);
        }

        // Create balance matching scan history
        await CreateUserBalance(user, reward, numberOfScans);

        return (user, reward, scanEvents);
    }

    #endregion
}
