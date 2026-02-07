using SaveForPerksAPI.Common;
using SaveForPerksAPI.Models;

namespace SaveForPerksAPI.Services;

public interface IRewardOwnerCategoryService
{
    Task<Result<IEnumerable<RewardOwnerCategoryDto>>> GetAllCategoriesAsync();
}
