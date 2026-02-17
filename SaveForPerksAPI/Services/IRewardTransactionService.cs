using SaveForPerksAPI.Common;
using SaveForPerksAPI.Models;

namespace SaveForPerksAPI.Services;

public interface IRewardTransactionService
{
    Task<Result<ScanEventResponseDto>> ProcessScanAndRewardsAsync(
        Guid businessId,
        Guid businessUserId,
        ScanEventForCreationDto request);

    Task<Result<CustomerBalanceAndInfoResponseDto>> GetCustomerBalanceForRewardAsync(
        Guid businessId,
        Guid rewardId, 
        string qrCodeValue,
        Guid businessUserId);

    Task<Result<ScanEventDto>> GetScanEventForRewardAsync(
        Guid businessId,
        Guid rewardId, 
        Guid scanEventId,
        Guid businessUserId);
}
