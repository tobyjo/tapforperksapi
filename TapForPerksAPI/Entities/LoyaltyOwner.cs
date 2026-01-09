using System;
using System.Collections.Generic;


namespace TapForPerksAPI.Entities;

public class LoyaltyOwner
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;        
    public string? Address { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<LoyaltyOwnerUser> LoyaltyOwnerUsers { get; set; } = new List<LoyaltyOwnerUser>();

    public virtual ICollection<LoyaltyProgramme> LoyaltyProgrammes { get; set; } = new List<LoyaltyProgramme>();
}
