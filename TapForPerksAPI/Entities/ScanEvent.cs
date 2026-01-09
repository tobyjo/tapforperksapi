using System;
using System.Collections.Generic;

namespace TapForPerksAPI.Entities;

public class ScanEvent
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid LoyaltyProgrammeId { get; set; }

    public Guid? LoyaltyOwnerUserId { get; set; }

    public DateTime ScannedAt { get; set; }

    public int PointsChange { get; set; }

    public virtual LoyaltyOwnerUser? LoyaltyOwnerUser { get; set; }

    public virtual LoyaltyProgramme LoyaltyProgramme { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
