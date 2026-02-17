using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaveForPerksAPI.Entities;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Tests.Integration.Fixtures;
using SaveForPerksAPI.Tests.Integration.Helpers;
using Xunit;

namespace SaveForPerksAPI.Tests.Integration.Services;

/// <summary>
/// Integration tests for RewardService using real in-memory database.
/// These tests verify actual database operations, transactions, and business logic.
/// Each test gets a fresh in-memory database to ensure complete isolation.
/// </summary>
public class RewardServiceIntegrationTests : IDisposable
{
    private readonly DatabaseFixture _fixture;
    private readonly TestDataBuilder _testData;

    public RewardServiceIntegrationTests()
    {
        _fixture = new DatabaseFixture(); // Creates fresh in-memory DB automatically
        _testData = new TestDataBuilder(_fixture.Context);
    }

    public void Dispose()
    {
        _fixture?.Dispose();
    }

    #region Verify In-Memory Database

    [Fact]
    public void VerifyInMemoryDatabase_NoRealDatabaseConnection()
    {
        // ✅ PROOF: This test verifies we're using in-memory database, NOT SQL Server
        // You can STOP SQL Server service and this test will still pass!
        
        // Verify provider is InMemory (not SQL Server)
        _fixture.Context.Database.ProviderName.Should().Be("Microsoft.EntityFrameworkCore.InMemory",
            "because we're using in-memory database, not SQL Server");
        
        // Try to get connection string - should throw for in-memory
        var gettingConnectionString = () => _fixture.Context.Database.GetConnectionString();
        gettingConnectionString.Should().Throw<InvalidOperationException>(
            "because in-memory databases don't have connection strings - they exist only in RAM");
    }

    [Fact]
    public async Task VerifyRepository_UsesInMemoryContext()
    {
        // This test proves the Repository is using the in-memory context
        
        // Create a user in the in-memory context directly
        var testUser = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "DirectContextUser",
            QrCodeValue = "QR-DIRECT-TEST",
            Email = "direct@test.com",
            AuthProviderId = "auth-123",
            CreatedAt = DateTime.UtcNow
        };
        _fixture.Context.Customer.Add(testUser);
        await _fixture.Context.SaveChangesAsync();
        
        // Now query via Repository - should find the same user
        var foundUser = await _fixture.Repository.GetCustomerByQrCodeValueAsync("QR-DIRECT-TEST");
        
