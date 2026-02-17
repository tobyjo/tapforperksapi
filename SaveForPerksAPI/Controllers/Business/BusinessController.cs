using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Services;

namespace SaveForPerksAPI.Controllers.Business
{
    [Route("api/business")]
    [Authorize]
    public class BusinessController : BaseApiController
    {
        private readonly IBusinessService _businessService;

        public BusinessController(
            IBusinessService businessService,
            ILogger<BusinessController> logger)
            : base(logger)
        {
            _businessService = businessService ?? throw new ArgumentNullException(nameof(businessService));
        }

        [HttpPost]
        public async Task<ActionResult<BusinessWithAdminUserResponseDto>> CreateBusinessWithAdminUser(
            BusinessWithAdminUserForCreationDto businessWithAdminUserForCreationDto)
        {
            Logger.LogInformation(
                "CreateBusiness called with BusinessName: {BusinessName}, Email: {Email}, AuthProviderId: {AuthProviderId}",
                businessWithAdminUserForCreationDto.BusinessName, 
                businessWithAdminUserForCreationDto.BusinessUserEmail,
                businessWithAdminUserForCreationDto.BusinessUserAuthProviderId);

            return await ExecuteAsync(
                () => _businessService.CreateBusinessAsync(businessWithAdminUserForCreationDto),
                nameof(CreateBusinessWithAdminUser));
        }

    }
}
