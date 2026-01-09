using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TapForPerksAPI.Entities;

namespace TapForPerksAPI.DbContexts;

public class TapForPerksContext : DbContext
{
    public TapForPerksContext()
    {
    }

    public TapForPerksContext(DbContextOptions<TapForPerksContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LoyaltyOwner> LoyaltyOwners { get; set; }

    public virtual DbSet<LoyaltyOwnerUser> LoyaltyOwnerUsers { get; set; }

    public virtual DbSet<LoyaltyProgramme> LoyaltyProgrammes { get; set; }

    public virtual DbSet<Reward> Rewards { get; set; }

    public virtual DbSet<RewardRedemption> RewardRedemptions { get; set; }

    public virtual DbSet<ScanEvent> ScanEvents { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBalance> UserBalances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoyaltyOwner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__loyalty___3213E83FEC734646");

            entity.ToTable("loyalty_owner");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Metadata).HasColumnName("metadata");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<LoyaltyOwnerUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__loyalty___3213E83FCA9F25E5");

            entity.ToTable("loyalty_owner_user");

            entity.HasIndex(e => e.LoyaltyOwnerId, "idx_loyalty_owner_user_owner_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AuthProviderId)
                .HasMaxLength(255)
                .HasColumnName("auth_provider_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            entity.Property(e => e.LoyaltyOwnerId).HasColumnName("loyalty_owner_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.LoyaltyOwner).WithMany(p => p.LoyaltyOwnerUsers)
                .HasForeignKey(d => d.LoyaltyOwnerId)
                .HasConstraintName("fk_loyalty_owner_user_owner");
        });

        modelBuilder.Entity<LoyaltyProgramme>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__loyalty___3213E83F9D47D4D7");

            entity.ToTable("loyalty_programme");

            entity.HasIndex(e => e.LoyaltyOwnerId, "idx_loyalty_programme_owner_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.LoyaltyOwnerId).HasColumnName("loyalty_owner_id");
            entity.Property(e => e.Metadata).HasColumnName("metadata");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.LoyaltyOwner).WithMany(p => p.LoyaltyProgrammes)
                .HasForeignKey(d => d.LoyaltyOwnerId)
                .HasConstraintName("fk_loyalty_programme_owner");
        });

        modelBuilder.Entity<Reward>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reward__3213E83F40482097");

            entity.ToTable("reward");

            entity.HasIndex(e => e.LoyaltyProgrammeId, "idx_reward_programme_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CostPoints).HasColumnName("cost_points");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LoyaltyProgrammeId).HasColumnName("loyalty_programme_id");
            entity.Property(e => e.MaxScans).HasColumnName("max_scans");
            entity.Property(e => e.Metadata).HasColumnName("metadata");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.RewardType)
                .HasMaxLength(100)
                .HasColumnName("reward_type");

            entity.HasOne(d => d.LoyaltyProgramme).WithMany(p => p.Rewards)
                .HasForeignKey(d => d.LoyaltyProgrammeId)
                .HasConstraintName("fk_reward_programme");
        });

        modelBuilder.Entity<RewardRedemption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reward_r__3213E83FC9ADA235");

            entity.ToTable("reward_redemption");

            entity.HasIndex(e => e.LoyaltyProgrammeId, "idx_reward_redemption_programme_id");

            entity.HasIndex(e => e.UserId, "idx_reward_redemption_user_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.LoyaltyOwnerUserId).HasColumnName("loyalty_owner_user_id");
            entity.Property(e => e.LoyaltyProgrammeId).HasColumnName("loyalty_programme_id");
            entity.Property(e => e.RedeemedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("redeemed_at");
            entity.Property(e => e.RewardId).HasColumnName("reward_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.LoyaltyOwnerUser).WithMany(p => p.RewardRedemptions)
                .HasForeignKey(d => d.LoyaltyOwnerUserId)
                .HasConstraintName("fk_reward_redemption_owner_user");

            entity.HasOne(d => d.LoyaltyProgramme).WithMany(p => p.RewardRedemptions)
                .HasForeignKey(d => d.LoyaltyProgrammeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reward_redemption_programme");

            entity.HasOne(d => d.Reward).WithMany(p => p.RewardRedemptions)
                .HasForeignKey(d => d.RewardId)
                .HasConstraintName("fk_reward_redemption_reward");

            entity.HasOne(d => d.User).WithMany(p => p.RewardRedemptions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_reward_redemption_user");
        });

        modelBuilder.Entity<ScanEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__scan_eve__3213E83F45201827");

            entity.ToTable("scan_event");

            entity.HasIndex(e => e.LoyaltyProgrammeId, "idx_scan_event_programme_id");

            entity.HasIndex(e => e.UserId, "idx_scan_event_user_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.LoyaltyOwnerUserId).HasColumnName("loyalty_owner_user_id");
            entity.Property(e => e.LoyaltyProgrammeId).HasColumnName("loyalty_programme_id");
            entity.Property(e => e.PointsChange)
                .HasDefaultValue(1)
                .HasColumnName("points_change");
            entity.Property(e => e.ScannedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("scanned_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.LoyaltyOwnerUser).WithMany(p => p.ScanEvents)
                .HasForeignKey(d => d.LoyaltyOwnerUserId)
                .HasConstraintName("fk_scan_event_owner_user");

            entity.HasOne(d => d.LoyaltyProgramme).WithMany(p => p.ScanEvents)
                .HasForeignKey(d => d.LoyaltyProgrammeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_scan_event_programme");

            entity.HasOne(d => d.User).WithMany(p => p.ScanEvents)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_scan_event_user");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F4461F44E");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164CB0A1AE0").IsUnique();

            entity.HasIndex(e => e.AuthProviderId, "UQ__users__C82CBBE99CDF45A3").IsUnique();

            entity.HasIndex(e => e.QrCodeValue, "UQ__users__C8EB4B8153934E5A").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AuthProviderId)
                .HasMaxLength(255)
                .HasColumnName("auth_provider_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.QrCodeValue)
                .HasMaxLength(255)
                .HasColumnName("qr_code_value");
        });

        modelBuilder.Entity<UserBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_bal__3213E83F23104F90");

            entity.ToTable("user_balance");

            entity.HasIndex(e => e.LoyaltyProgrammeId, "idx_user_balance_programme_id");

            entity.HasIndex(e => e.UserId, "idx_user_balance_user_id");

            entity.HasIndex(e => new { e.UserId, e.LoyaltyProgrammeId }, "uq_user_balance").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Balance).HasColumnName("balance");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("last_updated");
            entity.Property(e => e.LoyaltyProgrammeId).HasColumnName("loyalty_programme_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.LoyaltyProgramme).WithMany(p => p.UserBalances)
                .HasForeignKey(d => d.LoyaltyProgrammeId)
                .HasConstraintName("fk_user_balance_programme");

            entity.HasOne(d => d.User).WithMany(p => p.UserBalances)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_user_balance_user");
        });

        base.OnModelCreating(modelBuilder);
    }

   
}
