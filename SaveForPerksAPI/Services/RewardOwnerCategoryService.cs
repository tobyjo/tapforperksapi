using AutoMapper;
using SaveForPerksAPI.Common;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Repositories;

namespace SaveForPerksAPI.Services;

public class RewardOwnerCategoryService : IRewardOwnerCategoryService
{
    private readonly ISaveForPerksRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<RewardOwnerCategoryService> _logger;

    public RewardOwnerCategoryService(
        ISaveForPerksRepository repository,
        IMapper mapper,
        ILogger<RewardOwnerCategoryService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<RewardOwnerCategoryDto>>> GetAllCategoriesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all reward owner categories");

            var categories = await _repository.GetAllRewardOwnerCategoriesAsync();

            var categoryDtos = _mapper.Map<IEnumerable<RewardOwnerCategoryDto>>(categories);

            _logger.LogInformation(
                "Retrieved {Count} reward owner categories",
                categoryDtos.Count());

            return Result<IEnumerable<RewardOwnerCategoryDto>>.Success(categoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to retrieve reward owner categories. Error: {Error}",
                ex.Message);
            return Result<IEnumerable<RewardOwnerCategoryDto>>.Failure(
                "An error occurred while retrieving categories");
        }
    }
}
