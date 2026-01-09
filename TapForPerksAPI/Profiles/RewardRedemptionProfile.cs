using AutoMapper;

namespace TapForPerksAPI.Profiles;

public class RewardRedemptionProfile : Profile
{
    public RewardRedemptionProfile()
    {
        CreateMap<Entities.RewardRedemption, Models.RewardRedemptionDto>();
        
        CreateMap<Models.RewardRedemptionDto, Entities.RewardRedemption>()
            .ForMember(dest => dest.LoyaltyOwnerUser, opt => opt.Ignore())
            .ForMember(dest => dest.LoyaltyProgramme, opt => opt.Ignore())
            .ForMember(dest => dest.Reward, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
    }
}
