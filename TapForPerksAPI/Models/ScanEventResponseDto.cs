namespace TapForPerksAPI.Models;

public class ScanEventResponseDto
{
    public ScanEventDto ScanEvent { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public int CurrentBalance { get; set; }
    public bool RewardAvailable { get; set; }
    public AvailableRewardDto? AvailableReward { get; set; }  // Single reward instead of list
    public int TimesClaimable { get; set; }  // How many times can they claim it?
}

public class AvailableRewardDto
{
    public Guid RewardId { get; set; }
    public string RewardName { get; set; } = string.Empty;
    public string RewardType { get; set; } = string.Empty;
    public int RequiredPoints { get; set; }
}