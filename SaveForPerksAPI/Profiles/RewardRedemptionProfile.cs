using AutoMapper;

namespace SaveForPerksAPI.Profiles;

public class RewardRedemptionProfile : Profile
{
    public RewardRedemptionProfile()
    {
        // From database to DTO
        CreateMap<Entities.RewardRedemption, Models.RewardRedemptionDto>();

        // From DTO to database
        CreateMap<Models.RewardRedemptionDto, Entities.RewardRedemption>()
            .ForMember(dest => dest.RewardOwnerUser, opt => opt.Ignore())
            .ForMember(dest => dest.Reward, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // From DTO to database
        CreateMap<Models.RewardRedemptionForCreationDto, Entities.RewardRedemption>()
             .ForMember(dest => dest.RewardOwnerUser, opt => opt.Ignore())
            .ForMember(dest => dest.Reward, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
             .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RedeemedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());
    }
}
