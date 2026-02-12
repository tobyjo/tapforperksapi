using SaveForPerksAPI.Common;
using SaveForPerksAPI.Models;

namespace SaveForPerksAPI.Services;

public interface ICustomerService
{
    Task<Result<CustomerDto>> GetCustomerByAuthProviderIdAsync(string authProviderId);

    Task<Result<CustomerDto>> CreateCustomerAsync(CustomerForCreationDto request);

    Task<Result<CustomerDashboardDto>> GetDashboardAsync(Guid customerId);

    Task<Result<bool>> DeleteCustomerAsync(Guid customerId);
}
