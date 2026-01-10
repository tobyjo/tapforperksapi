using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TapForPerksAPI.Entities;

namespace TapForPerksAPI.DbContexts.EntityConfigurations;

public class LoyaltyOwnerUserConfiguration : IEntityTypeConfiguration<LoyaltyOwnerUser>
{
    public void Configure(EntityTypeBuilder<LoyaltyOwnerUser> builder)
    {
        builder.HasKey(e => e.Id).HasName("PK__loyalty___3213E83FCA9F25E5");

        builder.ToTable("loyalty_owner_user");

        builder.HasIndex(e => e.LoyaltyOwnerId, "idx_loyalty_owner_user_owner_id");

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

        builder.Property(e => e.LoyaltyOwnerId)
            .HasColumnName("loyalty_owner_id");

        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.HasOne(d => d.LoyaltyOwner)
            .WithMany(p => p.LoyaltyOwnerUsers)
            .HasForeignKey(d => d.LoyaltyOwnerId)
            .HasConstraintName("fk_loyalty_owner_user_owner");

        // Seed data
        builder.HasData(
            new LoyaltyOwnerUser
            {
                Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                LoyaltyOwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                AuthProviderId = "auth0|admin001",
                Email = "baristaone@dailygrind.com",
                Name = "Barista One",
                IsAdmin = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new LoyaltyOwnerUser
            {
                Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                LoyaltyOwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                AuthProviderId = "auth0|admin002",
                Email = "host@wedding.com",
                Name = "Wedding Host",
                IsAdmin = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
