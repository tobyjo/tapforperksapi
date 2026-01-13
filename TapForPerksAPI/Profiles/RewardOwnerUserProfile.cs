using AutoMapper;

namespace TapForPerksAPI.Profiles;

public class RewardOwnerUserProfile : Profile
{
    public RewardOwnerUserProfile()
    {
        CreateMap<Entities.RewardOwnerUser, Models.RewardOwnerUserDto>();
        
        CreateMap<Models.RewardOwnerUserDto, Entities.RewardOwnerUser>()
            .ForMember(dest => dest.RewardOwner, opt => opt.Ignore())
            .ForMember(dest => dest.RewardRedemptions, opt => opt.Ignore())
            .ForMember(dest => dest.ScanEvents, opt => opt.Ignore());
    }
}
