using System;
using System.Collections.Generic;

namespace TapForPerksAPI.Entities;

public class RewardRedemption
{
    public Guid Id { get; set; }


    public Guid UserId { get; set; }

    public Guid RewardId { get; set; }

    public Guid? LoyaltyOwnerUserId { get; set; }

    public DateTime RedeemedAt { get; set; }

    public virtual LoyaltyOwnerUser? LoyaltyOwnerUser { get; set; }

    public virtual Reward Reward { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
