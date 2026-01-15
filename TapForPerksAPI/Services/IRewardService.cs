using TapForPerksAPI.Common;
using TapForPerksAPI.Models;

namespace TapForPerksAPI.Services;

public interface IRewardService
{
    Task<Result<ScanEventResponseDto>> ProcessScanAndRewardsAsync(
        ScanEventForCreationDto request);
}
