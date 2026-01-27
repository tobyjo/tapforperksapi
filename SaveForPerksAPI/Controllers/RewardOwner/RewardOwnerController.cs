using Microsoft.AspNetCore.Mvc;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Services;

namespace SaveForPerksAPI.Controllers.RewardOwner
{
    [Route("api/reward-owner")]
    public class RewardOwnerController : BaseApiController
    {
        private readonly IRewardService rewardService;

        public RewardOwnerController(
            IRewardService rewardService,
            ILogger<RewardOwnerScanController> logger)
            : base(logger)
        {
            this.rewardService = rewardService ?? throw new ArgumentNullException(nameof(rewardService));
        }

        [HttpPost]
        public async Task<ActionResult<RewardOwnerWithAdminUserResponseDto>> CreateRewardOwnerWithAdminUser(
            RewardOwnerWithAdminUserForCreationDto rewardOwnerWithAdminUserForCreationDto)
        {
            Logger.LogInformation(
                "CreateRewardOwner called with RewardOwnerName: {RewardOwnerName}, Email: {Email}, AuthProviderId: {AuthProviderId}",
                rewardOwnerWithAdminUserForCreationDto.RewardOwnerName, 
                rewardOwnerWithAdminUserForCreationDto.RewardOwnerUserEmail,
                rewardOwnerWithAdminUserForCreationDto.RewardOwnerUserAuthProviderId);

            return await ExecuteAsync(
                () => rewardService.CreateRewardOwnerAsync(rewardOwnerWithAdminUserForCreationDto),
                nameof(CreateRewardOwnerWithAdminUser));
        }

    }
}
