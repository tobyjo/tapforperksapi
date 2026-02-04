namespace SaveForPerksAPI.Models
{
    public class UserBalanceAndInfoResponseDto
    {
        public string QrCodeValue { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public int CurrentBalance { get; set; }
        public AvailableRewardDto? AvailableReward { get; set; }  // Single reward instead of list
        public int NumRewardsAvailable { get; set; }  // How many rewards are available to claim?
    }

    public class AvailableRewardForUserDto
    {
        public Guid RewardId { get; set; }
        public string RewardName { get; set; } = string.Empty;
        public string RewardType { get; set; } = string.Empty;
        public int RequiredPoints { get; set; }
    }
}
