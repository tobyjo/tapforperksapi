using AutoMapper;

namespace TapForPerksAPI.Profiles;

public class LoyaltyOwnerUserProfile : Profile
{
    public LoyaltyOwnerUserProfile()
    {
        CreateMap<Entities.LoyaltyOwnerUser, Models.LoyaltyOwnerUserDto>();
        
        CreateMap<Models.LoyaltyOwnerUserDto, Entities.LoyaltyOwnerUser>()
            .ForMember(dest => dest.LoyaltyOwner, opt => opt.Ignore())
            .ForMember(dest => dest.RewardRedemptions, opt => opt.Ignore())
            .ForMember(dest => dest.ScanEvents, opt => opt.Ignore());
    }
}
