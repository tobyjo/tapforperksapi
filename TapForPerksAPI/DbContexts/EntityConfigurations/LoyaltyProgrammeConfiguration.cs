using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TapForPerksAPI.Entities;

namespace TapForPerksAPI.DbContexts.EntityConfigurations;

public class LoyaltyProgrammeConfiguration : IEntityTypeConfiguration<LoyaltyProgramme>
{
    public void Configure(EntityTypeBuilder<LoyaltyProgramme> builder)
    {
        builder.HasKey(e => e.Id).HasName("PK__loyalty___3213E83F9D47D4D7");

        builder.ToTable("loyalty_programme");

        builder.HasIndex(e => e.LoyaltyOwnerId, "idx_loyalty_programme_owner_id");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("id");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(sysdatetime())")
            .HasColumnName("created_at");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active");

        builder.Property(e => e.LoyaltyOwnerId)
            .HasColumnName("loyalty_owner_id");

        builder.Property(e => e.Metadata)
            .HasColumnName("metadata");

        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.HasOne(d => d.LoyaltyOwner)
            .WithMany(p => p.LoyaltyProgrammes)
            .HasForeignKey(d => d.LoyaltyOwnerId)
            .HasConstraintName("fk_loyalty_programme_owner");

        // Seed data
        builder.HasData(
            new LoyaltyProgramme
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                LoyaltyOwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Coffee Lovers Club",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new LoyaltyProgramme
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                LoyaltyOwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Healthy Habits Rewards",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
