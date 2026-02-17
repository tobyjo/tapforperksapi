using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Services;

namespace SaveForPerksAPI.Controllers.Business
{
    [Route("api/business/{businessId}/rewards")]
    [Authorize]
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
            Guid businessId,
            [FromBody] RewardForCreationDto rewardForCreationDto,
            [FromHeader(Name = "X-BusinessUser-Id")] Guid businessUserId)
        {
            Logger.LogInformation(
                "CreateReward called with BusinessId: {BusinessId}, Name: {Name}, Type: {Type}, CostPoints: {CostPoints}, BusinessUserId: {BusinessUserId}",
                businessId, 
                rewardForCreationDto.Name,
                rewardForCreationDto.RewardType,
                rewardForCreationDto.CostPoints,
                businessUserId);

            // Override the RewardOwnerId from the route parameter
            rewardForCreationDto.BusinessId = businessId;

            return await ExecuteAsync(
                () => _rewardManagementService.CreateRewardAsync(rewardForCreationDto, businessUserId),
                nameof(CreateReward));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RewardDto>>> GetRewards(
            Guid businessId,
            [FromHeader(Name = "X-BusinessUser-Id")] Guid businessUserId)
        {
            Logger.LogInformation(
                "GetRewards called with BusinessId: {BusinessId}, BusinessUserId: {BusinessUserId}",
                businessId,
                businessUserId);

            return await ExecuteAsync(
                () => _rewardManagementService.GetRewardsByBusinessIdAsync(businessId, businessUserId),
                nameof(GetRewards));
        }

        [HttpPut("{rewardId}")]
        public async Task<ActionResult<RewardDto>> UpdateReward(
            Guid businessId,
            Guid rewardId,
            [FromBody] RewardForUpdateDto rewardForUpdateDto,
            [FromHeader(Name = "X-BusinessUser-Id")] Guid businessUserId)
        {
            Logger.LogInformation(
                "UpdateReward called with BusinessId: {BusinessId}, RewardId: {RewardId}, BusinessUserId: {BusinessUserId}",
                businessId,
                rewardId,
                businessUserId);

            return await ExecuteAsync(
                () => _rewardManagementService.UpdateRewardAsync(rewardId, rewardForUpdateDto, businessUserId),
                nameof(UpdateReward));
        }

        // CRUD endpoints for Reward will be added here
    }
}
