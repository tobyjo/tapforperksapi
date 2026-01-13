using AutoMapper;

namespace TapForPerksAPI.Profiles
{
    public class RewardOwnerProfile : Profile
    {
        public RewardOwnerProfile()
        {
            // From database entity to DTO
            CreateMap<Entities.RewardOwner, Models.RewardOwnerDto>();

            // From DTO to database entity
            CreateMap<Models.RewardOwnerDto, Entities.RewardOwner>()
                .ForMember(dest => dest.Metadata, opt => opt.Ignore())
                .ForMember(dest => dest.RewardOwnerUsers, opt => opt.Ignore())
                .ForMember(dest => dest.Rewards, opt => opt.Ignore());
        }
    }
}
