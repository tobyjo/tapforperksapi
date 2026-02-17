using AutoMapper;
using Microsoft.Extensions.Logging;
using SaveForPerksAPI.Common;
using SaveForPerksAPI.Entities;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Repositories;

namespace SaveForPerksAPI.Services;

public class RewardTransactionService : IRewardTransactionService
{
    private readonly ISaveForPerksRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<RewardTransactionService> _logger;
    private readonly IAuthorizationService _authorizationService;

    public RewardTransactionService(
        ISaveForPerksRepository repository, 
        IMapper mapper,
        ILogger<RewardTransactionService> logger,
        IAuthorizationService authorizationService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public async Task<Result<ScanEventResponseDto>> ProcessScanAndRewardsAsync(
        Guid businessId,
        Guid businessUserId,
        ScanEventForCreationDto request)
    {
        // 1. Validate JWT token matches business user's auth provider ID
        var jwtAuthCheck = await _authorizationService.ValidateBusinessUserAuthorizationAsync(businessUserId);
        if (jwtAuthCheck.IsFailure)
            return Result<ScanEventResponseDto>.Failure(jwtAuthCheck.Error!);

        // 2. Validate BusinessUser belongs to Business
        var authCheck = await ValidateBusinessUserAuthorizationAsync(businessId, businessUserId);
        if (authCheck.IsFailure)
            return Result<ScanEventResponseDto>.Failure(authCheck.Error!);

        // 3. Validate Reward belongs to Business
        var rewardCheck = await ValidateRewardBelongsToBusinessAsync(businessId, request.RewardId);
        if (rewardCheck.IsFailure)
            return Result<ScanEventResponseDto>.Failure(rewardCheck.Error!);

        // 4. Validate request
        var validationResult = await ValidateRequestAsync(request);
        if (validationResult.IsFailure)
            return Result<ScanEventResponseDto>.Failure(validationResult.Error!);

        var (customer, reward, existingBalance) = validationResult.Value;

        // 5. Process transaction
        var processResult = await ProcessTransactionAsync(customer, reward, existingBalance, businessUserId, request);
        if (processResult.IsFailure)
            return Result<ScanEventResponseDto>.Failure(processResult.Error!);

        var (updatedBalance, scanEvent, redemptionIds) = processResult.Value;

        // 6. Build response
        var response = BuildResponse(customer, reward, updatedBalance, scanEvent, request.NumRewardsToClaim, redemptionIds);

        return Result<ScanEventResponseDto>.Success(response);
    }

    private async Task<Result<bool>> ValidateBusinessUserAuthorizationAsync(Guid businessId, Guid businessUserId)
    {
        var businessUser = await _repository.GetBusinessUserByIdAsync(businessUserId);
        if (businessUser == null)
        {
            _logger.LogWarning(
                "Authorization failed: BusinessUser not found. BusinessUserId: {BusinessUserId}",
                businessUserId);
            return Result<bool>.Failure("You are not authorized to perform this action");
        }

        if (businessUser.BusinessId != businessId)
        {
            _logger.LogWarning(
                "Authorization failed: BusinessUser {BusinessUserId} does not belong to Business {BusinessId}. Actual BusinessId: {ActualBusinessId}",
                businessUserId, businessId, businessUser.BusinessId);
            return Result<bool>.Failure("You are not authorized to perform scans for this business");
        }

        _logger.LogInformation(
            "Authorization successful: BusinessUser {BusinessUserId} belongs to Business {BusinessId}",
            businessUserId, businessId);

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> ValidateRewardBelongsToBusinessAsync(Guid businessId, Guid rewardId)
    {
        var reward = await _repository.GetRewardAsync(rewardId);
        if (reward == null)
        {
            _logger.LogWarning(
                "Reward not found. RewardId: {RewardId}",
                rewardId);
            return Result<bool>.Failure("Invalid reward");
        }

        if (reward.BusinessId != businessId)
        {
            _logger.LogWarning(
                "Authorization failed: Reward {RewardId} does not belong to Business {BusinessId}. Actual BusinessId: {ActualBusinessId}",
                rewardId, businessId, reward.BusinessId);
            return Result<bool>.Failure("This reward does not belong to your business");
        }

        _logger.LogInformation(
            "Reward validation successful: Reward {RewardId} belongs to Business {BusinessId}",
            rewardId, businessId);

        return Result<bool>.Success(true);
    }

    private async Task<Result<(Customer customer, Reward reward, CustomerBalance? balance)>> 
        ValidateRequestAsync(ScanEventForCreationDto request)
    {
        // Validate customer exists
        var customer = await _repository.GetCustomerByQrCodeValueAsync(request.QrCodeValue);
        if (customer == null)
        {
            _logger.LogWarning(
                "Customer not found. QrCode: {QrCode}, RewardId: {RewardId}", 
                request.QrCodeValue, request.RewardId);
            return Result<(Customer, Reward, CustomerBalance?)>.Failure(
                "Invalid QR code or reward");  // Generic - don't reveal which one failed
        }

        // Validate reward exists
        var reward = await _repository.GetRewardAsync(request.RewardId);
        if (reward == null)
        {
            _logger.LogWarning(
                "Reward not found. RewardId: {RewardId}, CustomerId: {CustomerId}, QrCode: {QrCode}", 
                request.RewardId, customer.Id, request.QrCodeValue);
            return Result<(Customer, Reward, CustomerBalance?)>.Failure(
                "Invalid QR code or reward");  // Generic - don't reveal which one failed
        }

        // Get existing balance
        var balance = await _repository.GetCustomerBalanceForRewardAsync(customer.Id, reward.Id);

        // Validate reward claiming
        if (request.NumRewardsToClaim > 0)
        {
            if (request.NumRewardsToClaim < 0 || request.NumRewardsToClaim > 100)
            {
                _logger.LogWarning(
                    "Invalid claim count. Customer: {CustomerId} ({CustomerName}), Requested: {Count}", 
                    customer.Id, customer.Name, request.NumRewardsToClaim);
                return Result<(Customer, Reward, CustomerBalance?)>.Failure(
                    "Number of rewards to claim must be between 0 and 100");  // OK - validation error
            }

            if (balance == null)
            {
                _logger.LogInformation(
                    "No balance for claim attempt. CustomerId: {CustomerId} ({CustomerName}), RewardId: {RewardId} ({RewardName})", 
                    customer.Id, customer.Name, reward.Id, reward.Name);
                return Result<(Customer, Reward, CustomerBalance?)>.Failure(
                    "You don't have any points for this reward yet");  // Customer-friendly, not revealing IDs
            }

            var currentBalance = balance.Balance;
            var requiredPoints = reward.CostPoints * request.NumRewardsToClaim;
            
            if (currentBalance < requiredPoints)
            {
                _logger.LogInformation(
                    "Insufficient points. Customer: {CustomerId} ({CustomerName}), Required: {Required}, Available: {Available}, Reward: {RewardName}", 
                    customer.Id, customer.Name, requiredPoints, currentBalance, reward.Name);
                return Result<(Customer, Reward, CustomerBalance?)>.Failure(
                    $"Insufficient points. Required: {requiredPoints}, Available: {currentBalance}");  // OK - customer's own data
            }
        }

        return Result<(Customer, Reward, CustomerBalance?)>.Success((customer, reward, balance));
    }

    private async Task<Result<(CustomerBalance balance, ScanEvent scanEvent, List<Guid>? redemptionIds)>> 
        ProcessTransactionAsync(
            Customer customer, 
            Reward reward, 
            CustomerBalance? existingBalance,
            Guid businessUserId,
            ScanEventForCreationDto request)
    {
        try
        {
            // 1. Add points to balance
            var balance = await AddPointsAsync(customer, reward, existingBalance, request.PointsChange);

            // 2. Claim rewards if requested (deduct points and create redemptions)
            List<Guid>? redemptionIds = null;
            if (request.NumRewardsToClaim > 0)
            {
                redemptionIds = await ClaimRewardsAsync(customer, reward, balance, request.NumRewardsToClaim);
                _logger.LogInformation(
                    "Rewards claimed. Customer: {CustomerId} ({CustomerName}), Reward: {RewardName}, Count: {Count}, PointsDeducted: {Points}", 
                    customer.Id, customer.Name, reward.Name, request.NumRewardsToClaim, reward.CostPoints * request.NumRewardsToClaim);
            }

            // 3. Create scan event
            var scanEvent = await CreateScanEventAsync(customer, reward, businessUserId, request);
            _logger.LogInformation(
                "Scan event created. ScanEventId: {ScanEventId}, Customer: {CustomerId}, Reward: {RewardId}, Points: +{Points}", 
                scanEvent.Id, customer.Id, reward.Id, request.PointsChange);

            // 4. Save all changes
            await _repository.SaveChangesAsync();

            return Result<(CustomerBalance, ScanEvent, List<Guid>?)>.Success((balance, scanEvent, redemptionIds));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Transaction failed. Customer: {CustomerId}, Reward: {RewardId}, Error: {Error}", 
                customer.Id, reward.Id, ex.Message);
            return Result<(CustomerBalance, ScanEvent, List<Guid>?)>.Failure(
                "An error occurred while processing your request");  // Generic for customer
        }
    }

    private async Task<CustomerBalance> AddPointsAsync(
        Customer customer, 
        Reward reward, 
        CustomerBalance? existing, 
        int pointsToAdd)
    {
        if (existing == null)
        {
            var newBalance = new CustomerBalance
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
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

    private async Task<List<Guid>> ClaimRewardsAsync(Customer customer, Reward reward, CustomerBalance balance, int count)
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
                CustomerId = customer.Id,
                RewardId = reward.Id,
                BusinessUserId = null, // Can be set if you track who processed the redemption
                RedeemedAt = DateTime.UtcNow
            };
            await _repository.CreateRewardRedemption(redemption);
            redemptionIds.Add(redemptionId);
        }

        return redemptionIds;
    }

