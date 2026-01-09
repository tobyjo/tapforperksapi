using AutoMapper;

namespace TapForPerksAPI.Profiles;

public class LoyaltyProgrammeProfile : Profile
{
    public LoyaltyProgrammeProfile()
    {
        CreateMap<Entities.LoyaltyProgramme, Models.LoyaltyProgrammeDto>();
        
        CreateMap<Models.LoyaltyProgrammeDto, Entities.LoyaltyProgramme>()
            .ForMember(dest => dest.Metadata, opt => opt.Ignore())
            .ForMember(dest => dest.LoyaltyOwner, opt => opt.Ignore())
            .ForMember(dest => dest.RewardRedemptions, opt => opt.Ignore())
            .ForMember(dest => dest.Rewards, opt => opt.Ignore())
            .ForMember(dest => dest.ScanEvents, opt => opt.Ignore())
            .ForMember(dest => dest.UserBalances, opt => opt.Ignore());
    }
}
