using System;
using System.Collections.Generic;

namespace SaveForPerksAPI.Entities;

public class Reward
{
    public Guid Id { get; set; }
    public Guid RewardOwnerId { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public int CostPoints { get; set; }
    public RewardType RewardType { get; set; }  // Changed from string to enum
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual RewardOwner RewardOwner { get; set; } = null!;
    public virtual ICollection<RewardRedemption> RewardRedemptions { get; set; } = new List<RewardRedemption>();
    public virtual ICollection<ScanEvent> ScanEvents { get; set; } = new List<ScanEvent>();
    public virtual ICollection<UserBalance> UserBalances { get; set; } = new List<UserBalance>();
}