    private async Task<ScanEvent> CreateScanEventAsync(
        Customer customer, 
        Reward reward, 
        Guid businessUserId,
        ScanEventForCreationDto request)
    {
        var scanEvent = new ScanEvent
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            RewardId = reward.Id,
            QrCodeValue = request.QrCodeValue,
            PointsChange = request.PointsChange,
            BusinessUserId = businessUserId,
            ScannedAt = DateTime.UtcNow
        };
        await _repository.CreateScanEvent(scanEvent);
        return scanEvent;
    }

    private ScanEventResponseDto BuildResponse(
        Customer customer, 
        Reward reward, 
        CustomerBalance balance,
        ScanEvent scanEvent,
        int numRewardsClaimed,
        List<Guid>? redemptionIds)
    {
        var response = new ScanEventResponseDto
        {
            ScanEvent = _mapper.Map<ScanEventDto>(scanEvent),
            CustomerName = customer.Name,
            CurrentBalance = balance.Balance,
            RewardAvailable = false,
            AvailableReward = null,
            NumRewardsAvailable = 0,
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
            response.NumRewardsAvailable = balance.Balance / reward.CostPoints;
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

    public async Task<Result<CustomerBalanceAndInfoResponseDto>> GetCustomerBalanceForRewardAsync(
        Guid businessId,
        Guid rewardId, 
        string qrCodeValue,
        Guid businessUserId)
    {
        // 1. Validate JWT token matches business user's auth provider ID
        var jwtAuthCheck = await _authorizationService.ValidateBusinessUserAuthorizationAsync(businessUserId);
        if (jwtAuthCheck.IsFailure)
            return Result<CustomerBalanceAndInfoResponseDto>.Failure(jwtAuthCheck.Error!);

        // 2. Validate Reward belongs to Business
        var rewardCheck = await ValidateRewardBelongsToBusinessAsync(businessId, rewardId);
        if (rewardCheck.IsFailure)
            return Result<CustomerBalanceAndInfoResponseDto>.Failure(rewardCheck.Error!);

        // 3. Validate customer and reward exist
        var validationResult = await ValidateUserAndRewardAsync(rewardId, qrCodeValue);
        if (validationResult.IsFailure)
            return Result<CustomerBalanceAndInfoResponseDto>.Failure(validationResult.Error!);

        var (customer, reward) = validationResult.Value;

        // 4. Get customer balance
        var balance = await _repository.GetCustomerBalanceForRewardAsync(customer.Id, rewardId);

        // 5. Build response
        var response = BuildUserBalanceResponse(customer, reward, balance, qrCodeValue);

        return Result<CustomerBalanceAndInfoResponseDto>.Success(response);
    }

    private async Task<Result<(Customer user, Reward reward)>> ValidateUserAndRewardAsync(
        Guid rewardId, 
        string qrCodeValue)
    {
        var user = await _repository.GetCustomerByQrCodeValueAsync(qrCodeValue);
        if (user == null)
        {
            _logger.LogWarning(
                "Customer balance check: Customer not found. QrCode: {QrCode}, RewardId: {RewardId}", 
                qrCodeValue, rewardId);
            return Result<(Customer, Reward)>.Failure(
                "Invalid QR code or reward");  // Generic
        }

        var reward = await _repository.GetRewardAsync(rewardId);
        if (reward == null)
        {
            _logger.LogWarning(
                "Customer balance check: Reward not found. RewardId: {RewardId}, CustomerId: {CustomerId}, QrCode: {QrCode}", 
                rewardId, user.Id, qrCodeValue);
            return Result<(Customer, Reward)>.Failure(
                "Invalid QR code or reward");  // Generic
        }

        return Result<(Customer, Reward)>.Success((user, reward));
    }

    private CustomerBalanceAndInfoResponseDto BuildUserBalanceResponse(
        Customer customer, 
        Reward reward, 
        CustomerBalance? balance,
        string qrCodeValue)
    {
        var response = new CustomerBalanceAndInfoResponseDto
        {
            QrCodeValue = qrCodeValue,
            CustomerName = customer.Name,
            CurrentBalance = balance?.Balance ?? 0,
            AvailableReward = null,
            NumRewardsAvailable = 0
        };

        // If no balance exists, return empty response
        if (balance == null)
            return response;

        // Check if reward is available based on reward type
        if (reward.RewardType == RewardType.IncrementalPoints && 
            reward.CostPoints > 0 && 
            balance.Balance >= reward.CostPoints)
        {
            response.NumRewardsAvailable = balance.Balance / reward.CostPoints;
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
        Guid businessId,
        Guid rewardId, 
        Guid scanEventId,
        Guid businessUserId)
    {
        // 1. Validate JWT token matches business user's auth provider ID
        var jwtAuthCheck = await _authorizationService.ValidateBusinessUserAuthorizationAsync(businessUserId);
        if (jwtAuthCheck.IsFailure)
            return Result<ScanEventDto>.Failure(jwtAuthCheck.Error!);

        // 2. Validate Reward belongs to Business
        var rewardCheck = await ValidateRewardBelongsToBusinessAsync(businessId, rewardId);
        if (rewardCheck.IsFailure)
            return Result<ScanEventDto>.Failure(rewardCheck.Error!);

        // 3. Get scan event
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


