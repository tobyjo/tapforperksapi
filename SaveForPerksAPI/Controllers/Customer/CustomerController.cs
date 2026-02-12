using Microsoft.AspNetCore.Mvc;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Services;

namespace SaveForPerksAPI.Controllers.Customer
{
    [Route("api/customer")]
    public class CustomerController : BaseApiController
    {
        private readonly ICustomerService _customerService;

        public CustomerController(
            ICustomerService customerService,
            ILogger<CustomerController> logger)
            : base(logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        [HttpGet("by-auth/{authProviderId}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerByAuthProviderId(string authProviderId)
        {
            Logger.LogInformation(
                "GetCustomerByAuthProviderId called with AuthProviderId: {AuthProviderId}",
                authProviderId);

            return await ExecuteAsync(
                () => _customerService.GetCustomerByAuthProviderIdAsync(authProviderId),
                nameof(GetCustomerByAuthProviderId));
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CustomerForCreationDto customerForCreationDto)
        {
            Logger.LogInformation(
                "CreateCustomer called with Email: {Email}, AuthProviderId: {AuthProviderId}, Name: {Name}",
                customerForCreationDto.Email,
                customerForCreationDto.AuthProviderId,
                customerForCreationDto.Name);

            return await ExecuteAsync(
                () => _customerService.CreateCustomerAsync(customerForCreationDto),
                nameof(CreateCustomer));
        }

        [HttpGet("{customerId}/dashboard")]
        public async Task<ActionResult<CustomerDashboardDto>> GetDashboard(Guid customerId)
        {
            Logger.LogInformation(
                "GetDashboard called for CustomerId: {CustomerId}",
                customerId);

            return await ExecuteAsync(
                () => _customerService.GetDashboardAsync(customerId),
                nameof(GetDashboard));
        }

        [HttpDelete("{customerId}")]
        public async Task<ActionResult> DeleteCustomer(Guid customerId)
        {
            Logger.LogInformation(
                "DeleteCustomer called for CustomerId: {CustomerId}",
                customerId);

            var result = await _customerService.DeleteCustomerAsync(customerId);

            if (result.IsSuccess)
            {
                return NoContent(); // 204 No Content - successful deletion
            }

            if (result.Error == "Customer not found")
            {
                return NotFound(result.Error); // 404 Not Found
            }

            return BadRequest(result.Error); // 400 Bad Request for other errors
        }
    }
}
