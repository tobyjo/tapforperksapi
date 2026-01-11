using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TapForPerksAPI.Models;
using TapForPerksAPI.Repositories;

namespace TapForPerksAPI.Controllers.LoyaltyOwner
{
    [ApiController]
    [Route("api/lo/scans")]
    public class LoyaltyOwnerScanController : ControllerBase
    {
        private readonly ISaveForPerksRepository saveForPerksRepository;
        private readonly IMapper mapper;

    
        public LoyaltyOwnerScanController(ISaveForPerksRepository saveForPerksRepository, IMapper mapper)
        {
            this.saveForPerksRepository = saveForPerksRepository ?? throw new ArgumentNullException(nameof(saveForPerksRepository));
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

        [HttpPost]
        public async Task<ActionResult<ScanEventDto>> CreateScanEventForReward(ScanEventForCreationDto scanEventForCreationDto)
        {
            var scanEventEntity = mapper.Map<Entities.ScanEvent>(scanEventForCreationDto);

            // Lookup user_id for this qrcode_value
            var userEntity = await saveForPerksRepository.GetUserByQrCodeValueAsync(scanEventEntity.QrCodeValue);
            if (userEntity == null)
            {
                return NotFound("User not found");
            }

            scanEventEntity.UserId = userEntity.Id;

            // Get reward to validate it exists and to see how to create or update the user_balance
            var rewardEntity = await saveForPerksRepository.GetRewardAsync(scanEventEntity.RewardId);
            if(rewardEntity == null) {
                return NotFound("Reward not found");
            }

            // Update or create user_balance
            var userBalanceEntity = await saveForPerksRepository.GetUserBalanceAsync(userEntity.Id, rewardEntity.Id);
            if (userBalanceEntity == null) {
                // Create new user_balance
                userBalanceEntity = new Entities.UserBalance
                {
                    Id = Guid.NewGuid(),
                    UserId = userEntity.Id,
                    RewardId = rewardEntity.Id,
                    Balance = scanEventEntity.PointsChange,
                    LastUpdated = DateTime.UtcNow
                };

       
                await saveForPerksRepository.CreateUserBalance(userBalanceEntity);
            }
            else
            {
                // Update existing user_balance
                userBalanceEntity.Balance += scanEventEntity.PointsChange;
                userBalanceEntity.LastUpdated = DateTime.UtcNow;
            }




            await saveForPerksRepository.CreateScanEvent(scanEventEntity);
            await saveForPerksRepository.SaveChangesAsync();

            var scanEventToReturn = mapper.Map<ScanEventDto>(scanEventEntity);

            // Declare response object here - outside all if blocks
            var scanEventResponse = new ScanEventResponseDto
            {
                ScanEvent = scanEventToReturn,
                CurrentBalance = userBalanceEntity.Balance,
                RewardAvailable = false,  // Default to false
                AvailableReward = null,
                TimesClaimable = 0
            };


            // Check if reward is available based on reward type
            if (rewardEntity.RewardType == Entities.RewardType.IncrementalPoints)
            {
                if (userBalanceEntity.Balance >= rewardEntity.CostPoints)
                {
                    int timesClaimable = userBalanceEntity.Balance / (rewardEntity.CostPoints ?? 1);

                    scanEventResponse.RewardAvailable = true;
                    scanEventResponse.AvailableReward = new AvailableRewardDto
                    {
                        RewardId = rewardEntity.Id,
                        RewardName = rewardEntity.Name,
                        RewardType = "incremental_points",
                        RequiredPoints = rewardEntity.CostPoints ?? 0
                    };
                    scanEventResponse.TimesClaimable = timesClaimable;
                }
            }

            return CreatedAtRoute("GetScanEventForReward",
                new { rewardId = scanEventToReturn.RewardId, scanEventId = scanEventToReturn.Id },
                scanEventResponse);
        }

        [HttpGet("History")]
        public async Task<ActionResult<IEnumerable<LoyaltyOwnerDto>>> GetScansHistoryForReward()
        {
            /*
            var loyaltyOwners = await tapForPerksRepository.GetLoyaltyOwnersAsync();
            var results = mapper.Map<IEnumerable<LoyaltyOwnerDto>>(loyaltyOwners);
            */
            return Ok(true);
        }
    }
}
