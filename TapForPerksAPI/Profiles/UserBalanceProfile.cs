using AutoMapper;

namespace TapForPerksAPI.Profiles;

public class UserBalanceProfile : Profile
{
    public UserBalanceProfile()
    {
        CreateMap<Entities.UserBalance, Models.UserBalanceDto>();
        
        CreateMap<Models.UserBalanceDto, Entities.UserBalance>()
            .ForMember(dest => dest.LoyaltyProgramme, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
    }
}
