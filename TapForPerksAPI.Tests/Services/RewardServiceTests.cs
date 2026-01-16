using AutoMapper;
using FluentAssertions;
using Moq;
using TapForPerksAPI.Common;
using TapForPerksAPI.Entities;
using TapForPerksAPI.Models;
using TapForPerksAPI.Repositories;
using TapForPerksAPI.Services;
using Xunit;

namespace TapForPerksAPI.Tests.Services;

public class RewardServiceTests
{
    private readonly Mock<ISaveForPerksRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly RewardService _service;

    public RewardServiceTests()
    {
        _mockRepository = new Mock<ISaveForPerksRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new RewardService(_mockRepository.Object, _mockMapper.Object);
    }

    #region GetScanEventForRewardAsync Tests

    [Fact]
    public async Task GetScanEventForRewardAsync_WhenEventExists_ReturnsSuccess()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var scanEventId = Guid.NewGuid();
        var scanEvent = new ScanEvent
        {
            Id = scanEventId,
            RewardId = rewardId,
            PointsChange = 1
        };
        var scanEventDto = new ScanEventDto
        {
            Id = scanEventId,
            RewardId = rewardId,
            PointsChange = 1
        };

        _mockRepository
            .Setup(r => r.GetScanEventAsync(rewardId, scanEventId))
            .ReturnsAsync(scanEvent);
        
        _mockMapper
            .Setup(m => m.Map<ScanEventDto>(scanEvent))
            .Returns(scanEventDto);

        // Act
        var result = await _service.GetScanEventForRewardAsync(rewardId, scanEventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(scanEventId);
    }

