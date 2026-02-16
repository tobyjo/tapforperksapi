using AutoMapper;
using SaveForPerksAPI.Common;
using SaveForPerksAPI.Entities;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Repositories;

namespace SaveForPerksAPI.Services;

public class CustomerService : ICustomerService
{
    private readonly ISaveForPerksRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;
    private readonly IQrCodeService _qrCodeService;
    private readonly IAuthorizationService _authorizationService;

    public CustomerService(
        ISaveForPerksRepository repository,
        IMapper mapper,
        ILogger<CustomerService> logger,
        IQrCodeService qrCodeService,
        IAuthorizationService authorizationService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public async Task<Result<CustomerDto>> GetCustomerByAuthProviderIdAsync(string authProviderId)
    {
        // 1. Validate input
        if (string.IsNullOrWhiteSpace(authProviderId))
        {
            _logger.LogWarning("GetCustomerByAuthProviderId called with empty authProviderId");
            return Result<CustomerDto>.Failure("Auth provider ID is required");
        }

        // 2. Get Customer by authProviderId
        var user = await _repository.GetCustomerByAuthProviderIdAsync(authProviderId);
        
        if (user == null)
        {
            _logger.LogInformation(
                "Customer not found for authProviderId: {AuthProviderId}", 
                authProviderId);
            return Result<CustomerDto>.Failure("Customer not found");
        }

        // 3. Map and return
        var userDto = _mapper.Map<CustomerDto>(user);
        
        _logger.LogInformation(
            "Customer found for authProviderId: {AuthProviderId}, CustomerId: {CustomerId}, Email: {Email}",
            authProviderId, user.Id, user.Email);

        return Result<CustomerDto>.Success(userDto);
    }

    public async Task<Result<CustomerDto>> CreateCustomerAsync(CustomerForCreationDto request)
    {
        // 1. Validate request
        var validationResult = ValidateCreateUserRequest(request);
        if (validationResult.IsFailure)
            return Result<CustomerDto>.Failure(validationResult.Error!);

        // 2. Validate JWT token matches auth provider ID
        var authCheck = _authorizationService.ValidateAuthProviderIdMatch(request.AuthProviderId);
        if (authCheck.IsFailure)
            return Result<CustomerDto>.Failure(authCheck.Error!);

        // 3. Check for duplicate email
        var emailCheck = await CheckDuplicateEmailAsync(request.Email);
        if (emailCheck.IsFailure)
            return Result<CustomerDto>.Failure(emailCheck.Error!);

        // 4. Check for duplicate authProviderId
        var authProviderCheck = await CheckDuplicateAuthProviderIdAsync(request.AuthProviderId);
        if (authProviderCheck.IsFailure)
            return Result<CustomerDto>.Failure(authProviderCheck.Error!);

        // 5. Generate unique QR code
        var qrCodeResult = await GenerateUniqueQrCodeAsync();
        if (qrCodeResult.IsFailure)
            return Result<CustomerDto>.Failure(qrCodeResult.Error!);

        // 6. Create the user
        var createResult = await CreateCustomerEntityAsync(request, qrCodeResult.Value);
        if (createResult.IsFailure)
            return Result<CustomerDto>.Failure(createResult.Error!);

        var user = createResult.Value;

        // 7. Map and return
        var userDto = _mapper.Map<CustomerDto>(user);

        _logger.LogInformation(
            "Customer created successfully. CustomerId: {CustomerId}, Email: {Email}, AuthProviderId: {AuthProviderId}, QrCodeValue: {QrCodeValue}",
            user.Id, user.Email, user.AuthProviderId, user.QrCodeValue);

        return Result<CustomerDto>.Success(userDto);
    }

    private Result<bool> ValidateCreateUserRequest(CustomerForCreationDto request)
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
        var existingUser = await _repository.GetCustomerByEmailAsync(email);
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
        var existingUser = await _repository.GetCustomerByAuthProviderIdAsync(authProviderId);
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

    private async Task<Result<Customer>> CreateCustomerEntityAsync(CustomerForCreationDto request, string qrCodeValue)
    {
        try
        {
            var customerId = Guid.NewGuid();
            var user = new Customer
            {
                Id = customerId,
                AuthProviderId = request.AuthProviderId,
                Email = request.Email,
                Name = request.Name,
                QrCodeValue = qrCodeValue,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateCustomerAsync(user);

            _logger.LogInformation(
                "Customer entity created. CustomerId: {CustomerId}, Email: {Email}, QrCodeValue: {QrCodeValue}",
                customerId, user.Email, user.QrCodeValue);

            // Save changes
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Transaction committed successfully. CustomerId: {CustomerId}",
                customerId);

            return Result<Customer>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create user. Email: {Email}, AuthProviderId: {AuthProviderId}, Error: {Error}",
                request.Email, request.AuthProviderId, ex.Message);
            return Result<Customer>.Failure(
                "An error occurred while creating the user");
        }
    }

