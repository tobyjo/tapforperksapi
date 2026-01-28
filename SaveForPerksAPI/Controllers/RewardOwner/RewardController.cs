using Microsoft.AspNetCore.Mvc;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Services;

namespace SaveForPerksAPI.Controllers.RewardOwner
{
    [Route("api/reward-owner/{rewardOwnerId}/rewards")]
    public class RewardController : BaseApiController
    {
        private readonly IRewardManagementService _rewardManagementService;

        public RewardController(
            IRewardManagementService rewardManagementService,
            ILogger<RewardController> logger)
            : base(logger)
        {
            _rewardManagementService = rewardManagementService ?? throw new ArgumentNullException(nameof(rewardManagementService));
        }

        [HttpPost]
        public async Task<ActionResult<RewardDto>> CreateReward(
            Guid rewardOwnerId,
            RewardForCreationDto rewardForCreationDto)
        {
            Logger.LogInformation(
                "CreateReward called with RewardOwnerId: {RewardOwnerId}, Name: {Name}, Type: {Type}, CostPoints: {CostPoints}",
                rewardOwnerId, 
                rewardForCreationDto.Name,
                rewardForCreationDto.RewardType,
                rewardForCreationDto.CostPoints);

            // Override the RewardOwnerId from the route parameter
            rewardForCreationDto.RewardOwnerId = rewardOwnerId;

            return await ExecuteAsync(
                () => _rewardManagementService.CreateRewardAsync(rewardForCreationDto),
                nameof(CreateReward));
        }

        // CRUD endpoints for Reward will be added here
    }
}
