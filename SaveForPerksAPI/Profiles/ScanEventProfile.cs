using AutoMapper;

namespace SaveForPerksAPI.Profiles;

public class ScanEventProfile : Profile
{
    public ScanEventProfile()
    {
        // From database to DTO
        CreateMap<Entities.ScanEvent, Models.ScanEventDto>();

        // From DTO to database
        CreateMap<Models.ScanEventForCreationDto, Entities.ScanEvent>()
            .ForMember(dest => dest.RewardOwnerUser, opt => opt.Ignore())
            .ForMember(dest => dest.Reward, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ScannedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());
    
    }
}
