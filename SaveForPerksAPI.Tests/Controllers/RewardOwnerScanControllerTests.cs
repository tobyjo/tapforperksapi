using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SaveForPerksAPI.Common;
using SaveForPerksAPI.Controllers.RewardOwner;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Services;
using Xunit;

namespace SaveForPerksAPI.Tests.Controllers;

/// <summary>
/// Unit tests for RewardOwnerScanController.
/// These tests mock the service layer to test controller routing and HTTP response handling.
/// For testing actual business logic, see Integration/Services/RewardServiceIntegrationTests.cs
/// </summary>
public class RewardOwnerScanControllerTests
{
    private readonly Mock<IRewardTransactionService> _mockRewardService;
    private readonly Mock<ILogger<RewardOwnerScanController>> _mockLogger;
    private readonly RewardOwnerScanController _controller;

    public RewardOwnerScanControllerTests()
    {
        _mockRewardService = new Mock<IRewardTransactionService>();
        _mockLogger = new Mock<ILogger<RewardOwnerScanController>>();
        _controller = new RewardOwnerScanController(_mockRewardService.Object, _mockLogger.Object);
    }

    #region GetScanEventForReward Tests

    [Fact]
    public async Task GetScanEventForReward_WhenEventExists_ReturnsOkWithDto()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var scanEventId = Guid.NewGuid();
        var expectedDto = new ScanEventDto 
        { 
            Id = scanEventId, 
            RewardId = rewardId,
            PointsChange = 1
        };
        
        _mockRewardService
            .Setup(s => s.GetScanEventForRewardAsync(rewardId, scanEventId))
            .ReturnsAsync(Result<ScanEventDto>.Success(expectedDto));

        // Act
        var result = await _controller.GetScanEventForReward(rewardId, scanEventId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDto = okResult.Value.Should().BeOfType<ScanEventDto>().Subject;
        returnedDto.Id.Should().Be(scanEventId);
        returnedDto.RewardId.Should().Be(rewardId);
    }

    [Fact]
    public async Task GetScanEventForReward_WhenEventNotFound_ReturnsNotFound()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var scanEventId = Guid.NewGuid();
        
        _mockRewardService
            .Setup(s => s.GetScanEventForRewardAsync(rewardId, scanEventId))
            .ReturnsAsync(Result<ScanEventDto>.Failure("Scan event not found"));

        // Act
        var result = await _controller.GetScanEventForReward(rewardId, scanEventId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Scan event not found");
    }

    #endregion

    #region GetUserBalanceForReward Tests

    [Fact]
    public async Task GetUserBalanceForReward_WhenUserExists_ReturnsOkWithBalance()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var qrCodeValue = "QR001-TEST";
        var expectedResponse = new UserBalanceAndInfoResponseDto
        {
            QrCodeValue = qrCodeValue,
            UserName = "Test User",
            CurrentBalance = 5,
            NumRewardsAvailable = 1
        };
        
        _mockRewardService
            .Setup(s => s.GetUserBalanceForRewardAsync(rewardId, qrCodeValue))
            .ReturnsAsync(Result<UserBalanceAndInfoResponseDto>.Success(expectedResponse));

