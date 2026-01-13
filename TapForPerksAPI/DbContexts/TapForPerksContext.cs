using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TapForPerksAPI.Entities;

namespace TapForPerksAPI.DbContexts;

public class TapForPerksContext : DbContext
{
    public TapForPerksContext(DbContextOptions<TapForPerksContext> options)
        : base(options)
    {
    }

    public DbSet<RewardOwner> RewardOwners { get; set; }
    public DbSet<RewardOwnerUser> RewardOwnerUsers { get; set; }
    public DbSet<Reward> Rewards { get; set; }
    public DbSet<RewardRedemption> RewardRedemptions { get; set; }
    public DbSet<ScanEvent> ScanEvents { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserBalance> UserBalances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TapForPerksContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
