using AutoMapper;
using SaveForPerksAPI.Common;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Repositories;

namespace SaveForPerksAPI.Services;

public class BusinessUserService : IBusinessUserService
{
    private readonly ISaveForPerksRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<BusinessUserService> _logger;
    private readonly IAuthorizationService _authorizationService;

    public BusinessUserService(
        ISaveForPerksRepository repository,
        IMapper mapper,
        ILogger<BusinessUserService> logger,
        IAuthorizationService authorizationService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public async Task<Result<IEnumerable<BusinessDto>>> GetBusinessesByAuthProviderIdAsync(string authProviderId)
    {
        // 1. Validate input
        if (string.IsNullOrWhiteSpace(authProviderId))
        {
            _logger.LogWarning("GetBusinessesByAuthProviderId called with empty authProviderId");
            return Result<IEnumerable<BusinessDto>>.Failure("Auth provider ID is required");
        }

        // 2. Get BusinessUser by authProviderId
        var businessUser = await _repository.GetBusinessUserByAuthProviderIdAsync(authProviderId);
        
        if (businessUser == null)
        {
            _logger.LogInformation(
                "BusinessUser not found for authProviderId: {AuthProviderId}", 
                authProviderId);
            // Return empty list as requested
            return Result<IEnumerable<BusinessDto>>.Success(new List<BusinessDto>());
        }

        // 3. Get Business
        var business = await _repository.GetBusinessByIdAsync(businessUser.BusinessId);
        
        if (business == null)
        {
            _logger.LogWarning(
                "Business not found for BusinessId: {BusinessId}, BusinessUserId: {BusinessUserId}", 
                businessUser.BusinessId, businessUser.Id);
            // Return empty list if business doesn't exist (data integrity issue)
            return Result<IEnumerable<BusinessDto>>.Success(new List<BusinessDto>());
        }

        // 4. Map and return
        var businessDto = _mapper.Map<BusinessDto>(business);
        
        _logger.LogInformation(
            "Business found for authProviderId: {AuthProviderId}, BusinessId: {BusinessId}, BusinessName: {Name}",
            authProviderId, business.Id, business.Name);


        return Result<IEnumerable<BusinessDto>>.Success(new List<BusinessDto> { businessDto });
    }

    public async Task<Result<BusinessUserDto>> GetBusinessUserByAuthProviderIdAsync(string authProviderId)
    {
        // 1. Validate input
        if (string.IsNullOrWhiteSpace(authProviderId))
        {
            _logger.LogWarning("GetBusinessUserByAuthProviderId called with empty authProviderId");
            return Result<BusinessUserDto>.Failure("Auth provider ID is required");
        }

        // 2. Get BusinessUser by authProviderId
        var businessUser = await _repository.GetBusinessUserByAuthProviderIdAsync(authProviderId);
        
        if (businessUser == null)
        {
            _logger.LogInformation(
                "BusinessUser not found for authProviderId: {AuthProviderId}", 
                authProviderId);
            return Result<BusinessUserDto>.Failure("Customer not found");
        }

        // 3. Map and return
        var rewardOwnerUserDto = _mapper.Map<BusinessUserDto>(businessUser);
        
        _logger.LogInformation(
            "BusinessUser found for authProviderId: {AuthProviderId}, BusinessUserId: {BusinessUserId}, Email: {Email}",
            authProviderId, businessUser.Id, businessUser.Email);


        return Result<BusinessUserDto>.Success(rewardOwnerUserDto);
    }

    public async Task<Result<IEnumerable<BusinessUserProfileResponseDto>>> GetBusinessUserProfilesByAuthProviderIdAsync(string authProviderId)
    {
        // 1. Validate input
        if (string.IsNullOrWhiteSpace(authProviderId))
        {
            _logger.LogWarning("GetBusinessUserProfilesByAuthProviderId called with empty authProviderId");
            return Result<IEnumerable<BusinessUserProfileResponseDto>>.Failure("Auth provider ID is required");
        }

        // 2. Validate JWT token matches auth provider ID
        var authCheck = _authorizationService.ValidateAuthProviderIdMatch(authProviderId);
        if (authCheck.IsFailure)
            return Result<IEnumerable<BusinessUserProfileResponseDto>>.Failure(authCheck.Error!);

        // 3. Get BusinessUser by authProviderId
        var businessUser = await _repository.GetBusinessUserByAuthProviderIdAsync(authProviderId);

        if (businessUser == null)
        {
            _logger.LogInformation(
                "BusinessUser not found for authProviderId: {AuthProviderId}", 
                authProviderId);
            // Return empty list as requested
            return Result<IEnumerable<BusinessUserProfileResponseDto>>.Success(
                new List<BusinessUserProfileResponseDto>());
        }

        // 4. Get Business
        var business = await _repository.GetBusinessByIdAsync(businessUser.BusinessId);

        if (business == null)
        {
            _logger.LogWarning(
                "Business not found for BusinessId: {BusinessId}, BusinessUserId: {BusinessUserId}", 
                businessUser.BusinessId, businessUser.Id);
            // Return empty list if reward owner doesn't exist
            return Result<IEnumerable<BusinessUserProfileResponseDto>>.Success(
                new List<BusinessUserProfileResponseDto>());
        }

        // 5. Determine if profile exists based on Business.Name
        var businessProfileExists = !string.IsNullOrWhiteSpace(business.Name);

        // 6. Get rewards for this business
        var reward = await _repository.GetRewardByBusinessIdAsync(businessUser.BusinessId);
        var rewards = reward != null 
            ? new List<RewardDto> { _mapper.Map<RewardDto>(reward) }
            : new List<RewardDto>();

        // 7. Map and build response
        var businessDto = _mapper.Map<BusinessDto>(business);
        var businessUserDto = _mapper.Map<BusinessUserDto>(businessUser);

        var profile = new BusinessUserProfileResponseDto
        {
            BusinessProfileExists = businessProfileExists,
            Business = businessDto,
            BusinessUser = businessUserDto,
            Rewards = rewards
        };

        _logger.LogInformation(
            "BusinessUserProfile found for authProviderId: {AuthProviderId}, BusinessId: {BusinessId}, BusinessUserId: {BusinessUserId}, ProfileExists: {ProfileExists}, RewardCount: {RewardCount}",
            authProviderId, business.Id, businessUser.Id, businessProfileExists, rewards.Count);

        return Result<IEnumerable<BusinessUserProfileResponseDto>>.Success(
            new List<BusinessUserProfileResponseDto> { profile });
    }
}
