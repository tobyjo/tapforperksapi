using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TapForPerksAPI.Entities;

namespace TapForPerksAPI.DbContexts.EntityConfigurations;

public class RewardOwnerUserConfiguration : IEntityTypeConfiguration<RewardOwnerUser>
{
    public void Configure(EntityTypeBuilder<RewardOwnerUser> builder)
    {
        builder.HasKey(e => e.Id).HasName("PK__reward___3213E83FCA9F25E5");

        builder.ToTable("reward_owner_user");

        builder.HasIndex(e => e.RewardOwnerId, "idx_reward_owner_user_owner_id");
        builder.Property(e => e.Id)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("id");

        builder.Property(e => e.AuthProviderId)
            .HasMaxLength(255)
            .HasColumnName("auth_provider_id");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(sysdatetime())")
            .HasColumnName("created_at");

        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .HasColumnName("email");

        builder.Property(e => e.IsAdmin)
            .HasColumnName("is_admin");

        builder.Property(e => e.RewardOwnerId)
            .HasColumnName("reward_owner_id");

        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.HasOne(d => d.RewardOwner)
            .WithMany(p => p.RewardOwnerUsers)
            .HasForeignKey(d => d.RewardOwnerId)
            .HasConstraintName("fk_reward_owner_user_owner");

        // Seed data
        builder.HasData(
            new RewardOwnerUser
            {
                Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                RewardOwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                AuthProviderId = "auth0|admin001",
                Email = "baristaone@dailygrind.com",
                Name = "Barista One",
                IsAdmin = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new RewardOwnerUser
            {
                Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                RewardOwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                AuthProviderId = "auth0|admin002",
                Email = "host@wedding.com",
                Name = "Wedding Host",
                IsAdmin = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