        // Act
        var result = await _controller.GetUserBalanceForReward(rewardId, qrCodeValue);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResponse = okResult.Value.Should().BeOfType<UserBalanceAndInfoResponseDto>().Subject;
        returnedResponse.CurrentBalance.Should().Be(5);
        returnedResponse.UserName.Should().Be("Test User");
    }

    [Fact]
    public async Task GetUserBalanceForReward_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var qrCodeValue = "INVALID-QR";
        
        _mockRewardService
            .Setup(s => s.GetUserBalanceForRewardAsync(rewardId, qrCodeValue))
            .ReturnsAsync(Result<UserBalanceAndInfoResponseDto>.Failure("User not found"));

        // Act
        var result = await _controller.GetUserBalanceForReward(rewardId, qrCodeValue);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("User not found");
    }

    #endregion

    #region CreatePointsAndClaimRewards Tests

    [Fact]
    public async Task CreatePointsAndClaimRewards_WhenValid_ReturnsCreatedAtRoute()
    {
        // Arrange
        var request = new ScanEventForCreationDto
        {
            QrCodeValue = "QR001",
            RewardId = Guid.NewGuid(),
            PointsChange = 1,
            NumRewardsToClaim = 0
        };
        
        var scanEventId = Guid.NewGuid();
        var response = new ScanEventResponseDto
        {
            ScanEvent = new ScanEventDto 
            { 
                Id = scanEventId, 
                RewardId = request.RewardId 
            },
            UserName = "Test User",
            CurrentBalance = 5,
            RewardAvailable = true,
            NumRewardsAvailable = 1
        };
        
        _mockRewardService
            .Setup(s => s.ProcessScanAndRewardsAsync(request))
            .ReturnsAsync(Result<ScanEventResponseDto>.Success(response));

        // Act
        var result = await _controller.CreatePointsAndClaimRewards(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        createdResult.RouteName.Should().Be("GetScanEventForReward");
        createdResult.RouteValues.Should().ContainKey("rewardId");
        createdResult.RouteValues.Should().ContainKey("scanEventId");
        
        var returnedResponse = createdResult.Value.Should().BeOfType<ScanEventResponseDto>().Subject;
        returnedResponse.CurrentBalance.Should().Be(5);
        returnedResponse.UserName.Should().Be("Test User");
    }

    [Fact]
    public async Task CreatePointsAndClaimRewards_WithRewardsClaimed_ReturnsSuccessWithClaimedInfo()
    {
        // Arrange
        var request = new ScanEventForCreationDto
        {
            QrCodeValue = "QR001",
            RewardId = Guid.NewGuid(),
            PointsChange = 1,
            NumRewardsToClaim = 2
        };
        
        var response = new ScanEventResponseDto
        {
            ScanEvent = new ScanEventDto 
            { 
                Id = Guid.NewGuid(), 
                RewardId = request.RewardId 
            },
            UserName = "Test User",
            CurrentBalance = 2,
            ClaimedRewards = new ClaimedRewardsDto
            {
                NumberClaimed = 2,
                RewardName = "Free Coffee",
                TotalPointsDeducted = 10,
                RedemptionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            }
        };
        
        _mockRewardService
            .Setup(s => s.ProcessScanAndRewardsAsync(request))
            .ReturnsAsync(Result<ScanEventResponseDto>.Success(response));

        // Act
        var result = await _controller.CreatePointsAndClaimRewards(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        var returnedResponse = createdResult.Value.Should().BeOfType<ScanEventResponseDto>().Subject;
        
        returnedResponse.ClaimedRewards.Should().NotBeNull();
        returnedResponse.ClaimedRewards!.NumberClaimed.Should().Be(2);
        returnedResponse.ClaimedRewards.RewardName.Should().Be("Free Coffee");
        returnedResponse.ClaimedRewards.TotalPointsDeducted.Should().Be(10);
        returnedResponse.ClaimedRewards.RedemptionIds.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreatePointsAndClaimRewards_WhenServiceFails_ReturnsBadRequest()
    {
        // Arrange
        var request = new ScanEventForCreationDto
        {
            QrCodeValue = "INVALID",
            RewardId = Guid.NewGuid(),
            PointsChange = 1,
            NumRewardsToClaim = 0
        };
        
        _mockRewardService
            .Setup(s => s.ProcessScanAndRewardsAsync(request))
            .ReturnsAsync(Result<ScanEventResponseDto>.Failure("User not found"));

        // Act
        var result = await _controller.CreatePointsAndClaimRewards(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("User not found");
    }

    [Fact]
    public async Task CreatePointsAndClaimRewards_WithInsufficientPoints_ReturnsBadRequest()
    {
        // Arrange
        var request = new ScanEventForCreationDto
        {
            QrCodeValue = "QR001",
            RewardId = Guid.NewGuid(),
            PointsChange = 1,
            NumRewardsToClaim = 5  // Trying to claim more than available
        };
        
        _mockRewardService
            .Setup(s => s.ProcessScanAndRewardsAsync(request))
            .ReturnsAsync(Result<ScanEventResponseDto>.Failure("Insufficient points. Required: 25, Available: 10"));

        // Act
        var result = await _controller.CreatePointsAndClaimRewards(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeOfType<string>().Subject.Should().Contain("Insufficient points");
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRewardService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new RewardOwnerScanController(null!, _mockLogger.Object));
        
        exception.ParamName.Should().Be("rewardTransactionService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new RewardOwnerScanController(_mockRewardService.Object, null!));
        
        exception.ParamName.Should().Be("logger");
    }

    #endregion
}
