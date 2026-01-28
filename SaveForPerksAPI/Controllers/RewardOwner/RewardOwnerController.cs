using Microsoft.AspNetCore.Mvc;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Services;

namespace SaveForPerksAPI.Controllers.RewardOwner
{
    [Route("api/reward-owner")]
    public class RewardOwnerController : BaseApiController
    {
        private readonly IRewardTransactionService _rewardTransactionService;

        public RewardOwnerController(
            IRewardTransactionService rewardTransactionService,
            ILogger<RewardOwnerScanController> logger)
            : base(logger)
        {
            _rewardTransactionService = rewardTransactionService ?? throw new ArgumentNullException(nameof(rewardTransactionService));
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
                () => _rewardTransactionService.CreateRewardOwnerAsync(rewardOwnerWithAdminUserForCreationDto),
                nameof(CreateRewardOwnerWithAdminUser));
        }

    }
}
