using AutoMapper;

namespace TapForPerksAPI.Profiles
{
    public class LoyaltyOwnerProfile : Profile
    {
        public LoyaltyOwnerProfile()
        {
            // From database entity to DTO
            CreateMap<Entities.LoyaltyOwner, Models.LoyaltyOwnerDto>();

            // From DTO to database entity
            CreateMap<Models.LoyaltyOwnerDto, Entities.LoyaltyOwner>()
                .ForMember(dest => dest.Metadata, opt => opt.Ignore())
                .ForMember(dest => dest.LoyaltyOwnerUsers, opt => opt.Ignore())
                .ForMember(dest => dest.LoyaltyProgrammes, opt => opt.Ignore());
        }
    }
}
