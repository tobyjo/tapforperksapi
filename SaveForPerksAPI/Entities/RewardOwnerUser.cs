using System;
using System.Collections.Generic;

namespace SaveForPerksAPI.Entities;

public class RewardOwnerUser
{
    public Guid Id { get; set; }

    public Guid RewardOwnerId { get; set; }

    public string AuthProviderId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsAdmin { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual RewardOwner RewardOwner { get; set; } = null!;

    public virtual ICollection<RewardRedemption> RewardRedemptions { get; set; } = new List<RewardRedemption>();

    public virtual ICollection<ScanEvent> ScanEvents { get; set; } = new List<ScanEvent>();
}
