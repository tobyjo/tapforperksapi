using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TapForPerksAPI.Models;
using TapForPerksAPI.Repositories;

namespace TapForPerksAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin")]
    public class AdminLoyaltyOwnerController : ControllerBase
    {
        private readonly ISaveForPerksRepository tapForPerksRepository;
        private readonly IMapper mapper;

        public AdminLoyaltyOwnerController(ISaveForPerksRepository tapForPerksRepository, IMapper mapper)
        {
            this.tapForPerksRepository = tapForPerksRepository ?? throw new ArgumentNullException(nameof(tapForPerksRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("GetLoyaltyOwners")]
        public async Task<ActionResult<IEnumerable<LoyaltyOwnerDto>>> GetLoyaltyOwners()
        {
            var loyaltyOwners = await tapForPerksRepository.GetLoyaltyOwnersAsync();
            var results = mapper.Map<IEnumerable<LoyaltyOwnerDto>>(loyaltyOwners);
   
            return Ok(results);
        }

    }
}
