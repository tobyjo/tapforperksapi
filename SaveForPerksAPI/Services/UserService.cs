using AutoMapper;
using SaveForPerksAPI.Common;
using SaveForPerksAPI.Entities;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Repositories;

namespace SaveForPerksAPI.Services;

public class UserService : IUserService
{
    private readonly ISaveForPerksRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly IQrCodeService _qrCodeService;

    public UserService(
        ISaveForPerksRepository repository,
        IMapper mapper,
        ILogger<UserService> logger,
        IQrCodeService qrCodeService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
    }

    public async Task<Result<UserDto>> GetUserByAuthProviderIdAsync(string authProviderId)
    {
        // 1. Validate input
        if (string.IsNullOrWhiteSpace(authProviderId))
        {
            _logger.LogWarning("GetUserByAuthProviderId called with empty authProviderId");
            return Result<UserDto>.Failure("Auth provider ID is required");
        }

        // 2. Get User by authProviderId
        var user = await _repository.GetUserByAuthProviderIdAsync(authProviderId);
        
        if (user == null)
        {
            _logger.LogInformation(
                "User not found for authProviderId: {AuthProviderId}", 
                authProviderId);
            return Result<UserDto>.Failure("User not found");
        }

        // 3. Map and return
        var userDto = _mapper.Map<UserDto>(user);
        
        _logger.LogInformation(
            "User found for authProviderId: {AuthProviderId}, UserId: {UserId}, Email: {Email}",
            authProviderId, user.Id, user.Email);

        return Result<UserDto>.Success(userDto);
    }

    public async Task<Result<UserDto>> CreateUserAsync(UserForCreationDto request)
    {
        // 1. Validate request
        var validationResult = ValidateCreateUserRequest(request);
        if (validationResult.IsFailure)
            return Result<UserDto>.Failure(validationResult.Error!);

        // 2. Check for duplicate email
        var emailCheck = await CheckDuplicateEmailAsync(request.Email);
        if (emailCheck.IsFailure)
            return Result<UserDto>.Failure(emailCheck.Error!);

        // 3. Check for duplicate authProviderId
        var authProviderCheck = await CheckDuplicateAuthProviderIdAsync(request.AuthProviderId);
        if (authProviderCheck.IsFailure)
            return Result<UserDto>.Failure(authProviderCheck.Error!);

        // 4. Generate unique QR code
        var qrCodeResult = await GenerateUniqueQrCodeAsync();
        if (qrCodeResult.IsFailure)
            return Result<UserDto>.Failure(qrCodeResult.Error!);

        // 5. Create the user
        var createResult = await CreateUserEntityAsync(request, qrCodeResult.Value);
        if (createResult.IsFailure)
            return Result<UserDto>.Failure(createResult.Error!);

        var user = createResult.Value;

        // 6. Map and return
        var userDto = _mapper.Map<UserDto>(user);

        _logger.LogInformation(
            "User created successfully. UserId: {UserId}, Email: {Email}, AuthProviderId: {AuthProviderId}, QrCodeValue: {QrCodeValue}",
            user.Id, user.Email, user.AuthProviderId, user.QrCodeValue);

        return Result<UserDto>.Success(userDto);
    }

    private Result<bool> ValidateCreateUserRequest(UserForCreationDto request)
    {
        if (string.IsNullOrWhiteSpace(request.AuthProviderId))
        {
            _logger.LogWarning("Validation failed: AuthProviderId is required");
            return Result<bool>.Failure("Auth provider ID is required");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("Validation failed: Email is required");
            return Result<bool>.Failure("Email is required");
        }

        // Basic email validation
        if (!request.Email.Contains('@') || !request.Email.Contains('.'))
        {
            _logger.LogWarning("Validation failed: Invalid email format. Email: {Email}", request.Email);
            return Result<bool>.Failure("Invalid email format");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Validation failed: Name is required");
            return Result<bool>.Failure("Name is required");
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> CheckDuplicateEmailAsync(string email)
    {
        var existingUser = await _repository.GetUserByEmailAsync(email);
        if (existingUser != null)
        {
            _logger.LogWarning(
                "Duplicate email detected during user creation. Email: {Email}",
                email);
            return Result<bool>.Failure("A user with this email already exists");
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> CheckDuplicateAuthProviderIdAsync(string authProviderId)
    {
        var existingUser = await _repository.GetUserByAuthProviderIdAsync(authProviderId);
        if (existingUser != null)
        {
            _logger.LogWarning(
                "Duplicate auth provider ID detected during user creation. AuthProviderId: {AuthProviderId}",
                authProviderId);
            return Result<bool>.Failure("A user with this auth provider ID already exists");
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<string>> GenerateUniqueQrCodeAsync()
    {
        const int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var qrCodeValue = _qrCodeService.GenerateQrCodeValue();
            
            if (await _qrCodeService.IsQrCodeUniqueAsync(qrCodeValue))
            {
                _logger.LogInformation(
                    "Unique QR code generated: {QrCodeValue} (attempt {Attempt})",
                    qrCodeValue, attempt + 1);
                return Result<string>.Success(qrCodeValue);
            }

            _logger.LogWarning(
                "QR code collision detected: {QrCodeValue} (attempt {Attempt})",
                qrCodeValue, attempt + 1);
        }

        _logger.LogError("Failed to generate unique QR code after {MaxAttempts} attempts", maxAttempts);
        return Result<string>.Failure("Unable to generate unique QR code. Please try again");
    }

    private async Task<Result<User>> CreateUserEntityAsync(UserForCreationDto request, string qrCodeValue)
    {
        try
        {
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                AuthProviderId = request.AuthProviderId,
                Email = request.Email,
                Name = request.Name,
                QrCodeValue = qrCodeValue,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateUserAsync(user);

            _logger.LogInformation(
                "User entity created. UserId: {UserId}, Email: {Email}, QrCodeValue: {QrCodeValue}",
                userId, user.Email, user.QrCodeValue);

            // Save changes
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Transaction committed successfully. UserId: {UserId}",
                userId);

            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create user. Email: {Email}, AuthProviderId: {AuthProviderId}, Error: {Error}",
                request.Email, request.AuthProviderId, ex.Message);
            return Result<User>.Failure(
                "An error occurred while creating the user");
        }
    }
}
