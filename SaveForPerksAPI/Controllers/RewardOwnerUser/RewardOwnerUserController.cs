using Microsoft.AspNetCore.Mvc;
using SaveForPerksAPI.Controllers.RewardOwner;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Services;

namespace SaveForPerksAPI.Controllers.RewardOwnerUser
{
    [Route("api/reward-owner-user")]
    public class RewardOwnerUserController : BaseApiController
    {
        private readonly IRewardOwnerUserService _rewardOwnerUserService;

        public RewardOwnerUserController(
           IRewardOwnerUserService rewardOwnerUserService,
           ILogger<RewardOwnerUserController> logger)
           : base(logger)
        {
            _rewardOwnerUserService = rewardOwnerUserService ?? throw new ArgumentNullException(nameof(rewardOwnerUserService));
        }

        [HttpGet("{authProviderId}/reward-owners")]
        public async Task<ActionResult<IEnumerable<RewardOwnerDto>>> GetRewardOwnersByAuthProviderId(string authProviderId)
        {
            Logger.LogInformation(
                "GetRewardOwnersByAuthProviderId called with AuthProviderId: {AuthProviderId}",
                authProviderId);

            return await ExecuteAsync(
                () => _rewardOwnerUserService.GetRewardOwnersByAuthProviderIdAsync(authProviderId),
                nameof(GetRewardOwnersByAuthProviderId));
        }


    }
}
