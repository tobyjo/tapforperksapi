using SaveForPerksAPI.Common;
using SaveForPerksAPI.Models;

namespace SaveForPerksAPI.Services;

public interface IRewardTransactionService
{
    Task<Result<RewardOwnerWithAdminUserResponseDto>> CreateRewardOwnerAsync(
        RewardOwnerWithAdminUserForCreationDto rewardOwnerWithAdminUserForCreationDto);

    Task<Result<ScanEventResponseDto>> ProcessScanAndRewardsAsync(
        ScanEventForCreationDto request);
    
    Task<Result<UserBalanceAndInfoResponseDto>> GetUserBalanceForRewardAsync(
        Guid rewardId, 
        string qrCodeValue);
    
    Task<Result<ScanEventDto>> GetScanEventForRewardAsync(
        Guid rewardId, 
        Guid scanEventId);
}
