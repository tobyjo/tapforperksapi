using AutoMapper;
using SaveForPerksAPI.Common;
using SaveForPerksAPI.Entities;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Repositories;

namespace SaveForPerksAPI.Services;

public class RewardManagementService : IRewardManagementService
{
    private readonly ISaveForPerksRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<RewardManagementService> _logger;

    public RewardManagementService(
        ISaveForPerksRepository repository,
        IMapper mapper,
        ILogger<RewardManagementService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RewardDto>> CreateRewardAsync(RewardForCreationDto request, Guid rewardOwnerUserId)
    {
        // 1. Validate request
        var validationResult = ValidateCreateRewardRequest(request);
        if (validationResult.IsFailure)
            return Result<RewardDto>.Failure(validationResult.Error!);

        // 2. Verify RewardOwner exists
        var rewardOwnerCheck = await VerifyRewardOwnerExistsAsync(request.RewardOwnerId);
        if (rewardOwnerCheck.IsFailure)
            return Result<RewardDto>.Failure(rewardOwnerCheck.Error!);

        // 3. Verify RewardOwnerUser belongs to RewardOwner
        var userAuthCheck = await VerifyUserBelongsToRewardOwnerAsync(rewardOwnerUserId, request.RewardOwnerId);
        if (userAuthCheck.IsFailure)
            return Result<RewardDto>.Failure(userAuthCheck.Error!);

        // 4. Check for existing reward for this RewardOwner
        var existingRewardCheck = await CheckForExistingRewardAsync(request.RewardOwnerId);
        if (existingRewardCheck.IsFailure)
            return Result<RewardDto>.Failure(existingRewardCheck.Error!);

        // 5. Create the reward
        var createResult = await CreateRewardEntityAsync(request);
        if (createResult.IsFailure)
            return Result<RewardDto>.Failure(createResult.Error!);

        var reward = createResult.Value;

        // 6. Map and return
        var rewardDto = _mapper.Map<RewardDto>(reward);

        _logger.LogInformation(
            "Reward created successfully. RewardId: {RewardId}, RewardOwnerId: {RewardOwnerId}, Name: {Name}, Type: {Type}, CreatedBy: {UserId}",
            reward.Id, reward.RewardOwnerId, reward.Name, reward.RewardType, rewardOwnerUserId);

        return Result<RewardDto>.Success(rewardDto);
    }

    private Result<bool> ValidateCreateRewardRequest(RewardForCreationDto request)
    {
        if (request.RewardOwnerId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: RewardOwnerId is empty");
            return Result<bool>.Failure("Reward owner ID is required");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Validation failed: Name is required");
            return Result<bool>.Failure("Reward name is required");
        }

        if (string.IsNullOrWhiteSpace(request.RewardType))
        {
            _logger.LogWarning("Validation failed: RewardType is required");
            return Result<bool>.Failure("Reward type is required");
        }

        // Validate RewardType value
        if (!Enum.TryParse<RewardType>(request.RewardType, ignoreCase: true, out _))
        {
            _logger.LogWarning(
                "Validation failed: Invalid RewardType. Value: {RewardType}", 
                request.RewardType);
            return Result<bool>.Failure($"Invalid reward type: {request.RewardType}");
        }

        if (!request.CostPoints.HasValue)
        {
            _logger.LogWarning("Validation failed: CostPoints is required");
            return Result<bool>.Failure("Cost points is required");
        }

        if (request.CostPoints.Value < 0)
        {
            _logger.LogWarning(
                "Validation failed: CostPoints must be non-negative. Value: {CostPoints}", 
                request.CostPoints.Value);
            return Result<bool>.Failure("Cost points must be zero or greater");
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> VerifyRewardOwnerExistsAsync(Guid rewardOwnerId)
    {
        var rewardOwner = await _repository.GetRewardOwnerByIdAsync(rewardOwnerId);
        if (rewardOwner == null)
        {
            _logger.LogWarning(
                "RewardOwner not found. RewardOwnerId: {RewardOwnerId}", 
                rewardOwnerId);
            return Result<bool>.Failure("Reward owner not found");
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> VerifyUserBelongsToRewardOwnerAsync(Guid rewardOwnerUserId, Guid rewardOwnerId)
    {
        if (rewardOwnerUserId == Guid.Empty)
        {
            _logger.LogWarning("RewardOwnerUserId is empty");
            return Result<bool>.Failure("User ID is required");
        }

        var rewardOwnerUser = await _repository.GetRewardOwnerUserByIdAsync(rewardOwnerUserId);
        if (rewardOwnerUser == null)
        {
            _logger.LogWarning(
                "RewardOwnerUser not found. UserId: {UserId}",
                rewardOwnerUserId);
            return Result<bool>.Failure("User not found");
        }

        if (rewardOwnerUser.RewardOwnerId != rewardOwnerId)
        {
            _logger.LogWarning(
                "Authorization failed: User does not belong to RewardOwner. UserId: {UserId}, UserRewardOwnerId: {UserRewardOwnerId}, RequestedRewardOwnerId: {RequestedRewardOwnerId}",
                rewardOwnerUserId, rewardOwnerUser.RewardOwnerId, rewardOwnerId);
            return Result<bool>.Failure("You do not have permission to manage this reward owner");
        }

        _logger.LogInformation(
            "User authorization verified. UserId: {UserId}, RewardOwnerId: {RewardOwnerId}",
            rewardOwnerUserId, rewardOwnerId);

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> CheckForExistingRewardAsync(Guid rewardOwnerId)
    {
        var existingReward = await _repository.GetRewardByRewardOwnerIdAsync(rewardOwnerId);
        if (existingReward != null)
        {
            _logger.LogWarning(
                "Reward already exists for RewardOwner. RewardOwnerId: {RewardOwnerId}, ExistingRewardId: {ExistingRewardId}",
                rewardOwnerId, existingReward.Id);
            return Result<bool>.Failure("A reward already exists for this reward owner. Only one reward per reward owner is currently allowed");
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<Reward>> CreateRewardEntityAsync(RewardForCreationDto request)
    {
        try
        {
            var rewardId = Guid.NewGuid();
            var reward = new Reward
            {
                Id = rewardId,
                RewardOwnerId = request.RewardOwnerId,
                Name = request.Name,
                RewardType = Enum.Parse<RewardType>(request.RewardType, ignoreCase: true),
                CostPoints = request.CostPoints!.Value,
                IsActive = true, // Default to true
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateRewardAsync(reward);

            _logger.LogInformation(
                "Reward entity created. RewardId: {RewardId}, RewardOwnerId: {RewardOwnerId}, Name: {Name}",
                rewardId, reward.RewardOwnerId, reward.Name);

            // Save changes
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Transaction committed successfully. RewardId: {RewardId}",
                rewardId);

            return Result<Reward>.Success(reward);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create reward. RewardOwnerId: {RewardOwnerId}, Name: {Name}, Error: {Error}",
                request.RewardOwnerId, request.Name, ex.Message);
            return Result<Reward>.Failure(
                "An error occurred while creating the reward");
        }
    }

    public async Task<Result<IEnumerable<RewardDto>>> GetRewardsByRewardOwnerIdAsync(Guid rewardOwnerId, Guid rewardOwnerUserId)
    {
        // 1. Validate input
        if (rewardOwnerId == Guid.Empty)
        {
            _logger.LogWarning("GetRewardsByRewardOwnerId called with empty RewardOwnerId");
            return Result<IEnumerable<RewardDto>>.Failure("Reward owner ID is required");
        }

        // 2. Verify RewardOwner exists
        var rewardOwnerCheck = await VerifyRewardOwnerExistsAsync(rewardOwnerId);
        if (rewardOwnerCheck.IsFailure)
            return Result<IEnumerable<RewardDto>>.Failure(rewardOwnerCheck.Error!);

        // 3. Verify RewardOwnerUser belongs to RewardOwner
        var userAuthCheck = await VerifyUserBelongsToRewardOwnerAsync(rewardOwnerUserId, rewardOwnerId);
        if (userAuthCheck.IsFailure)
            return Result<IEnumerable<RewardDto>>.Failure(userAuthCheck.Error!);

        // 4. Get reward for this RewardOwner
        var reward = await _repository.GetRewardByRewardOwnerIdAsync(rewardOwnerId);

        if (reward == null)
        {
            _logger.LogInformation(
                "No rewards found for RewardOwnerId: {RewardOwnerId}, RequestedBy: {UserId}",
                rewardOwnerId, rewardOwnerUserId);
            // Return empty list if no reward exists
            return Result<IEnumerable<RewardDto>>.Success(new List<RewardDto>());
        }

        // 5. Map and return
        var rewardDto = _mapper.Map<RewardDto>(reward);

        _logger.LogInformation(
            "Reward found for RewardOwnerId: {RewardOwnerId}, RewardId: {RewardId}, Name: {Name}, RequestedBy: {UserId}",
            rewardOwnerId, reward.Id, reward.Name, rewardOwnerUserId);

        return Result<IEnumerable<RewardDto>>.Success(new List<RewardDto> { rewardDto });
    }
}
