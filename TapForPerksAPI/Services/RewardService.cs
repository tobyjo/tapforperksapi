using AutoMapper;
using TapForPerksAPI.Common;
using TapForPerksAPI.Entities;
using TapForPerksAPI.Models;
using TapForPerksAPI.Repositories;

namespace TapForPerksAPI.Services;

public class RewardService : IRewardService
{
    private readonly ISaveForPerksRepository _repository;
    private readonly IMapper _mapper;

    public RewardService(ISaveForPerksRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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

        var (updatedBalance, scanEvent) = processResult.Value;

        // 3. Build response
        var response = BuildResponse(user, reward, updatedBalance, scanEvent);

        return Result<ScanEventResponseDto>.Success(response);
    }

    private async Task<Result<(User user, Reward reward, UserBalance? balance)>> 
        ValidateRequestAsync(ScanEventForCreationDto request)
    {
        // Validate user exists
        var user = await _repository.GetUserByQrCodeValueAsync(request.QrCodeValue);
        if (user == null)
            return Result<(User, Reward, UserBalance?)>.Failure("User not found");

        // Validate reward exists
        var reward = await _repository.GetRewardAsync(request.RewardId);
        if (reward == null)
            return Result<(User, Reward, UserBalance?)>.Failure("Reward not found");

        // Get existing balance
        var balance = await _repository.GetUserBalanceForRewardAsync(user.Id, reward.Id);

        // Validate reward claiming
        if (request.NumRewardsToClaim > 0)
        {
            if (request.NumRewardsToClaim < 0 || request.NumRewardsToClaim > 100)
                return Result<(User, Reward, UserBalance?)>.Failure(
                    "Number of rewards to claim must be between 0 and 100");

            if (balance == null)
                return Result<(User, Reward, UserBalance?)>.Failure(
                    "Cannot claim rewards - no points balance exists");

            var currentBalance = balance.Balance;
            var requiredPoints = reward.CostPoints * request.NumRewardsToClaim;
            
            if (currentBalance < requiredPoints)
                return Result<(User, Reward, UserBalance?)>.Failure(
                    $"Insufficient points. Required: {requiredPoints}, Available: {currentBalance}");
        }

        return Result<(User, Reward, UserBalance?)>.Success((user, reward, balance));
    }

    private async Task<Result<(UserBalance balance, ScanEvent scanEvent)>> 
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
            if (request.NumRewardsToClaim > 0)
            {
                await ClaimRewardsAsync(user, reward, balance, request.NumRewardsToClaim);
            }

            // 3. Create scan event
            var scanEvent = await CreateScanEventAsync(user, reward, request);

            // 4. Save all changes
            await _repository.SaveChangesAsync();

            return Result<(UserBalance, ScanEvent)>.Success((balance, scanEvent));
        }
        catch (Exception ex)
        {
            return Result<(UserBalance, ScanEvent)>.Failure($"Transaction failed: {ex.Message}");
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

    private async Task ClaimRewardsAsync(User user, Reward reward, UserBalance balance, int count)
    {
        var totalCost = reward.CostPoints * count;
        balance.Balance -= totalCost;
        balance.LastUpdated = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
        {
            var redemption = new RewardRedemption
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RewardId = reward.Id,
                RewardOwnerUserId = null, // Can be set if you track who processed the redemption
                RedeemedAt = DateTime.UtcNow
            };
            await _repository.CreateRewardRedemption(redemption);
        }
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
        ScanEvent scanEvent)
    {
        var response = new ScanEventResponseDto
        {
            ScanEvent = _mapper.Map<ScanEventDto>(scanEvent),
            UserName = user.Name,
            CurrentBalance = balance.Balance,
            RewardAvailable = false,
            AvailableReward = null,
            TimesClaimable = 0
        };

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
}
