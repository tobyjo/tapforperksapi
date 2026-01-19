using AutoMapper;

namespace SaveForPerksAPI.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<Entities.User, Models.UserDto>();
        
        CreateMap<Models.UserDto, Entities.User>()
            .ForMember(dest => dest.RewardRedemptions, opt => opt.Ignore())
            .ForMember(dest => dest.ScanEvents, opt => opt.Ignore())
            .ForMember(dest => dest.UserBalances, opt => opt.Ignore());
    }
}
