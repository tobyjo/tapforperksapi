using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaveForPerksAPI.Entities;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Repositories;

namespace SaveForPerksAPI.Controllers.Business
{
    [ApiController]
    [Route("api/business/redemption")]
    [Authorize]
    public class BusinessRedemptionController : ControllerBase
    {
        private readonly ISaveForPerksRepository saveForPerksRepository;
        private readonly IMapper mapper;


        public BusinessRedemptionController(ISaveForPerksRepository saveForPerksRepository, IMapper mapper)
        {
            this.saveForPerksRepository = saveForPerksRepository ?? throw new ArgumentNullException(nameof(saveForPerksRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

    /** Might need GET to get rewards but I think rewards are now redeemed in BusinessScanCOntroller

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
            var customerEntity = await saveForPerksRepository.GetCustomerByQrCodeValueAsync(rewardRedemptionForCreationDto.QrCodeValue);
            if (customerEntity == null)
            {
                return NotFound("Customer not found");
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
                CustomerName = customerEntity.Name,
                CurrentBalance = 0 // Will be updated below if applicable
            };

            if ( rewardEntity.RewardType == RewardType.IncrementalPoints)
            {
                // Get user balance for this reward owner and user
                var customerBalanceEntity = await saveForPerksRepository.GetCustomerBalanceForRewardAsync(customerEntity.Id, rewardRedemptionForCreationDto.RewardId);
                if( customerBalanceEntity == null || customerBalanceEntity.Balance < (rewardEntity.CostPoints * rewardRedemptionForCreationDto.NumRewardsToClaim))
                {
                    return BadRequest("Insufficient points to redeem this reward");
                }
                // Deduct points from user balance
                customerBalanceEntity.Balance -= (rewardEntity.CostPoints * rewardRedemptionForCreationDto.NumRewardsToClaim);

                // await saveForPerksRepository.UpdateCustomerBalance(customerBalanceEntity);
                await saveForPerksRepository.SaveChangesAsync();

                rewardRedemptionResponse.CurrentBalance = customerBalanceEntity.Balance;
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
    */
    }
}
