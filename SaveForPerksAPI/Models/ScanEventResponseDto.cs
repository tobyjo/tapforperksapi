namespace SaveForPerksAPI.Models;

public class ScanEventResponseDto
{
    public ScanEventDto ScanEvent { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public int CurrentBalance { get; set; }
    public bool RewardAvailable { get; set; }
    public AvailableRewardDto? AvailableReward { get; set; }
    public int NumRewardsAvailable { get; set; }
    public ClaimedRewardsDto? ClaimedRewards { get; set; }  // NEW: Info about rewards just claimed
}

public class AvailableRewardDto
{
    public Guid RewardId { get; set; }
    public string RewardName { get; set; } = string.Empty;
    public string RewardType { get; set; } = string.Empty;
    public int RequiredPoints { get; set; }
}

public class ClaimedRewardsDto
{
    public int NumberClaimed { get; set; }
    public string RewardName { get; set; } = string.Empty;
    public int TotalPointsDeducted { get; set; }
    public List<Guid> RedemptionIds { get; set; } = new();  // IDs of the redemption records created
}