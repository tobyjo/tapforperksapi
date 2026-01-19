using System;
using System.Collections.Generic;

namespace SaveForPerksAPI.Entities;

public class ScanEvent
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid RewardId { get; set; }

    public Guid? RewardOwnerUserId { get; set; }

    public string QrCodeValue { get; set; } = null!;

    public DateTime ScannedAt { get; set; }

    public int PointsChange { get; set; }

    public virtual RewardOwnerUser? RewardOwnerUser { get; set; }

    public virtual Reward Reward { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
