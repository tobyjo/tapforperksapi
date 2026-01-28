using AutoMapper;
using SaveForPerksAPI.Common;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Repositories;

namespace SaveForPerksAPI.Services;

public class RewardOwnerUserService : IRewardOwnerUserService
{
    private readonly ISaveForPerksRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<RewardOwnerUserService> _logger;

    public RewardOwnerUserService(
        ISaveForPerksRepository repository,
        IMapper mapper,
        ILogger<RewardOwnerUserService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<RewardOwnerDto>>> GetRewardOwnersByAuthProviderIdAsync(string authProviderId)
    {
        // 1. Validate input
        if (string.IsNullOrWhiteSpace(authProviderId))
        {
            _logger.LogWarning("GetRewardOwnersByAuthProviderId called with empty authProviderId");
            return Result<IEnumerable<RewardOwnerDto>>.Failure("Auth provider ID is required");
        }

        // 2. Get RewardOwnerUser by authProviderId
        var rewardOwnerUser = await _repository.GetRewardOwnerUserByAuthProviderIdAsync(authProviderId);
        
        if (rewardOwnerUser == null)
        {
            _logger.LogInformation(
                "RewardOwnerUser not found for authProviderId: {AuthProviderId}", 
                authProviderId);
            // Return empty list as requested
            return Result<IEnumerable<RewardOwnerDto>>.Success(new List<RewardOwnerDto>());
        }

        // 3. Get RewardOwner
        var rewardOwner = await _repository.GetRewardOwnerByIdAsync(rewardOwnerUser.RewardOwnerId);
        
        if (rewardOwner == null)
        {
            _logger.LogWarning(
                "RewardOwner not found for RewardOwnerId: {RewardOwnerId}, UserId: {UserId}", 
                rewardOwnerUser.RewardOwnerId, rewardOwnerUser.Id);
            // Return empty list if reward owner doesn't exist (data integrity issue)
            return Result<IEnumerable<RewardOwnerDto>>.Success(new List<RewardOwnerDto>());
        }

        // 4. Map and return
        var rewardOwnerDto = _mapper.Map<RewardOwnerDto>(rewardOwner);
        
        _logger.LogInformation(
            "RewardOwner found for authProviderId: {AuthProviderId}, RewardOwnerId: {RewardOwnerId}, RewardOwnerName: {Name}",
            authProviderId, rewardOwner.Id, rewardOwner.Name);


        return Result<IEnumerable<RewardOwnerDto>>.Success(new List<RewardOwnerDto> { rewardOwnerDto });
    }

    public async Task<Result<RewardOwnerUserDto>> GetRewardOwnerUserByAuthProviderIdAsync(string authProviderId)
    {
        // 1. Validate input
        if (string.IsNullOrWhiteSpace(authProviderId))
        {
            _logger.LogWarning("GetRewardOwnerUserByAuthProviderId called with empty authProviderId");
            return Result<RewardOwnerUserDto>.Failure("Auth provider ID is required");
        }

        // 2. Get RewardOwnerUser by authProviderId
        var rewardOwnerUser = await _repository.GetRewardOwnerUserByAuthProviderIdAsync(authProviderId);
        
        if (rewardOwnerUser == null)
        {
            _logger.LogInformation(
                "RewardOwnerUser not found for authProviderId: {AuthProviderId}", 
                authProviderId);
            return Result<RewardOwnerUserDto>.Failure("User not found");
        }

        // 3. Map and return
        var rewardOwnerUserDto = _mapper.Map<RewardOwnerUserDto>(rewardOwnerUser);
        
        _logger.LogInformation(
            "RewardOwnerUser found for authProviderId: {AuthProviderId}, UserId: {UserId}, Email: {Email}",
            authProviderId, rewardOwnerUser.Id, rewardOwnerUser.Email);

        return Result<RewardOwnerUserDto>.Success(rewardOwnerUserDto);
    }
}
