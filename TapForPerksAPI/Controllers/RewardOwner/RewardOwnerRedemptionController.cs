using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TapForPerksAPI.Entities;
using TapForPerksAPI.Models;
using TapForPerksAPI.Repositories;

namespace TapForPerksAPI.Controllers.RewardOwner
{
    [ApiController]
    [Route("api/reward-owner/redemption")]
    public class RewardOwnerRedemptionController : ControllerBase
    {
        private readonly ISaveForPerksRepository saveForPerksRepository;
        private readonly IMapper mapper;


        public RewardOwnerRedemptionController(ISaveForPerksRepository saveForPerksRepository, IMapper mapper)
        {
            this.saveForPerksRepository = saveForPerksRepository ?? throw new ArgumentNullException(nameof(saveForPerksRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("{rewardId}/{rewardRedemptionId}", Name = "GetRewardRedemption")]
        public async Task<ActionResult<RewardRedemptionDto>> GetRewardRedemption(Guid rewardId, Guid rewardRedemptionId)
        {
            var rewardRedemptionEntity = await saveForPerksRepository.GetRewardRedemptionAsync(rewardId, rewardRedemptionId);
            if (rewardRedemptionEntity == null)
            {
                return NotFound();
            }
            var rewardRedemptionToReturn = mapper.Map<RewardRedemptionDto>(rewardRedemptionEntity);
            return Ok(rewardRedemptionToReturn);
        }

        [HttpPost]
        public async Task<ActionResult<RewardRedemptionDto>> CreateRedemption(RewardRedemptionForCreationDto rewardRedemptionForCreationDto)
        {
            var rewardRedemptionEntity = mapper.Map<Entities.RewardRedemption>(rewardRedemptionForCreationDto);

            // Lookup user_id for this qrcode_value
            var userEntity = await saveForPerksRepository.GetUserByQrCodeValueAsync(rewardRedemptionForCreationDto.QrCodeValue);
            if (userEntity == null)
            {
                return NotFound("User not found");
            }

            // Get reward to validate it exists and to see how to create or update the user_balance
            var rewardEntity = await saveForPerksRepository.GetRewardAsync(rewardRedemptionForCreationDto.RewardId);
            if (rewardEntity == null)
            {
                return NotFound("Reward not found");
            }

            // Declare response object here - outside all if blocks
            var rewardRedemptionResponse = new RewardRedemptionResponseDto
            {
                UserName = userEntity.Name,
                CurrentBalance = 0 // Will be updated below if applicable
            };

            if ( rewardEntity.RewardType == RewardType.IncrementalPoints)
            {
                // Get user balance for this reward owner and user
                var userBalanceEntity = await saveForPerksRepository.GetUserBalanceForRewardAsync(userEntity.Id, rewardRedemptionForCreationDto.RewardId);
                if( userBalanceEntity == null || userBalanceEntity.Balance < (rewardEntity.CostPoints * rewardRedemptionForCreationDto.NumRewardsToClaim))
                {
                    return BadRequest("Insufficient points to redeem this reward");
                }
                // Deduct points from user balance
                userBalanceEntity.Balance -= (rewardEntity.CostPoints * rewardRedemptionForCreationDto.NumRewardsToClaim);

                // await saveForPerksRepository.UpdateUserBalance(userBalanceEntity);
                await saveForPerksRepository.SaveChangesAsync();

                rewardRedemptionResponse.CurrentBalance = userBalanceEntity.Balance;
            }
            else
            {
                return BadRequest("Reward type not yet configured");
            }

            return CreatedAtRoute("GetRewardRedemption",
                new
                {
                    rewardId = rewardRedemptionEntity.RewardId,
                    rewardRedemptionId = rewardRedemptionEntity.Id
                },
                rewardRedemptionResponse);

        }
    }
}
