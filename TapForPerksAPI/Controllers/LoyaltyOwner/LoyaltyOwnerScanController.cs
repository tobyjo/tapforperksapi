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
        private readonly ITapForPerksRepository tapForPerksRepository;
        private readonly IMapper mapper;

    
            public LoyaltyOwnerScanController(ITapForPerksRepository tapForPerksRepository, IMapper mapper)
            {
                this.tapForPerksRepository = tapForPerksRepository ?? throw new ArgumentNullException(nameof(tapForPerksRepository));
                this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            [HttpGet("History")]
            public async Task<ActionResult<IEnumerable<LoyaltyOwnerDto>>> GetScansHistoryForLoyaltyProgramme()
            {
            /*
                var loyaltyOwners = await tapForPerksRepository.GetLoyaltyOwnersAsync();
                var results = mapper.Map<IEnumerable<LoyaltyOwnerDto>>(loyaltyOwners);
            */
                return Ok(true);
            }

    }
}
