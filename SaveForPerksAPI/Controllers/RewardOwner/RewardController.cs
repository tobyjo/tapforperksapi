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
            [FromBody] RewardForCreationDto rewardForCreationDto,
            [FromHeader(Name = "X-RewardOwnerUser-Id")] Guid rewardOwnerUserId)
        {
            Logger.LogInformation(
                "CreateReward called with RewardOwnerId: {RewardOwnerId}, Name: {Name}, Type: {Type}, CostPoints: {CostPoints}, UserId: {UserId}",
                rewardOwnerId, 
                rewardForCreationDto.Name,
                rewardForCreationDto.RewardType,
                rewardForCreationDto.CostPoints,
                rewardOwnerUserId);

            // Override the RewardOwnerId from the route parameter
            rewardForCreationDto.RewardOwnerId = rewardOwnerId;

            return await ExecuteAsync(
                () => _rewardManagementService.CreateRewardAsync(rewardForCreationDto, rewardOwnerUserId),
                nameof(CreateReward));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RewardDto>>> GetRewards(
            Guid rewardOwnerId,
            [FromHeader(Name = "X-RewardOwnerUser-Id")] Guid rewardOwnerUserId)
        {
            Logger.LogInformation(
                "GetRewards called with RewardOwnerId: {RewardOwnerId}, UserId: {UserId}",
                rewardOwnerId,
                rewardOwnerUserId);

            return await ExecuteAsync(
                () => _rewardManagementService.GetRewardsByRewardOwnerIdAsync(rewardOwnerId, rewardOwnerUserId),
                nameof(GetRewards));
        }

        // CRUD endpoints for Reward will be added here
    }
}
