using System;
using System.Collections.Generic;


namespace SaveForPerksAPI.Entities;

public class RewardOwner
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;        
    public string? Address { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<RewardOwnerUser> RewardOwnerUsers { get; set; } = new List<RewardOwnerUser>();

    public virtual ICollection<Reward> Rewards { get; set; } = new List<Reward>();
}
