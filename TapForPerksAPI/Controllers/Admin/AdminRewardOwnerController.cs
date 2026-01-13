using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TapForPerksAPI.Models;
using TapForPerksAPI.Repositories;

namespace TapForPerksAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin")]
    public class AdminRewardOwnerController : ControllerBase
    {
        private readonly ISaveForPerksRepository tapForPerksRepository;
        private readonly IMapper mapper;

        public AdminRewardOwnerController(ISaveForPerksRepository tapForPerksRepository, IMapper mapper)
        {
            this.tapForPerksRepository = tapForPerksRepository ?? throw new ArgumentNullException(nameof(tapForPerksRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("GetRewardOwners")]
        public async Task<ActionResult<IEnumerable<RewardOwnerDto>>> GetRewardOwners()
        {
            var rewardOwners = await tapForPerksRepository.GetRewardOwnersAsync();
            var results = mapper.Map<IEnumerable<RewardOwnerDto>>(rewardOwners);
   
            return Ok(results);
        }

    }
}
