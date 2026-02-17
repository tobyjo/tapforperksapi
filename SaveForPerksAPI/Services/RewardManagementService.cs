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
    private readonly IAuthorizationService _authorizationService;

    public RewardManagementService(
        ISaveForPerksRepository repository,
        IMapper mapper,
        ILogger<RewardManagementService> logger,
        IAuthorizationService authorizationService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public async Task<Result<RewardDto>> CreateRewardAsync(RewardForCreationDto request, Guid businessUserId)
    {
        // 1. Validate JWT token matches business user's auth provider ID
        var authCheck = await _authorizationService.ValidateBusinessUserAuthorizationAsync(businessUserId);
        if (authCheck.IsFailure)
            return Result<RewardDto>.Failure(authCheck.Error!);

        // 2. Validate request
        var validationResult = ValidateCreateRewardRequest(request);
        if (validationResult.IsFailure)
            return Result<RewardDto>.Failure(validationResult.Error!);

        // 3. Verify Business exists
        var businessCheck = await VerifyBusinessExistsAsync(request.BusinessId);
        if (businessCheck.IsFailure)
            return Result<RewardDto>.Failure(businessCheck.Error!);

        // 4. Verify BusinessUser belongs to Business
        var userAuthCheck = await VerifyUserBelongsToBusinessAsync(businessUserId, request.BusinessId);
        if (userAuthCheck.IsFailure)
            return Result<RewardDto>.Failure(userAuthCheck.Error!);

        // 5. Check for existing reward for this Business
        var existingRewardCheck = await CheckForExistingRewardAsync(request.BusinessId);
        if (existingRewardCheck.IsFailure)
            return Result<RewardDto>.Failure(existingRewardCheck.Error!);

        // 6. Create the reward
        var createResult = await CreateRewardEntityAsync(request);
        if (createResult.IsFailure)
            return Result<RewardDto>.Failure(createResult.Error!);

        var reward = createResult.Value;

        // 7. Map and return
        var rewardDto = _mapper.Map<RewardDto>(reward);

        _logger.LogInformation(
            "Reward created successfully. RewardId: {RewardId}, BusinessId: {BusinessId}, Name: {Name}, Type: {Type}, CreatedBy: {businessUserId}",
            reward.Id, reward.BusinessId, reward.Name, reward.RewardType, businessUserId);

        return Result<RewardDto>.Success(rewardDto);
    }

    public async Task<Result<RewardDto>> UpdateRewardAsync(Guid rewardId, RewardForUpdateDto request, Guid businessUserId)
    {
        // 1. Validate JWT token matches business user's auth provider ID
        var authCheck = await _authorizationService.ValidateBusinessUserAuthorizationAsync(businessUserId);
        if (authCheck.IsFailure)
            return Result<RewardDto>.Failure(authCheck.Error!);

        // 2. Validate request
        var validationResult = ValidateUpdateRewardRequest(request);
        if (validationResult.IsFailure)
            return Result<RewardDto>.Failure(validationResult.Error!);

        // 3. Get reward and validate it exists
        var reward = await _repository.GetRewardAsync(rewardId);
        if (reward == null)
        {
            _logger.LogWarning("Reward not found. RewardId: {RewardId}", rewardId);
            return Result<RewardDto>.Failure("Reward not found");
        }

        // 4. Get BusinessUser and validate
        var businessUser = await _repository.GetBusinessUserByIdAsync(businessUserId);
        if (businessUser == null)
        {
            _logger.LogWarning("BusinessUser not found. BusinessUserId: {BusinessUserId}", businessUserId);
            return Result<RewardDto>.Failure("User not found");
        }

        // 5. Verify BusinessUser belongs to the same Business as the Reward
        if (businessUser.BusinessId != reward.BusinessId)
        {
            _logger.LogWarning(
                "Authorization failed: BusinessUser {BusinessUserId} does not belong to Business {BusinessId} that owns Reward {RewardId}",
                businessUserId, reward.BusinessId, rewardId);
            return Result<RewardDto>.Failure("You do not have permission to modify this reward");
        }

        // 6. Verify BusinessUser is an admin
        if (!businessUser.IsAdmin)
        {
            _logger.LogWarning(
                "Authorization failed: BusinessUser {BusinessUserId} is not an admin. Cannot update Reward {RewardId}",
                businessUserId, rewardId);
            return Result<RewardDto>.Failure("Only administrators can modify rewards");
        }

        // 7. Update the reward
        var updateResult = await UpdateRewardEntityAsync(reward, request, businessUserId);
        if (updateResult.IsFailure)
            return Result<RewardDto>.Failure(updateResult.Error!);

        var updatedReward = updateResult.Value;

        // 8. Map and return
        var rewardDto = _mapper.Map<RewardDto>(updatedReward);

        _logger.LogInformation(
            "Reward updated successfully. RewardId: {RewardId}, BusinessId: {BusinessId}, UpdatedBy: {BusinessUserId}",
            rewardId, reward.BusinessId, businessUserId);

        return Result<RewardDto>.Success(rewardDto);
    }

    private Result<bool> ValidateUpdateRewardRequest(RewardForUpdateDto request)
    {
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

    private async Task<Result<Reward>> UpdateRewardEntityAsync(
        Reward reward, 
        RewardForUpdateDto request, 
        Guid businessUserId)
    {
        try
        {
            // Update the reward properties
            reward.Name = request.Name;
            reward.RewardType = Enum.Parse<RewardType>(request.RewardType, ignoreCase: true);
            reward.CostPoints = request.CostPoints!.Value;

            if (request.IsActive.HasValue)
            {
                reward.IsActive = request.IsActive.Value;
            }

            _logger.LogInformation(
                "Reward entity updated. RewardId: {RewardId}, Name: {Name}, UpdatedBy: {BusinessUserId}",
                reward.Id, reward.Name, businessUserId);

            // Save changes (EF Core tracks changes automatically)
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Transaction committed successfully. RewardId: {RewardId}",
                reward.Id);

            return Result<Reward>.Success(reward);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to update reward. RewardId: {RewardId}, Name: {Name}, Error: {Error}",
                reward.Id, request.Name, ex.Message);
            return Result<Reward>.Failure(
                "An error occurred while updating the reward");
        }
    }

    private Result<bool> ValidateCreateRewardRequest(RewardForCreationDto request)
    {
        if (request.BusinessId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: BusinessId is empty");
            return Result<bool>.Failure("Business ID is required");
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

    private async Task<Result<bool>> VerifyBusinessExistsAsync(Guid businessId)
    {
        var rewardOwner = await _repository.GetBusinessByIdAsync(businessId);
        if (rewardOwner == null)
        {
            _logger.LogWarning(
                "Business not found. BusinessId: {BusinessId}", 
                businessId);
            return Result<bool>.Failure("Business not found");
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> VerifyUserBelongsToBusinessAsync(Guid businessUserId, Guid businessId)
    {
        if (businessUserId == Guid.Empty)
        {
            _logger.LogWarning("BusinessUserId is empty");
            return Result<bool>.Failure("Business User ID is required");
        }

        var rewardOwnerUser = await _repository.GetBusinessUserByIdAsync(businessUserId);
        if (rewardOwnerUser == null)
        {
            _logger.LogWarning(
                "BusinessUser not found. BusinessUserId: {BusinessUserId}",
                businessUserId);
            return Result<bool>.Failure("Customer not found");
        }

        if (rewardOwnerUser.BusinessId != businessId)
        {
            _logger.LogWarning(
                "Authorization failed: Business Customer does not belong to Business. CustomerId: {CustomerId}, UserRewardOwnerId: {UserRewardOwnerId}, RequestedRewardOwnerId: {RequestedRewardOwnerId}",
                businessUserId, rewardOwnerUser.BusinessId, businessId);
            return Result<bool>.Failure("You do not have permission to manage this business");
        }

        _logger.LogInformation(
            "Customer authorization verified. BusinessUserId: {BusinessUserId}, BusinessId: {BusinessId}",
            businessUserId, businessId);

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> CheckForExistingRewardAsync(Guid businessId)
    {
        var existingReward = await _repository.GetRewardByBusinessIdAsync(businessId);
        if (existingReward != null)
        {
            _logger.LogWarning(
                "Reward already exists for Business. BusinessId: {BusinessId}, ExistingRewardId: {ExistingRewardId}",
                businessId, existingReward.Id);
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
                BusinessId = request.BusinessId,
                Name = request.Name,
                RewardType = Enum.Parse<RewardType>(request.RewardType, ignoreCase: true),
                CostPoints = request.CostPoints!.Value,
                IsActive = true, // Default to true
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateRewardAsync(reward);

            _logger.LogInformation(
                "Reward entity created. RewardId: {RewardId}, BusinessId: {BusinessId}, Name: {Name}",
                rewardId, reward.BusinessId, reward.Name);

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
                "Failed to create reward. BusinessId: {BusinessId}, Name: {Name}, Error: {Error}",
                request.BusinessId, request.Name, ex.Message);
            return Result<Reward>.Failure(
                "An error occurred while creating the reward");
        }
    }

    public async Task<Result<IEnumerable<RewardDto>>> GetRewardsByBusinessIdAsync(Guid businessId, Guid businessUserId)
    {
        // 1. Validate JWT token matches business user's auth provider ID
        var authCheck = await _authorizationService.ValidateBusinessUserAuthorizationAsync(businessUserId);
        if (authCheck.IsFailure)
            return Result<IEnumerable<RewardDto>>.Failure(authCheck.Error!);

        // 2. Validate input
        if (businessId == Guid.Empty)
        {
            _logger.LogWarning("GetRewardsByBusinessId called with empty BusinessId");
            return Result<IEnumerable<RewardDto>>.Failure("Business ID is required");
        }

        // 3. Verify Business exists
        var rewardOwnerCheck = await VerifyBusinessExistsAsync(businessId);
        if (rewardOwnerCheck.IsFailure)
            return Result<IEnumerable<RewardDto>>.Failure(rewardOwnerCheck.Error!);

        // 4. Verify BusinessUser belongs to Business
        var userAuthCheck = await VerifyUserBelongsToBusinessAsync(businessUserId, businessId);
        if (userAuthCheck.IsFailure)
            return Result<IEnumerable<RewardDto>>.Failure(userAuthCheck.Error!);

        // 5. Get reward for this Business
        var reward = await _repository.GetRewardByBusinessIdAsync(businessId);

        if (reward == null)
        {
            _logger.LogInformation(
                "No rewards found for BusinessId: {BusinessId}, RequestedBy: {BusinessUserId}",
                businessId, businessUserId);
            // Return empty list if no reward exists
            return Result<IEnumerable<RewardDto>>.Success(new List<RewardDto>());
        }

        // 6. Map and return
        var rewardDto = _mapper.Map<RewardDto>(reward);

        _logger.LogInformation(
            "Reward found for BusinessId: {BusinessId}, RewardId: {RewardId}, Name: {Name}, RequestedBy: {BusinessUserId}",
            businessId, reward.Id, reward.Name, businessUserId);

        return Result<IEnumerable<RewardDto>>.Success(new List<RewardDto> { rewardDto });
    }
}
