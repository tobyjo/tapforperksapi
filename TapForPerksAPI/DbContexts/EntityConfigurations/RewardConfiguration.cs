using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TapForPerksAPI.Entities;

namespace TapForPerksAPI.DbContexts.EntityConfigurations;

public class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> builder)
    {
        builder.HasKey(e => e.Id).HasName("PK__reward__3213E83F40482097");

        builder.ToTable("reward");

        builder.HasIndex(e => e.LoyaltyProgrammeId, "idx_reward_programme_id");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("id");

        builder.Property(e => e.CostPoints)
            .HasColumnName("cost_points");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(sysdatetime())")
            .HasColumnName("created_at");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(e => e.LoyaltyProgrammeId)
            .HasColumnName("loyalty_programme_id");

       
        builder.Property(e => e.Metadata)
            .HasColumnName("metadata");

        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.RewardType)
            .HasMaxLength(100)
            .HasColumnName("reward_type");

        builder.HasOne(d => d.LoyaltyProgramme)
            .WithMany(p => p.Rewards)
            .HasForeignKey(d => d.LoyaltyProgrammeId)
            .HasConstraintName("fk_reward_programme");

        // Seed data
        builder.HasData(
            new Reward
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                LoyaltyProgrammeId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Free Coffee at 5 points",
                RewardType = "points",
                CostPoints = 5,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Reward
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                LoyaltyProgrammeId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Free Pastry at 5 points",
                RewardType = "points",
                CostPoints = 5,
                IsActive = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Reward
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                LoyaltyProgrammeId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Wedding Drink Allowance of 2 drinks",
                RewardType = "allowance_limit",
                CostPoints = 2,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