    public async Task<Result<CustomerDashboardDto>> GetDashboardAsync(Guid customerId)
    {
        try
        {
            // 1. Validate JWT token matches customer's auth provider ID
            var authCheck = await _authorizationService.ValidateCustomerAuthorizationAsync(customerId);
            if (authCheck.IsFailure)
                return Result<CustomerDashboardDto>.Failure(authCheck.Error!);

            _logger.LogInformation("Building dashboard for CustomerId: {CustomerId}", customerId);

            // 2. Get all balances with business and reward details (only where balance > 0)
            var balances = await _repository.GetCustomerBalancesWithDetailsAsync(customerId);
            var balancesList = balances.ToList();

            // 3. Build Progress
            var currentTotalPoints = balancesList.Sum(b => b.Balance);
            var rewardsAvailable = balancesList.Count(b => b.Balance >= b.Reward.CostPoints);

            var progress = new CustomerProgressDto
            {
                CurrentTotalPoints = currentTotalPoints,
                RewardsAvailable = rewardsAvailable
            };

            // 4. Build Achievements
            var lifetimeRewardsClaimed = await _repository.GetLifetimeRewardsClaimedCountAsync(customerId);
            var totalPointsEarned = await _repository.GetLifetimePointsEarnedAsync(customerId);

            var achievements = new CustomerAchievementsDto
            {
                LifetimeRewardsClaimed = lifetimeRewardsClaimed,
                TotalPointsEarned = totalPointsEarned
            };

            // 5. Build Active Businesses (Top 3 by most recent scan)
            var businessesWithScanDates = new List<(CustomerActiveBusinessDto business, DateTime? lastScan)>();

            foreach (var balance in balancesList)
            {
                var lastScanDate = await _repository.GetMostRecentScanDateForBusinessAsync(
                    customerId, 
                    balance.Reward.BusinessId);

                var activeBusinessDto = new CustomerActiveBusinessDto
                {
                    Business = _mapper.Map<BusinessDto>(balance.Reward.Business),
                    Balance = balance.Balance,
                    CostPoints = balance.Reward.CostPoints,
                    RewardsAvailable = balance.Balance / balance.Reward.CostPoints
                };

                businessesWithScanDates.Add((activeBusinessDto, lastScanDate));
            }

            // Sort by most recent scan and take top 3
            var top3Businesses = businessesWithScanDates
                .OrderByDescending(x => x.lastScan ?? DateTime.MinValue)
                .Take(3)
                .Select(x => x.business)
                .ToList();

            // 6. Build Last 30 Days Stats
            var pointsEarned = await _repository.GetLast30DaysPointsEarnedAsync(customerId);
            var scansCompleted = await _repository.GetLast30DaysScansCountAsync(customerId);
            var rewardsClaimed = await _repository.GetLast30DaysRewardsClaimedCountAsync(customerId);

            var last30Days = new CustomerLast30DaysDto
            {
                PointsEarned = pointsEarned,
                ScansCompleted = scansCompleted,
                RewardsClaimed = rewardsClaimed
            };

            // 7. Build complete dashboard
            var dashboard = new CustomerDashboardDto
            {
                Progress = progress,
                Achievements = achievements,
                Top3Businesses = top3Businesses,
                Last30Days = last30Days
            };

            _logger.LogInformation(
                "Dashboard built successfully for CustomerId: {CustomerId}. TotalPoints: {TotalPoints}, RewardsAvailable: {RewardsAvailable}, Top3Businesses: {Top3Count}",
                customerId, currentTotalPoints, rewardsAvailable, top3Businesses.Count);

            return Result<CustomerDashboardDto>.Success(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to build dashboard for CustomerId: {CustomerId}, Error: {Error}",
                customerId, ex.Message);
            return Result<CustomerDashboardDto>.Failure(
                "An error occurred while loading your dashboard");
        }
    }

    public async Task<Result<bool>> DeleteCustomerAsync(Guid customerId)
    {
        try
        {
            _logger.LogInformation("Starting deletion process for CustomerId: {CustomerId}", customerId);

            // 1. Validate JWT token matches customer's auth provider ID
            var authCheck = await _authorizationService.ValidateCustomerAuthorizationAsync(customerId);
            if (authCheck.IsFailure)
                return Result<bool>.Failure(authCheck.Error!);

            // 2. Verify customer exists (already done in ValidateCustomerAuthorizationAsync, but get reference)
            var customer = await _repository.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                _logger.LogWarning("Customer not found for deletion. CustomerId: {CustomerId}", customerId);
                return Result<bool>.Failure("Customer not found");
            }

            // 3. Delete related records in order (most dependent first)
            // Delete customer_balance records
            await _repository.DeleteCustomerBalancesAsync(customerId);
            _logger.LogInformation("Deleted customer balances for CustomerId: {CustomerId}", customerId);

            // Delete reward_redemption records
            await _repository.DeleteRewardRedemptionsAsync(customerId);
            _logger.LogInformation("Deleted reward redemptions for CustomerId: {CustomerId}", customerId);

            // Delete scan_event records
            await _repository.DeleteScanEventsAsync(customerId);
            _logger.LogInformation("Deleted scan events for CustomerId: {CustomerId}", customerId);

            // 4. Finally delete the customer
            await _repository.DeleteCustomerAsync(customer);
            _logger.LogInformation("Deleted customer record for CustomerId: {CustomerId}", customerId);

            // 5. Save all changes in one transaction
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Customer deleted successfully. CustomerId: {CustomerId}, Email: {Email}, Name: {Name}",
                customerId, customer.Email, customer.Name);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to delete customer. CustomerId: {CustomerId}, Error: {Error}",
                customerId, ex.Message);
            return Result<bool>.Failure(
                "An error occurred while deleting the customer");
        }
    }
}
