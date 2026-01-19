namespace SaveForPerksAPI.Models
{
    public class UserBalanceAndInfoResponseDto
    {
        public string QrCodeValue { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public int CurrentBalance { get; set; }
        public AvailableRewardDto? AvailableReward { get; set; }  // Single reward instead of list
        public int TimesClaimable { get; set; }  // How many times can they claim it?
    }

    public class AvailableRewardForUserDto
    {
        public Guid RewardId { get; set; }
        public string RewardName { get; set; } = string.Empty;
        public string RewardType { get; set; } = string.Empty;
        public int RequiredPoints { get; set; }
    }
}
