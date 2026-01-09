using AutoMapper;

namespace TapForPerksAPI.Profiles;

public class ScanEventProfile : Profile
{
    public ScanEventProfile()
    {
        CreateMap<Entities.ScanEvent, Models.ScanEventDto>();
        
        CreateMap<Models.ScanEventDto, Entities.ScanEvent>()
            .ForMember(dest => dest.LoyaltyOwnerUser, opt => opt.Ignore())
            .ForMember(dest => dest.LoyaltyProgramme, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
    }
}
