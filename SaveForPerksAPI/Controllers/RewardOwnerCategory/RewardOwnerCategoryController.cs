using Microsoft.AspNetCore.Mvc;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Services;

namespace SaveForPerksAPI.Controllers.RewardOwnerCategory
{
    [Route("api/reward-owner-category")]
    public class RewardOwnerCategoryController : BaseApiController
    {
        private readonly IRewardOwnerCategoryService _rewardOwnerCategoryService;

        public RewardOwnerCategoryController(
            IRewardOwnerCategoryService rewardOwnerCategoryService,
            ILogger<RewardOwnerCategoryController> logger)
            : base(logger)
        {
            _rewardOwnerCategoryService = rewardOwnerCategoryService ?? throw new ArgumentNullException(nameof(rewardOwnerCategoryService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RewardOwnerCategoryDto>>> GetAllCategories()
        {
            Logger.LogInformation("GetAllCategories called");

            return await ExecuteAsync(
                () => _rewardOwnerCategoryService.GetAllCategoriesAsync(),
                nameof(GetAllCategories));
        }
    }
}
