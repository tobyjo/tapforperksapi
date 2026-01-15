using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TapForPerksAPI.Models;
using TapForPerksAPI.Repositories;
using TapForPerksAPI.Services;

namespace TapForPerksAPI.Controllers.RewardOwner
{
    [ApiController]
    [Route("api/reward-owner/scans")]
    public class RewardOwnerScanController : ControllerBase
    {
        private readonly ISaveForPerksRepository saveForPerksRepository;
        private readonly IRewardService rewardService;
        private readonly IMapper mapper;

    
        public RewardOwnerScanController(
            ISaveForPerksRepository saveForPerksRepository, 
            IRewardService rewardService,
            IMapper mapper)
        {
            this.saveForPerksRepository = saveForPerksRepository ?? throw new ArgumentNullException(nameof(saveForPerksRepository));
            this.rewardService = rewardService ?? throw new ArgumentNullException(nameof(rewardService));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("{rewardId}/events/{scanEventId}", Name = "GetScanEventForReward")]
        public async Task<ActionResult<ScanEventDto>> GetScanEventForReward(Guid rewardId, Guid scanEventId)
        {
            var scanEventEntity = await saveForPerksRepository.GetScanEventAsync(rewardId, scanEventId);
            if (scanEventEntity == null)
            {
                return NotFound();
            }
            var scanEventToReturn = mapper.Map<ScanEventDto>(scanEventEntity);
            return Ok(scanEventToReturn);
        }

        [HttpGet("{rewardId}/userbalance/{qrCodeValue}", Name = "GetUserBalanceForReward")]
        public async Task<ActionResult<UserBalanceAndInfoResponseDto>> GetUserBalanceForReward(Guid rewardId, string qrCodeValue)
        {
            // Lookup user_id for this qrcode_value
            var userEntity = await saveForPerksRepository.GetUserByQrCodeValueAsync(qrCodeValue);
            if (userEntity == null)
            {
                return NotFound("User not found");
            }

            // Get reward to validate it exists and to see how to create or update the user_balance
            var rewardEntity = await saveForPerksRepository.GetRewardAsync(rewardId);
            if (rewardEntity == null)
            {
                return NotFound("Reward not found");
            }

            var userBalanceAndInfo = new UserBalanceAndInfoResponseDto
            {
                QrCodeValue = qrCodeValue,
                UserName = userEntity.Name,
                CurrentBalance = 0,
                AvailableReward = null,
                TimesClaimable = 0
            };

            var userPointsInfo = await saveForPerksRepository.GetUserBalanceForRewardAsync(userEntity.Id, rewardId);
            if (userPointsInfo == null)
            {
                // No balance found (no scans made) for this reward by this user
                return Ok(userBalanceAndInfo);
            }
            userBalanceAndInfo.CurrentBalance = userPointsInfo.Balance;

 

            // Check if reward is available based on reward type
            if (rewardEntity.RewardType == Entities.RewardType.IncrementalPoints)
            {
                if (rewardEntity.CostPoints > 0 && userPointsInfo.Balance >= rewardEntity.CostPoints)
                {
                    int timesClaimable = userPointsInfo.Balance / rewardEntity.CostPoints;

                    userBalanceAndInfo.AvailableReward = new AvailableRewardDto
                    {
                        RewardId = rewardEntity.Id,
                        RewardName = rewardEntity.Name,
                        RewardType = "incremental_points",
                        RequiredPoints = rewardEntity.CostPoints
                    };
                    userBalanceAndInfo.TimesClaimable = timesClaimable;
                }
            }

            return Ok(userBalanceAndInfo);
        }

        [HttpPost]
        public async Task<ActionResult<ScanEventResponseDto>> CreatePointsAndClaimRewards(
            ScanEventForCreationDto scanEventForCreationDto)
        {
            var result = await rewardService.ProcessScanAndRewardsAsync(scanEventForCreationDto);
            
            if (result.IsFailure)
                return BadRequest(result.Error);

            return CreatedAtRoute("GetScanEventForReward",
                new { rewardId = result.Value!.ScanEvent.RewardId, scanEventId = result.Value.ScanEvent.Id },
                result.Value);
        }

        [HttpGet("History")]
        public async Task<ActionResult<IEnumerable<RewardOwnerDto>>> GetScansHistoryForReward()
        {
       
            return Ok(true);
        }
    }
}
