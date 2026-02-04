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


        // Combines GetRewardOwnersByAuthProviderId and GetRewardOwnerUserByAuthProviderId into one call to just get details on the RewardOwnerUser
        // and the RewardOwner they belong to
        [HttpGet("profile/{authProviderId}")]
        public async Task<ActionResult<IEnumerable<RewardOwnerUserProfileResponseDto>>> GetRewardOwnerUserProfilesByAuthProviderId(string authProviderId)
        {
            Logger.LogInformation(
                "GetRewardOwnerUserProfilesByAuthProviderId called with AuthProviderId: {AuthProviderId}",
                authProviderId);

            return await ExecuteAsync(
                () => _rewardOwnerUserService.GetRewardOwnerUserProfilesByAuthProviderIdAsync(authProviderId),
                nameof(GetRewardOwnerUserProfilesByAuthProviderId));
        }



        /* The following could be deprecated and not used ****************/

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

        [HttpGet("by-auth/{authProviderId}")]
        public async Task<ActionResult<RewardOwnerUserDto>> GetRewardOwnerUserByAuthProviderId(string authProviderId)
        {
            Logger.LogInformation(
                "GetRewardOwnerUserByAuthProviderId called with AuthProviderId: {AuthProviderId}",
                authProviderId);

            return await ExecuteAsync(
                () => _rewardOwnerUserService.GetRewardOwnerUserByAuthProviderIdAsync(authProviderId),
                nameof(GetRewardOwnerUserByAuthProviderId));
        }


    }
}
