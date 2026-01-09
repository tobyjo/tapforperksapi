using AutoMapper;

namespace TapForPerksAPI.Profiles;

public class RewardProfile : Profile
{
    public RewardProfile()
    {
        CreateMap<Entities.Reward, Models.RewardDto>();
        
        CreateMap<Models.RewardDto, Entities.Reward>()
            .ForMember(dest => dest.Metadata, opt => opt.Ignore())
            .ForMember(dest => dest.LoyaltyProgramme, opt => opt.Ignore())
            .ForMember(dest => dest.RewardRedemptions, opt => opt.Ignore());
    }
}
