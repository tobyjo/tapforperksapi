using AutoMapper;
using Microsoft.Extensions.Logging;
using TapForPerksAPI.Common;
using TapForPerksAPI.Entities;
using TapForPerksAPI.Models;
using TapForPerksAPI.Repositories;

namespace TapForPerksAPI.Services;

public class RewardService : IRewardService
{
    private readonly ISaveForPerksRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<RewardService> _logger;

    public RewardService(
        ISaveForPerksRepository repository, 
        IMapper mapper,
        ILogger<RewardService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ScanEventResponseDto>> ProcessScanAndRewardsAsync(
        ScanEventForCreationDto request)
    {
        // 1. Validate request
        var validationResult = await ValidateRequestAsync(request);
        if (validationResult.IsFailure)
            return Result<ScanEventResponseDto>.Failure(validationResult.Error!);

        var (user, reward, existingBalance) = validationResult.Value;

        // 2. Process transaction
        var processResult = await ProcessTransactionAsync(user, reward, existingBalance, request);
        if (processResult.IsFailure)
            return Result<ScanEventResponseDto>.Failure(processResult.Error!);

        var (updatedBalance, scanEvent, redemptionIds) = processResult.Value;

        // 3. Build response
        var response = BuildResponse(user, reward, updatedBalance, scanEvent, request.NumRewardsToClaim, redemptionIds);

        return Result<ScanEventResponseDto>.Success(response);
    }

    private async Task<Result<(User user, Reward reward, UserBalance? balance)>> 
        ValidateRequestAsync(ScanEventForCreationDto request)
    {
        // Validate user exists
        var user = await _repository.GetUserByQrCodeValueAsync(request.QrCodeValue);
        if (user == null)
        {
            _logger.LogWarning(
                "User not found. QrCode: {QrCode}, RewardId: {RewardId}", 
                request.QrCodeValue, request.RewardId);
            return Result<(User, Reward, UserBalance?)>.Failure(
                "Invalid QR code or reward");  // Generic - don't reveal which one failed
        }

        // Validate reward exists
        var reward = await _repository.GetRewardAsync(request.RewardId);
        if (reward == null)
        {
            _logger.LogWarning(
                "Reward not found. RewardId: {RewardId}, UserId: {UserId}, QrCode: {QrCode}", 
                request.RewardId, user.Id, request.QrCodeValue);
            return Result<(User, Reward, UserBalance?)>.Failure(
                "Invalid QR code or reward");  // Generic - don't reveal which one failed
        }

        // Get existing balance
        var balance = await _repository.GetUserBalanceForRewardAsync(user.Id, reward.Id);

        // Validate reward claiming
        if (request.NumRewardsToClaim > 0)
        {
            if (request.NumRewardsToClaim < 0 || request.NumRewardsToClaim > 100)
            {
                _logger.LogWarning(
                    "Invalid claim count. User: {UserId} ({UserName}), Requested: {Count}", 
                    user.Id, user.Name, request.NumRewardsToClaim);
                return Result<(User, Reward, UserBalance?)>.Failure(
                    "Number of rewards to claim must be between 0 and 100");  // OK - validation error
            }

            if (balance == null)
            {
                _logger.LogInformation(
                    "No balance for claim attempt. UserId: {UserId} ({UserName}), RewardId: {RewardId} ({RewardName})", 
                    user.Id, user.Name, reward.Id, reward.Name);
                return Result<(User, Reward, UserBalance?)>.Failure(
                    "You don't have any points for this reward yet");  // User-friendly, not revealing IDs
            }

            var currentBalance = balance.Balance;
            var requiredPoints = reward.CostPoints * request.NumRewardsToClaim;
            
            if (currentBalance < requiredPoints)
            {
                _logger.LogInformation(
                    "Insufficient points. User: {UserId} ({UserName}), Required: {Required}, Available: {Available}, Reward: {RewardName}", 
                    user.Id, user.Name, requiredPoints, currentBalance, reward.Name);
                return Result<(User, Reward, UserBalance?)>.Failure(
                    $"Insufficient points. Required: {requiredPoints}, Available: {currentBalance}");  // OK - user's own data
            }
        }

        return Result<(User, Reward, UserBalance?)>.Success((user, reward, balance));
    }

    private async Task<Result<(UserBalance balance, ScanEvent scanEvent, List<Guid>? redemptionIds)>> 
        ProcessTransactionAsync(
            User user, 
            Reward reward, 
            UserBalance? existingBalance,
            ScanEventForCreationDto request)
    {
        try
        {
            // 1. Add points to balance
            var balance = await AddPointsAsync(user, reward, existingBalance, request.PointsChange);

            // 2. Claim rewards if requested (deduct points and create redemptions)
            List<Guid>? redemptionIds = null;
            if (request.NumRewardsToClaim > 0)
            {
                redemptionIds = await ClaimRewardsAsync(user, reward, balance, request.NumRewardsToClaim);
                _logger.LogInformation(
                    "Rewards claimed. User: {UserId} ({UserName}), Reward: {RewardName}, Count: {Count}, PointsDeducted: {Points}", 
                    user.Id, user.Name, reward.Name, request.NumRewardsToClaim, reward.CostPoints * request.NumRewardsToClaim);
            }

            // 3. Create scan event
            var scanEvent = await CreateScanEventAsync(user, reward, request);
            _logger.LogInformation(
                "Scan event created. ScanEventId: {ScanEventId}, User: {UserId}, Reward: {RewardId}, Points: +{Points}", 
                scanEvent.Id, user.Id, reward.Id, request.PointsChange);

            // 4. Save all changes
            await _repository.SaveChangesAsync();

            return Result<(UserBalance, ScanEvent, List<Guid>?)>.Success((balance, scanEvent, redemptionIds));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Transaction failed. User: {UserId}, Reward: {RewardId}, Error: {Error}", 
                user.Id, reward.Id, ex.Message);
            return Result<(UserBalance, ScanEvent, List<Guid>?)>.Failure(
                "An error occurred while processing your request");  // Generic for user
        }
    }