        foundUser.Should().NotBeNull("because the repository should use the same in-memory context");
        foundUser!.Name.Should().Be("DirectContextUser", 
            "because this proves the Repository is querying the SAME in-memory database we just wrote to");
        foundUser.Id.Should().Be(testUser.Id);
    }

    #endregion

    #region ProcessScanAndRewardsAsync - New User Scenarios

    [Fact]
    public async Task ProcessScan_NewUser_CreatesBalanceAndScanEvent()
    {
        // Arrange - Create test data
        var (user, reward, businessUser) = await _testData.CreateNewUserScenario();

        var request = new ScanEventForCreationDto
        {
            QrCodeValue = user.QrCodeValue,
            RewardId = reward.Id,
            PointsChange = 1,
            NumRewardsToClaim = 0
        };

        // Act - Call real service with real database
        var result = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId, 
            businessUser.Id, 
            request);

        // Assert - Verify response
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CustomerName.Should().Be(user.Name);
        result.Value.CurrentBalance.Should().Be(1);
        result.Value.RewardAvailable.Should().BeFalse(); // Not enough points yet

        // Assert - Verify database state
        var balance = await _fixture.Context.CustomerBalances
            .FirstOrDefaultAsync(ub => ub.CustomerId == user.Id && ub.RewardId == reward.Id);
        balance.Should().NotBeNull();
        balance!.Balance.Should().Be(1);

        var scanEvent = await _fixture.Context.ScanEvents
            .FirstOrDefaultAsync(se => se.CustomerId == user.Id && se.RewardId == reward.Id);
        scanEvent.Should().NotBeNull();
        scanEvent!.PointsChange.Should().Be(1);
        scanEvent.QrCodeValue.Should().Be(user.QrCodeValue);
    }

    [Fact]
    public async Task ProcessScan_ExistingUser_UpdatesBalance()
    {
        // Arrange - Customer already has 4 points (use unique QR to avoid collision with real DB)
        var uniqueQr = $"TEST-EXISTING-{Guid.NewGuid()}";
        var businessUser = await _testData.CreateRewardOwner();
        var user = await _testData.CreateUser("ExistingUser", uniqueQr);
        var reward = await _testData.CreateReward("Free Coffee", costPoints: 5, rewardOwner: businessUser);
        await _testData.CreateUserBalance(user, reward, balance: 4);

        var request = new ScanEventForCreationDto
        {
            QrCodeValue = user.QrCodeValue,
            RewardId = reward.Id,
            PointsChange = 1,
            NumRewardsToClaim = 0
        };

        // Act
        var result = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId,
            businessUser.Id,
            request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentBalance.Should().Be(5, "because user had 4 points and we added 1");
        result.Value.RewardAvailable.Should().BeTrue(); // Now has enough!
        result.Value.NumRewardsAvailable.Should().Be(1);

        // Verify database
        var balance = await _fixture.Context.CustomerBalances
            .FirstAsync(ub => ub.CustomerId == user.Id && ub.RewardId == reward.Id);
        balance.Balance.Should().Be(5);
    }

    #endregion

    #region ProcessScanAndRewardsAsync - Reward Claiming Scenarios


    [Fact]
    public async Task Toby_ProcessScan_BalanceWithPointsMatchesReward_ClaimNoReward_AddsPoints()
    {
        // Arrange - Customer has 3 points, claims 2 points which now matches reward (costs 5 points) but cannot claim yet 
        // Use unique QR code to avoid collision with real database
        var uniqueQr = $"TEST-CLAIM-{Guid.NewGuid()}";
        var businessUser = await _testData.CreateRewardOwner();
        var user = await _testData.CreateUser("TobyUser", uniqueQr);
        var reward = await _testData.CreateReward("Free Coffee after 5 paid coffees", costPoints: 5, rewardOwner: businessUser);
        await _testData.CreateUserBalance(user, reward, balance: 3);

        var request = new ScanEventForCreationDto
        {
            QrCodeValue = user.QrCodeValue,
            RewardId = reward.Id,
            PointsChange = 2, // Add 2 more points
            NumRewardsToClaim = 0 // Claim 1 reward
        };

        // Act
        var result = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId,
            businessUser.Id,
            request);

        // Assert - Response
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentBalance.Should().Be(5, "because 3 + 2 = 5");
        result.Value.ClaimedRewards.Should().BeNull();
        /*
  result.Value.ClaimedRewards!.NumberClaimed.Should().Be(1);
        result.Value.ClaimedRewards.RewardName.Should().Be("Free Coffee");
        result.Value.ClaimedRewards.TotalPointsDeducted.Should().Be(5);
        result.Value.ClaimedRewards.RedemptionIds.Should().HaveCount(1);
        */
        // Assert - Database: Balance updated
        var updatedBalance = await _fixture.Context.CustomerBalances
            .FirstAsync(ub => ub.CustomerId == user.Id && ub.RewardId == reward.Id);
        updatedBalance.Balance.Should().Be(5);

        // Assert - Database: Redemption created
        var redemptions = await _fixture.Context.RewardRedemptions
            .Where(r => r.CustomerId == user.Id && r.RewardId == reward.Id)
            .ToListAsync();
        redemptions.Should().HaveCount(0);

    }

    [Fact]
    public async Task ProcessScan_ClaimOneReward_DeductsPointsAndCreatesRedemption()
    {
        // Arrange - Customer has 10 points, wants to claim 1 reward (costs 5 points)
        // Use unique QR code to avoid collision with real database
        var uniqueQr = $"TEST-CLAIM-{Guid.NewGuid()}";
        var businessUser = await _testData.CreateRewardOwner();
        var user = await _testData.CreateUser("ClaimUser", uniqueQr);
        var reward = await _testData.CreateReward("Free Coffee", costPoints: 5, rewardOwner: businessUser);
        await _testData.CreateUserBalance(user, reward, balance: 10);

        var request = new ScanEventForCreationDto
        {
            QrCodeValue = user.QrCodeValue,
            RewardId = reward.Id,
            PointsChange = 1, // Add 1 more point
            NumRewardsToClaim = 1 // Claim 1 reward
        };

        // Act
        var result = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId,
            businessUser.Id,
            request);

        // Assert - Response
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentBalance.Should().Be(6, "because 10 + 1 - 5 = 6");
        result.Value.ClaimedRewards.Should().NotBeNull();
        result.Value.ClaimedRewards!.NumberClaimed.Should().Be(1);
        result.Value.ClaimedRewards.RewardName.Should().Be("Free Coffee");
        result.Value.ClaimedRewards.TotalPointsDeducted.Should().Be(5);
        result.Value.ClaimedRewards.RedemptionIds.Should().HaveCount(1);

        // Assert - Database: Balance updated
        var updatedBalance = await _fixture.Context.CustomerBalances
            .FirstAsync(ub => ub.CustomerId == user.Id && ub.RewardId == reward.Id);
        updatedBalance.Balance.Should().Be(6);

        // Assert - Database: Redemption created
        var redemptions = await _fixture.Context.RewardRedemptions
            .Where(r => r.CustomerId == user.Id && r.RewardId == reward.Id)
            .ToListAsync();
        redemptions.Should().HaveCount(1);
        redemptions[0].RedeemedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ProcessScan_ClaimMultipleRewards_DeductsCorrectPointsAndCreatesMultipleRedemptions()
    {
        // Arrange - Customer has 12 points, wants to claim 2 rewards (costs 5 points each)
        // Use unique QR code to avoid collision with real database
        var uniqueQr = $"TEST-MULTI-{Guid.NewGuid()}";
        var businessUser = await _testData.CreateRewardOwner();
        var user = await _testData.CreateUser("MultiClaimUser", uniqueQr);
        var reward = await _testData.CreateReward("Free Coffee", costPoints: 5, rewardOwner: businessUser);
        await _testData.CreateUserBalance(user, reward, balance: 12);

        var request = new ScanEventForCreationDto
        {
            QrCodeValue = user.QrCodeValue,
            RewardId = reward.Id,
            PointsChange = 0, // No new points
            NumRewardsToClaim = 2 // Claim 2 rewards
        };

        // Act
        var result = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId,
            businessUser.Id,
            request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentBalance.Should().Be(2, "because 12 - (5*2) = 2");
        result.Value.ClaimedRewards!.NumberClaimed.Should().Be(2);
        result.Value.ClaimedRewards.TotalPointsDeducted.Should().Be(10);
        result.Value.ClaimedRewards.RedemptionIds.Should().HaveCount(2);

        // Verify 2 redemptions created
        var redemptions = await _fixture.Context.RewardRedemptions
            .Where(r => r.CustomerId == user.Id && r.RewardId == reward.Id)
            .ToListAsync();
        redemptions.Should().HaveCount(2);
    }

    [Fact]
    public async Task ProcessScan_InsufficientPoints_ReturnsFailureAndDoesNotModifyDatabase()
    {
        // Arrange - Customer only has 3 points, but reward costs 5
        var (user, reward, balance, businessUser) = await _testData.CreateUserWithInsufficientPoints(
            currentPoints: 3,
            rewardCost: 5);

        var request = new ScanEventForCreationDto
        {
            QrCodeValue = user.QrCodeValue,
            RewardId = reward.Id,
            PointsChange = 1, // Add 1 point (total would be 4, still not enough)
            NumRewardsToClaim = 1 // Try to claim 1 reward
        };

        // Act
        var result = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId,
            businessUser.Id,
            request);

        // Assert - Returns failure
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Insufficient points");
        result.Error.Should().Contain("5"); // Required
        result.Error.Should().Contain("3"); // Available

        // Assert - Database unchanged (transaction rolled back)
        var unchangedBalance = await _fixture.Context.CustomerBalances
            .FirstAsync(ub => ub.CustomerId == user.Id && ub.RewardId == reward.Id);
        unchangedBalance.Balance.Should().Be(3); // Still 3, not updated

        // No scan event created
        var scanEvents = await _fixture.Context.ScanEvents
            .Where(se => se.CustomerId == user.Id && se.RewardId == reward.Id)
            .ToListAsync();
        scanEvents.Should().BeEmpty();

        // No redemptions created
        var redemptions = await _fixture.Context.RewardRedemptions
            .Where(r => r.CustomerId == user.Id && r.RewardId == reward.Id)
            .ToListAsync();
        redemptions.Should().BeEmpty();
    }

    #endregion

    #region ProcessScanAndRewardsAsync - Error Scenarios

    [Fact]
    public async Task ProcessScan_InvalidQrCode_ReturnsFailure()
    {
        // Arrange - QR code doesn't exist
        var businessUser = await _testData.CreateRewardOwner();
        var reward = await _testData.CreateReward(rewardOwner: businessUser);

        var request = new ScanEventForCreationDto
        {
            QrCodeValue = "INVALID-QR-CODE",
            RewardId = reward.Id,
            PointsChange = 1,
            NumRewardsToClaim = 0
        };

        // Act
        var result = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId,
            businessUser.Id,
            request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid QR code or reward"); // Generic for security
    }

    [Fact]
    public async Task ProcessScan_InvalidRewardId_ReturnsFailure()
    {
        // Arrange - Reward doesn't exist
        var businessUser = await _testData.CreateRewardOwner();
        var user = await _testData.CreateUser();

        var request = new ScanEventForCreationDto
        {
            QrCodeValue = user.QrCodeValue,
            RewardId = Guid.NewGuid(), // Non-existent reward
            PointsChange = 1,
            NumRewardsToClaim = 0
        };

        // Act
        var result = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId,
            businessUser.Id,
            request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid QR code or reward");
    }

    [Fact]
    public async Task ProcessScan_ClaimRewardWithoutBalance_ReturnsFailure()
    {
        // Arrange - Customer exists but has never scanned (no balance record)
        var businessUser = await _testData.CreateRewardOwner();
        var user = await _testData.CreateUser();
        var reward = await _testData.CreateReward(rewardOwner: businessUser);
        // No balance created!

        var request = new ScanEventForCreationDto
        {
            QrCodeValue = user.QrCodeValue,
            RewardId = reward.Id,
            PointsChange = 1,
            NumRewardsToClaim = 1 // Try to claim without any balance
        };

        // Act
        var result = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId,
            businessUser.Id,
            request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("don't have any points");
    }

    #endregion

    #region GetUserBalanceForRewardAsync Tests

    [Fact]
    public async Task GetUserBalance_ExistingBalance_ReturnsCorrectInfo()
    {
        // Arrange - Use a UNIQUE QR code that definitely doesn't exist in real database
        var uniqueQr = $"TEST-UNIQUE-{Guid.NewGuid()}";
        var businessUser = await _testData.CreateRewardOwner();
        var user = await _testData.CreateUser("TestOnlyUser", uniqueQr);
        var reward = await _testData.CreateReward("Free Coffee", costPoints: 5, rewardOwner: businessUser);
        var balance = await _testData.CreateUserBalance(user, reward, balance: 12);

        // VERIFY: Customer was created in in-memory database with correct name
        var verifyUser = await _fixture.Context.Customer.FirstAsync(u => u.Id == user.Id);
        verifyUser.Name.Should().Be("TestOnlyUser", "because we created a user in the test data");

        // Act
        var result = await _fixture.Service.GetCustomerBalanceForRewardAsync(
            businessUser.BusinessId,
            reward.Id,
            uniqueQr,
            Guid.Empty); // Auth is mocked in tests

        // Assert
        result.IsSuccess.Should().BeTrue("because the user exists in our in-memory test database");
        result.Value!.CustomerName.Should().Be("TestOnlyUser", 
            "because the user was created with name 'TestOnlyUser' in our in-memory test data");
        result.Value.QrCodeValue.Should().Be(uniqueQr);
        result.Value.CurrentBalance.Should().Be(12);
        result.Value.NumRewardsAvailable.Should().Be(2); // 12 / 5 = 2
        result.Value.AvailableReward.Should().NotBeNull();
        result.Value.AvailableReward!.RewardName.Should().Be("Free Coffee");
        result.Value.AvailableReward.RequiredPoints.Should().Be(5);
    }

    [Fact]
    public async Task GetUserBalance_NoBalance_ReturnsZeroPoints()
    {
        // Arrange - Customer exists but no scans yet
        var (user, reward, businessUser) = await _testData.CreateNewUserScenario();

        // Act
        var result = await _fixture.Service.GetCustomerBalanceForRewardAsync(
            businessUser.BusinessId,
            reward.Id,
            user.QrCodeValue,
            Guid.Empty); // Auth is mocked in tests

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentBalance.Should().Be(0);
        result.Value.NumRewardsAvailable.Should().Be(0);
        result.Value.AvailableReward.Should().BeNull();
    }

    [Fact]
    public async Task GetUserBalance_InvalidQrCode_ReturnsFailure()
    {
        // Arrange
        var businessUser = await _testData.CreateRewardOwner();
        var reward = await _testData.CreateReward(rewardOwner: businessUser);

        // Act
        var result = await _fixture.Service.GetCustomerBalanceForRewardAsync(
            businessUser.BusinessId,
            reward.Id,
            "INVALID-QR",
            Guid.Empty); // Auth is mocked in tests

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid QR code or reward");
    }

    #endregion

    #region GetScanEventForRewardAsync Tests

    [Fact]
    public async Task GetScanEvent_ExistingEvent_ReturnsDto()
    {
        // Arrange
        var businessUser = await _testData.CreateRewardOwner();
        var user = await _testData.CreateUser();
        var reward = await _testData.CreateReward(rewardOwner: businessUser);
        var scanEvent = await _testData.CreateScanEvent(user, reward, pointsChange: 1);

        // Act
        var result = await _fixture.Service.GetScanEventForRewardAsync(
            businessUser.BusinessId,
            reward.Id,
            scanEvent.Id,
            Guid.Empty); // Auth is mocked in tests

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(scanEvent.Id);
        result.Value.RewardId.Should().Be(reward.Id);
        result.Value.PointsChange.Should().Be(1);
    }

    [Fact]
    public async Task GetScanEvent_NonExistentEvent_ReturnsFailure()
    {
        // Arrange
        var businessUser = await _testData.CreateRewardOwner();
        var reward = await _testData.CreateReward(rewardOwner: businessUser);

        // Act
        var result = await _fixture.Service.GetScanEventForRewardAsync(
            businessUser.BusinessId,
            reward.Id,
            Guid.NewGuid(),
            Guid.Empty); // Auth is mocked in tests

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Scan event not found");
    }

    #endregion

    #region Complex Workflow Scenarios

    [Fact]
    public async Task CompleteUserJourney_MultipleScansAndClaims_WorksCorrectly()
    {
        // Arrange - Start with new user
        var (user, reward, businessUser) = await _testData.CreateNewUserScenario();

        // Act & Assert - Scan 1: Earn first point
        var scan1 = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId,
            businessUser.Id,
            new ScanEventForCreationDto
        {
            QrCodeValue = user.QrCodeValue,
            RewardId = reward.Id,
            PointsChange = 1,
            NumRewardsToClaim = 0
        });
        scan1.Value!.CurrentBalance.Should().Be(1);
        scan1.Value.RewardAvailable.Should().BeFalse();

        // Scan 2-5: Earn more points
        for (int i = 0; i < 4; i++)
        {
            await _fixture.Service.ProcessScanAndRewardsAsync(
                businessUser.BusinessId,
                businessUser.Id,
                new ScanEventForCreationDto
            {
                QrCodeValue = user.QrCodeValue,
                RewardId = reward.Id,
                PointsChange = 1,
                NumRewardsToClaim = 0
            });
        }

        // Scan 6: Earn point and claim reward
        var scan6 = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId,
            businessUser.Id,
            new ScanEventForCreationDto
        {
            QrCodeValue = user.QrCodeValue,
            RewardId = reward.Id,
            PointsChange = 1,
            NumRewardsToClaim = 1
        });
        scan6.Value!.CurrentBalance.Should().Be(1); // 6 - 5 = 1
        scan6.Value.ClaimedRewards.Should().NotBeNull();
        scan6.Value.ClaimedRewards!.NumberClaimed.Should().Be(1);

        // Verify database state
        var scanEvents = await _fixture.Context.ScanEvents
            .Where(se => se.CustomerId == user.Id)
            .ToListAsync();
        scanEvents.Should().HaveCount(6);

        var redemptions = await _fixture.Context.RewardRedemptions
            .Where(r => r.CustomerId == user.Id)
            .ToListAsync();
        redemptions.Should().HaveCount(1);

        var finalBalance = await _fixture.Context.CustomerBalances
            .FirstAsync(ub => ub.CustomerId == user.Id);
        finalBalance.Balance.Should().Be(1);
    }

    [Fact]
    public async Task MultipleUsers_SameReward_IndependentBalances()
    {
        // Arrange - Two users, same reward
        var businessUser = await _testData.CreateRewardOwner();
        var reward = await _testData.CreateReward("Free Coffee", 5, RewardType.IncrementalPoints, businessUser);
        var alice = await _testData.CreateUser("Alice", "QR001-ALICE");
        var bob = await _testData.CreateUser("Bob", "QR002-BOB");

        // Act - Alice earns 10 points
        for (int i = 0; i < 10; i++)
        {
            await _fixture.Service.ProcessScanAndRewardsAsync(
                businessUser.BusinessId,
                businessUser.Id,
                new ScanEventForCreationDto
            {
                QrCodeValue = alice.QrCodeValue,
                RewardId = reward.Id,
                PointsChange = 1
            });
        }

        // Bob earns 3 points
        for (int i = 0; i < 3; i++)
        {
            await _fixture.Service.ProcessScanAndRewardsAsync(
                businessUser.BusinessId,
                businessUser.Id,
                new ScanEventForCreationDto
            {
                QrCodeValue = bob.QrCodeValue,
                RewardId = reward.Id,
                PointsChange = 1
            });
        }

        // Assert - Check balances are independent
        var aliceBalance = await _fixture.Context.CustomerBalances
            .FirstAsync(ub => ub.CustomerId == alice.Id);
        aliceBalance.Balance.Should().Be(10);

        var bobBalance = await _fixture.Context.CustomerBalances
            .FirstAsync(ub => ub.CustomerId == bob.Id);
        bobBalance.Balance.Should().Be(3);

        // Alice can claim, Bob cannot
        var aliceResult = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId,
            businessUser.Id,
            new ScanEventForCreationDto
        {
            QrCodeValue = alice.QrCodeValue,
            RewardId = reward.Id,
            PointsChange = 0,
            NumRewardsToClaim = 2
        });
        aliceResult.IsSuccess.Should().BeTrue();

        var bobResult = await _fixture.Service.ProcessScanAndRewardsAsync(
            businessUser.BusinessId,
            businessUser.Id,
            new ScanEventForCreationDto
        {
            QrCodeValue = bob.QrCodeValue,
            RewardId = reward.Id,
            PointsChange = 0,
            NumRewardsToClaim = 1
        });
        bobResult.IsFailure.Should().BeTrue();
    }

    #endregion
}
