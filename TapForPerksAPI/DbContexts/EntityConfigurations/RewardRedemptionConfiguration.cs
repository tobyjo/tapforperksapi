using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TapForPerksAPI.Entities;

namespace TapForPerksAPI.DbContexts.EntityConfigurations;

public class RewardRedemptionConfiguration : IEntityTypeConfiguration<RewardRedemption>
{
    public void Configure(EntityTypeBuilder<RewardRedemption> builder)
    {
        builder.HasKey(e => e.Id).HasName("PK__reward_r__3213E83FC9ADA235");

        builder.ToTable("reward_redemption");

        builder.HasIndex(e => e.RewardId, "idx_reward_redemption_reward_id");
        builder.HasIndex(e => e.UserId, "idx_reward_redemption_user_id");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("id");

        builder.Property(e => e.LoyaltyOwnerUserId)
            .HasColumnName("loyalty_owner_user_id");

        builder.Property(e => e.RewardId)
            .HasColumnName("reward_id");

        builder.Property(e => e.RedeemedAt)
            .HasDefaultValueSql("(sysdatetime())")
            .HasColumnName("redeemed_at");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.HasOne(d => d.LoyaltyOwnerUser)
            .WithMany(p => p.RewardRedemptions)
            .HasForeignKey(d => d.LoyaltyOwnerUserId)
            .HasConstraintName("fk_reward_redemption_owner_user");

        builder.HasOne(d => d.Reward)
            .WithMany(p => p.RewardRedemptions)
            .HasForeignKey(d => d.RewardId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_reward_redemption_programme");

        builder.HasOne(d => d.User)
            .WithMany(p => p.RewardRedemptions)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("fk_reward_redemption_user");

        // No seed data for redemptions initially
    }
}