    [Fact]
    public async Task GetScanEventForRewardAsync_WhenEventNotFound_ReturnsFailure()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var scanEventId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetScanEventAsync(rewardId, scanEventId))
            .ReturnsAsync((ScanEvent?)null);

        // Act
        var result = await _service.GetScanEventForRewardAsync(rewardId, scanEventId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Scan event not found");
    }

    #endregion

    #region GetUserBalanceForRewardAsync Tests

    [Fact]
    public async Task GetUserBalanceForRewardAsync_WhenUserAndRewardExist_ReturnsBalance()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var qrCodeValue = "QR001";
        var user = new User { Id = Guid.NewGuid(), Name = "Test User", QrCodeValue = qrCodeValue };
        var reward = new Reward { Id = rewardId, Name = "Free Coffee", CostPoints = 5, RewardType = RewardType.IncrementalPoints };
        var balance = new UserBalance { Balance = 7 };

        _mockRepository.Setup(r => r.GetUserByQrCodeValueAsync(qrCodeValue)).ReturnsAsync(user);
        _mockRepository.Setup(r => r.GetRewardAsync(rewardId)).ReturnsAsync(reward);
        _mockRepository.Setup(r => r.GetUserBalanceForRewardAsync(user.Id, rewardId)).ReturnsAsync(balance);

        // Act
        var result = await _service.GetUserBalanceForRewardAsync(rewardId, qrCodeValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentBalance.Should().Be(7);
        result.Value.UserName.Should().Be("Test User");
        result.Value.TimesClaimable.Should().Be(1); // 7/5 = 1
    }

    [Fact]
    public async Task GetUserBalanceForRewardAsync_WhenUserNotFound_ReturnsFailure()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var qrCodeValue = "INVALID";

        _mockRepository.Setup(r => r.GetUserByQrCodeValueAsync(qrCodeValue)).ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserBalanceForRewardAsync(rewardId, qrCodeValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User not found");
    }

    [Fact]
    public async Task GetUserBalanceForRewardAsync_WhenNoBalanceExists_ReturnsZeroBalance()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var qrCodeValue = "QR001";
        var user = new User { Id = Guid.NewGuid(), Name = "Test User", QrCodeValue = qrCodeValue };
        var reward = new Reward { Id = rewardId, Name = "Free Coffee", CostPoints = 5, RewardType = RewardType.IncrementalPoints };

        _mockRepository.Setup(r => r.GetUserByQrCodeValueAsync(qrCodeValue)).ReturnsAsync(user);
        _mockRepository.Setup(r => r.GetRewardAsync(rewardId)).ReturnsAsync(reward);
        _mockRepository.Setup(r => r.GetUserBalanceForRewardAsync(user.Id, rewardId)).ReturnsAsync((UserBalance?)null);

        // Act
        var result = await _service.GetUserBalanceForRewardAsync(rewardId, qrCodeValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentBalance.Should().Be(0);
        result.Value.TimesClaimable.Should().Be(0);
    }

    #endregion

    #region ProcessScanAndRewardsAsync Tests

    [Fact]
    public async Task ProcessScanAndRewardsAsync_WithValidRequest_CreatesBalanceAndScanEvent()
    {
        // Arrange
        var request = new ScanEventForCreationDto
        {
            QrCodeValue = "QR001",
            RewardId = Guid.NewGuid(),
            PointsChange = 1,
            NumRewardsToClaim = 0
        };

        var user = new User { Id = Guid.NewGuid(), Name = "Test User", QrCodeValue = request.QrCodeValue };
        var reward = new Reward 
        { 
            Id = request.RewardId, 
            Name = "Free Coffee", 
            CostPoints = 5, 
            RewardType = RewardType.IncrementalPoints 
        };
        
        var scanEventDto = new ScanEventDto { Id = Guid.NewGuid(), RewardId = request.RewardId };

        _mockRepository.Setup(r => r.GetUserByQrCodeValueAsync(request.QrCodeValue)).ReturnsAsync(user);
        _mockRepository.Setup(r => r.GetRewardAsync(request.RewardId)).ReturnsAsync(reward);
        _mockRepository.Setup(r => r.GetUserBalanceForRewardAsync(user.Id, reward.Id)).ReturnsAsync((UserBalance?)null);
        _mockRepository.Setup(r => r.CreateUserBalance(It.IsAny<UserBalance>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.CreateScanEvent(It.IsAny<ScanEvent>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        
        _mockMapper.Setup(m => m.Map<ScanEventDto>(It.IsAny<ScanEvent>())).Returns(scanEventDto);

        // Act
        var result = await _service.ProcessScanAndRewardsAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentBalance.Should().Be(1);
        result.Value.UserName.Should().Be("Test User");
        
        _mockRepository.Verify(r => r.CreateUserBalance(It.Is<UserBalance>(ub => ub.Balance == 1)), Times.Once);
        _mockRepository.Verify(r => r.CreateScanEvent(It.IsAny<ScanEvent>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessScanAndRewardsAsync_WithRewardClaim_DeductsPointsAndCreatesRedemptions()
    {
        // Arrange
        var request = new ScanEventForCreationDto
        {
            QrCodeValue = "QR001",
            RewardId = Guid.NewGuid(),
            PointsChange = 1,
            NumRewardsToClaim = 2
        };

        var user = new User { Id = Guid.NewGuid(), Name = "Test User", QrCodeValue = request.QrCodeValue };
        var reward = new Reward 
        { 
            Id = request.RewardId, 
            Name = "Free Coffee", 
            CostPoints = 5, 
            RewardType = RewardType.IncrementalPoints 
        };
        var existingBalance = new UserBalance { Balance = 10, UserId = user.Id, RewardId = reward.Id };
        
        var scanEventDto = new ScanEventDto { Id = Guid.NewGuid(), RewardId = request.RewardId };

        _mockRepository.Setup(r => r.GetUserByQrCodeValueAsync(request.QrCodeValue)).ReturnsAsync(user);
        _mockRepository.Setup(r => r.GetRewardAsync(request.RewardId)).ReturnsAsync(reward);
        _mockRepository.Setup(r => r.GetUserBalanceForRewardAsync(user.Id, reward.Id)).ReturnsAsync(existingBalance);
        _mockRepository.Setup(r => r.CreateRewardRedemption(It.IsAny<RewardRedemption>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.CreateScanEvent(It.IsAny<ScanEvent>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        
        _mockMapper.Setup(m => m.Map<ScanEventDto>(It.IsAny<ScanEvent>())).Returns(scanEventDto);

        // Act
        var result = await _service.ProcessScanAndRewardsAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentBalance.Should().Be(1); // 10 + 1 - (5*2) = 1
        result.Value.ClaimedRewards.Should().NotBeNull();
        result.Value.ClaimedRewards!.NumberClaimed.Should().Be(2);
        result.Value.ClaimedRewards.TotalPointsDeducted.Should().Be(10);
        
        _mockRepository.Verify(r => r.CreateRewardRedemption(It.IsAny<RewardRedemption>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessScanAndRewardsAsync_WithInsufficientPoints_ReturnsFailure()
    {
        // Arrange
        var request = new ScanEventForCreationDto
        {
            QrCodeValue = "QR001",
            RewardId = Guid.NewGuid(),
            PointsChange = 1,
            NumRewardsToClaim = 3  // Needs 15 points
        };

        var user = new User { Id = Guid.NewGuid(), Name = "Test User", QrCodeValue = request.QrCodeValue };
        var reward = new Reward 
        { 
            Id = request.RewardId, 
            Name = "Free Coffee", 
            CostPoints = 5, 
            RewardType = RewardType.IncrementalPoints 
        };
        var existingBalance = new UserBalance { Balance = 10 };  // Only 10 points

        _mockRepository.Setup(r => r.GetUserByQrCodeValueAsync(request.QrCodeValue)).ReturnsAsync(user);
        _mockRepository.Setup(r => r.GetRewardAsync(request.RewardId)).ReturnsAsync(reward);
        _mockRepository.Setup(r => r.GetUserBalanceForRewardAsync(user.Id, reward.Id)).ReturnsAsync(existingBalance);

        // Act
        var result = await _service.ProcessScanAndRewardsAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Insufficient points");
        
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    #endregion
}
