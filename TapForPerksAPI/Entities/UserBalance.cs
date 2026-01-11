using System;
using System.Collections.Generic;

namespace TapForPerksAPI.Entities;

public class UserBalance
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid RewardId { get; set; }

    public int Balance { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual Reward Reward { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
