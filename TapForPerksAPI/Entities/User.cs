using System;
using System.Collections.Generic;

namespace TapForPerksAPI.Entities;

public class User
{
    public Guid Id { get; set; }

    public string AuthProviderId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string QrCodeValue { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<RewardRedemption> RewardRedemptions { get; set; } = new List<RewardRedemption>();

    public virtual ICollection<ScanEvent> ScanEvents { get; set; } = new List<ScanEvent>();

    public virtual ICollection<UserBalance> UserBalances { get; set; } = new List<UserBalance>();
}