    private async Task<UserBalance> AddPointsAsync(
        User user, 
        Reward reward, 
        UserBalance? existing, 
        int pointsToAdd)
    {
        if (existing == null)
        {
            var newBalance = new UserBalance
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RewardId = reward.Id,
                Balance = pointsToAdd,
                LastUpdated = DateTime.UtcNow
            };
            await _repository.CreateUserBalance(newBalance);
            return newBalance;
        }

        existing.Balance += pointsToAdd;
        existing.LastUpdated = DateTime.UtcNow;
        return existing;
    }

    private async Task<List<Guid>> ClaimRewardsAsync(User user, Reward reward, UserBalance balance, int count)
    {
        var totalCost = reward.CostPoints * count;
        balance.Balance -= totalCost;
        balance.LastUpdated = DateTime.UtcNow;

        var redemptionIds = new List<Guid>();

        for (int i = 0; i < count; i++)
        {
            var redemptionId = Guid.NewGuid();
            var redemption = new RewardRedemption
            {
                Id = redemptionId,
                UserId = user.Id,
                RewardId = reward.Id,
                RewardOwnerUserId = null, // Can be set if you track who processed the redemption
                RedeemedAt = DateTime.UtcNow
            };
            await _repository.CreateRewardRedemption(redemption);
            redemptionIds.Add(redemptionId);
        }

        return redemptionIds;
    }

    private async Task<ScanEvent> CreateScanEventAsync(User user, Reward reward, ScanEventForCreationDto request)
    {
        var scanEvent = new ScanEvent
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RewardId = reward.Id,
            QrCodeValue = request.QrCodeValue,
            PointsChange = request.PointsChange,
            RewardOwnerUserId = request.RewardOwnerUserId,
            ScannedAt = DateTime.UtcNow
        };
        await _repository.CreateScanEvent(scanEvent);
        return scanEvent;
    }

    private ScanEventResponseDto BuildResponse(
        User user, 
        Reward reward, 
        UserBalance balance,
        ScanEvent scanEvent,
        int numRewardsClaimed,
        List<Guid>? redemptionIds)
    {
        var response = new ScanEventResponseDto
        {
            ScanEvent = _mapper.Map<ScanEventDto>(scanEvent),
            UserName = user.Name,
            CurrentBalance = balance.Balance,
            RewardAvailable = false,
            AvailableReward = null,
            TimesClaimable = 0,
            ClaimedRewards = null
        };

        // Add claimed rewards info if any were claimed
        if (numRewardsClaimed > 0 && redemptionIds != null)
        {
            response.ClaimedRewards = new ClaimedRewardsDto
            {
                NumberClaimed = numRewardsClaimed,
                RewardName = reward.Name,
                TotalPointsDeducted = reward.CostPoints * numRewardsClaimed,
                RedemptionIds = redemptionIds
            };
        }

        // Check if rewards are still available after this transaction
        if (reward.RewardType == RewardType.IncrementalPoints && 
            reward.CostPoints > 0 && 
            balance.Balance >= reward.CostPoints)
        {
            response.RewardAvailable = true;
            response.TimesClaimable = balance.Balance / reward.CostPoints;
            response.AvailableReward = new AvailableRewardDto
            {
                RewardId = reward.Id,
                RewardName = reward.Name,
                RewardType = "incremental_points",
                RequiredPoints = reward.CostPoints
            };
        }

        return response;
    }

    public async Task<Result<UserBalanceAndInfoResponseDto>> GetUserBalanceForRewardAsync(
        Guid rewardId, 
        string qrCodeValue)
    {
        // 1. Validate user and reward exist
        var validationResult = await ValidateUserAndRewardAsync(rewardId, qrCodeValue);
        if (validationResult.IsFailure)
            return Result<UserBalanceAndInfoResponseDto>.Failure(validationResult.Error!);

        var (user, reward) = validationResult.Value;

        // 2. Get user balance
        var balance = await _repository.GetUserBalanceForRewardAsync(user.Id, rewardId);

        // 3. Build response
        var response = BuildUserBalanceResponse(user, reward, balance, qrCodeValue);

        return Result<UserBalanceAndInfoResponseDto>.Success(response);
    }

    private async Task<Result<(User user, Reward reward)>> ValidateUserAndRewardAsync(
        Guid rewardId, 
        string qrCodeValue)
    {
        var user = await _repository.GetUserByQrCodeValueAsync(qrCodeValue);
        if (user == null)
        {
            _logger.LogWarning(
                "User balance check: User not found. QrCode: {QrCode}, RewardId: {RewardId}", 
                qrCodeValue, rewardId);
            return Result<(User, Reward)>.Failure(
                "Invalid QR code or reward");  // Generic
        }

        var reward = await _repository.GetRewardAsync(rewardId);
        if (reward == null)
        {
            _logger.LogWarning(
                "User balance check: Reward not found. RewardId: {RewardId}, UserId: {UserId}, QrCode: {QrCode}", 
                rewardId, user.Id, qrCodeValue);
            return Result<(User, Reward)>.Failure(
                "Invalid QR code or reward");  // Generic
        }

        return Result<(User, Reward)>.Success((user, reward));
    }

    private UserBalanceAndInfoResponseDto BuildUserBalanceResponse(
        User user, 
        Reward reward, 
        UserBalance? balance,
        string qrCodeValue)
    {
        var response = new UserBalanceAndInfoResponseDto
        {
            QrCodeValue = qrCodeValue,
            UserName = user.Name,
            CurrentBalance = balance?.Balance ?? 0,
            AvailableReward = null,
            TimesClaimable = 0
        };

        // If no balance exists, return empty response
        if (balance == null)
            return response;

        // Check if reward is available based on reward type
        if (reward.RewardType == RewardType.IncrementalPoints && 
            reward.CostPoints > 0 && 
            balance.Balance >= reward.CostPoints)
        {
            response.TimesClaimable = balance.Balance / reward.CostPoints;
            response.AvailableReward = new AvailableRewardDto
            {
                RewardId = reward.Id,
                RewardName = reward.Name,
                RewardType = "incremental_points",
                RequiredPoints = reward.CostPoints
            };
        }

        return response;
    }

    public async Task<Result<ScanEventDto>> GetScanEventForRewardAsync(
        Guid rewardId, 
        Guid scanEventId)
    {
        var scanEvent = await _repository.GetScanEventAsync(rewardId, scanEventId);
        
        if (scanEvent == null)
        {
            _logger.LogWarning(
                "Scan event not found. ScanEventId: {ScanEventId}, RewardId: {RewardId}", 
                scanEventId, rewardId);
            return Result<ScanEventDto>.Failure(
                "Scan event not found");  // Generic - don't reveal IDs
        }

        var scanEventDto = _mapper.Map<ScanEventDto>(scanEvent);
        return Result<ScanEventDto>.Success(scanEventDto);
    }
}


