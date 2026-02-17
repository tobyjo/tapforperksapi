using AutoMapper;
using SaveForPerksAPI.Common;
using SaveForPerksAPI.Entities;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Repositories;

namespace SaveForPerksAPI.Services;

public class BusinessService : IBusinessService
{
    private readonly ISaveForPerksRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<BusinessService> _logger;
    private readonly IAuthorizationService _authorizationService;

    public BusinessService(
        ISaveForPerksRepository repository,
        IMapper mapper,
        ILogger<BusinessService> logger,
        IAuthorizationService authorizationService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public async Task<Result<BusinessWithAdminUserResponseDto>> CreateBusinessAsync(
        BusinessWithAdminUserForCreationDto request)
    {
        // 1. Validate JWT token matches auth provider ID
        var authCheck = _authorizationService.ValidateAuthProviderIdMatch(request.BusinessUserAuthProviderId);
        if (authCheck.IsFailure)
            return Result<BusinessWithAdminUserResponseDto>.Failure(authCheck.Error!);

        // 2. Validate request
        var validationResult = ValidateCreateBusinessRequest(request);
        if (validationResult.IsFailure)
            return Result<BusinessWithAdminUserResponseDto>.Failure(validationResult.Error!);

        // 3. Validate category exists
        var categoryCheck = await ValidateCategoryExistsAsync(request.BusinessCategoryId);
        if (categoryCheck.IsFailure)
            return Result<BusinessWithAdminUserResponseDto>.Failure(categoryCheck.Error!);

        // 4. Check for duplicate email
        var duplicateEmailCheck = await CheckDuplicateEmailAsync(request.BusinessUserEmail);
        if (duplicateEmailCheck.IsFailure)
            return Result<BusinessWithAdminUserResponseDto>.Failure(duplicateEmailCheck.Error!);

        // 5. Check for duplicate auth provider ID
        var duplicateAuthCheck = await CheckDuplicateAuthProviderIdAsync(request.BusinessUserAuthProviderId);
        if (duplicateAuthCheck.IsFailure)
            return Result<BusinessWithAdminUserResponseDto>.Failure(duplicateAuthCheck.Error!);

        // 6. Create entities in transaction
        var createResult = await CreateBusinessAndUserAsync(request);
        if (createResult.IsFailure)
            return Result<BusinessWithAdminUserResponseDto>.Failure(createResult.Error!);

        var (business, businessUser) = createResult.Value;

        // 7. Build response
        var response = BuildCreateBusinessResponse(business, businessUser);

        _logger.LogInformation(
            "Business and admin user created successfully. BusinessId: {BusinessId}, BusinessUserId: {UserId}, Email: {Email}",
            business.Id, businessUser.Id, businessUser.Email);

        return Result<BusinessWithAdminUserResponseDto>.Success(response);
    }

    private Result<bool> ValidateCreateBusinessRequest(BusinessWithAdminUserForCreationDto request)
    {
        if (string.IsNullOrWhiteSpace(request.BusinessName))
        {
            _logger.LogWarning("Validation failed: BusinessName is required");
            return Result<bool>.Failure("Business name is required");
        }

        if (request.BusinessCategoryId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: BusinessCategoryId is required");
            return Result<bool>.Failure("Category ID is required");
        }

        if (string.IsNullOrWhiteSpace(request.BusinessUserAuthProviderId))
        {
            _logger.LogWarning("Validation failed: Auth provider ID is required");
            return Result<bool>.Failure("Authentication provider ID is required");
        }

        if (string.IsNullOrWhiteSpace(request.BusinessUserEmail))
        {
            _logger.LogWarning("Validation failed: Email is required");
            return Result<bool>.Failure("Email is required");
        }

        // Basic email validation
        if (!request.BusinessUserEmail.Contains('@') || !request.BusinessUserEmail.Contains('.'))
        {
            _logger.LogWarning("Validation failed: Invalid email format. Email: {Email}", request.BusinessUserEmail);
            return Result<bool>.Failure("Invalid email format");
        }

        if (string.IsNullOrWhiteSpace(request.BusinessUserName))
        {
            _logger.LogWarning("Validation failed: User name is required");
            return Result<bool>.Failure("User name is required");
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> ValidateCategoryExistsAsync(Guid categoryId)
    {
        var category = await _repository.GetBusinessCategoryByIdAsync(categoryId);
        if (category == null)
        {
            _logger.LogWarning(
                "Category not found. CategoryId: {CategoryId}",
                categoryId);
            return Result<bool>.Failure("Invalid category. Please select a valid category");
        }

        _logger.LogInformation(
            "Category validated. CategoryId: {CategoryId}, CategoryName: {CategoryName}",
            categoryId, category.Name);

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> CheckDuplicateEmailAsync(string email)
    {
        var existingUser = await _repository.GetBusinessUserByEmailAsync(email);
        if (existingUser != null)
        {
            _logger.LogWarning(
                "Duplicate email detected during business creation. Email: {Email}",
                email);
            return Result<bool>.Failure("A user with this email already exists");
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> CheckDuplicateAuthProviderIdAsync(string authProviderId)
    {
        var existingUser = await _repository.GetBusinessUserByAuthProviderIdAsync(authProviderId);
        if (existingUser != null)
        {
            _logger.LogWarning(
                "Duplicate auth provider ID detected during business creation. AuthProviderId: {AuthProviderId}",
                authProviderId);
            return Result<bool>.Failure("A user with this authentication provider ID already exists");
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<(Business business, BusinessUser businessUser)>>
        CreateBusinessAndUserAsync(BusinessWithAdminUserForCreationDto request)
    {
        try
        {
            // Create Business
            var businessId = Guid.NewGuid();
            var business = new Business
            {
                Id = businessId,
                Name = request.BusinessName,
                Description = request.BusinessDescription,
                CategoryId = request.BusinessCategoryId,
                Address = null, // Can be added later if needed
                CreatedAt = DateTime.UtcNow
            };
            await _repository.CreateBusinessAsync(business);

            _logger.LogInformation(
                "Business entity created. BusinessId: {BusinessId}, Name: {Name}, CategoryId: {CategoryId}",
                businessId, business.Name, business.CategoryId);

            // Create BusinessUser (admin)
            var businessUserId = Guid.NewGuid();
            var businessUser = new BusinessUser
            {
                Id = businessUserId,
                BusinessId = businessId,
                AuthProviderId = request.BusinessUserAuthProviderId,
                Email = request.BusinessUserEmail,
                Name = request.BusinessUserName,
                IsAdmin = true, // Always admin for this creation flow
                CreatedAt = DateTime.UtcNow
            };
            await _repository.CreateBusinessUserAsync(businessUser);

            _logger.LogInformation(
                "BusinessUser entity created. BusinessUserId: {BusinessUserId}, Email: {Email}, IsAdmin: true, BusinessId: {BusinessId}",
                businessUserId, businessUser.Email, businessId);

            // Save changes (atomic transaction)
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Transaction committed successfully. BusinessId: {BusinessId}, BusinessUserId: {BusinessUserId}",
                businessId, businessUserId);

            return Result<(Business, BusinessUser)>.Success((business, businessUser));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create business and admin user. BusinessName: {Name}, Email: {Email}, Error: {Error}",
                request.BusinessName, request.BusinessUserEmail, ex.Message);
            return Result<(Business, BusinessUser)>.Failure(
                "An error occurred while creating the business account");
        }
    }

    private BusinessWithAdminUserResponseDto BuildCreateBusinessResponse(
        Business business,
        BusinessUser businessUser)
    {
        return new BusinessWithAdminUserResponseDto
        {
            Business = _mapper.Map<BusinessDto>(business),
            BusinessUser = _mapper.Map<BusinessUserDto>(businessUser)
        };
    }
}
