namespace SaveForPerksAPI.Models
{
    public class RewardOwnerUserProfileResponseDto
    {
        public bool RewardOwnerProfileExists { get; set; }
        public RewardOwnerDto RewardOwner { get; set; } = null!;
        public RewardOwnerUserDto RewardOwnerUser { get; set; } = null!;
    }
}
