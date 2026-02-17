using Microsoft.AspNetCore.Mvc;
using SaveForPerksAPI.Controllers.Business;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace SaveForPerksAPI.Controllers.BusinessUser
{
    [Route("api/business-user")]
    [Authorize]
    public class BusinessUserController : BaseApiController
    {
        private readonly IBusinessUserService _businessUserService;

        public BusinessUserController(
           IBusinessUserService businessUserService,
           ILogger<BusinessUserController> logger)
           : base(logger)
        {
            _businessUserService = businessUserService ?? throw new ArgumentNullException(nameof(businessUserService));
        }


        // Combines GetBusinessesByAuthProviderId and GetBusinessUserByAuthProviderId into one call to just get details on the BusinessUser
        // and the Business they belong to
        [HttpGet("profile/{authProviderId}")]
        public async Task<ActionResult<IEnumerable<BusinessUserProfileResponseDto>>> GetBusinessUserProfilesByAuthProviderId(string authProviderId)
        {
            Logger.LogInformation(
                "GetBusinessUserProfilesByAuthProviderId called with AuthProviderId: {AuthProviderId}",
                authProviderId);

            return await ExecuteAsync(
                () => _businessUserService.GetBusinessUserProfilesByAuthProviderIdAsync(authProviderId),
                nameof(GetBusinessUserProfilesByAuthProviderId));
        }



        /* The following could be deprecated and not used ****************/
        /*
        [HttpGet("{authProviderId}/businesses")]
        public async Task<ActionResult<IEnumerable<BusinessDto>>> GetBusinessesByAuthProviderId(string authProviderId)
        {
            Logger.LogInformation(
                "GetBusinessesByAuthProviderId called with AuthProviderId: {AuthProviderId}",
                authProviderId);

            return await ExecuteAsync(
                () => _businessUserService.GetBusinessesByAuthProviderIdAsync(authProviderId),
                nameof(GetBusinessesByAuthProviderId));
        }

        [HttpGet("by-auth/{authProviderId}")]
        public async Task<ActionResult<BusinessUserDto>> GetBusinessUserByAuthProviderId(string authProviderId)
        {
            Logger.LogInformation(
                "GetBusinessUserByAuthProviderId called with AuthProviderId: {AuthProviderId}",
                authProviderId);

            return await ExecuteAsync(
                () => _businessUserService.GetBusinessUserByAuthProviderIdAsync(authProviderId),
                nameof(GetBusinessUserByAuthProviderId));
        }
        */

    }
}
