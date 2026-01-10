using System;
using System.Collections.Generic;

namespace TapForPerksAPI.Entities;

public class Reward
{
    public Guid Id { get; set; }

    public Guid LoyaltyProgrammeId { get; set; }

    public string Name { get; set; } = null!;

    public string RewardType { get; set; } = null!;

    public int? CostPoints { get; set; }

    public string? Metadata { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual LoyaltyProgramme LoyaltyProgramme { get; set; } = null!;

    public virtual ICollection<RewardRedemption> RewardRedemptions { get; set; } = new List<RewardRedemption>();
}
