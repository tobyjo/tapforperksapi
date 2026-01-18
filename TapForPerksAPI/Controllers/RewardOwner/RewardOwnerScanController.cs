using Microsoft.AspNetCore.Mvc;
using TapForPerksAPI.Models;
using TapForPerksAPI.Services;

namespace TapForPerksAPI.Controllers.RewardOwner
{
    [Route("api/reward-owner/scans")]
    public class RewardOwnerScanController : BaseApiController
    {
        private readonly IRewardService rewardService;

        public RewardOwnerScanController(
            IRewardService rewardService, 
            ILogger<RewardOwnerScanController> logger)
            : base(logger)
        {
            this.rewardService = rewardService ?? throw new ArgumentNullException(nameof(rewardService));
        }

        [HttpGet("{rewardId}/events/{scanEventId}", Name = "GetScanEventForReward")]
        public async Task<ActionResult<ScanEventDto>> GetScanEventForReward(Guid rewardId, Guid scanEventId)
        {
            return await ExecuteAsync(
                () => rewardService.GetScanEventForRewardAsync(rewardId, scanEventId),
                nameof(GetScanEventForReward));
        }

        [HttpGet("{rewardId}/userbalance/{qrCodeValue}", Name = "GetUserBalanceForReward")]
        public async Task<ActionResult<UserBalanceAndInfoResponseDto>> GetUserBalanceForReward(
            Guid rewardId, 
            string qrCodeValue)
        {
            return await ExecuteAsync(
                () => rewardService.GetUserBalanceForRewardAsync(rewardId, qrCodeValue),
                nameof(GetUserBalanceForReward));
        }

        [HttpPost]
        public async Task<ActionResult<ScanEventResponseDto>> CreatePointsAndClaimRewards(
            ScanEventForCreationDto scanEventForCreationDto)
        {
            return await ExecuteCreatedAsync(
                () => rewardService.ProcessScanAndRewardsAsync(scanEventForCreationDto),
                "GetScanEventForReward",
                v => new { rewardId = v.ScanEvent.RewardId, scanEventId = v.ScanEvent.Id },
                nameof(CreatePointsAndClaimRewards));
        }

        [HttpGet("History")]
        public Task<ActionResult<IEnumerable<RewardOwnerDto>>> GetScansHistoryForReward()
        {
            return Task.FromResult<ActionResult<IEnumerable<RewardOwnerDto>>>(Ok(true));
        }
    }
}

