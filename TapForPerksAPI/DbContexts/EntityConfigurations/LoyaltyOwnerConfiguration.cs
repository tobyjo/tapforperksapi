using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TapForPerksAPI.Entities;

namespace TapForPerksAPI.DbContexts.EntityConfigurations;

public class LoyaltyOwnerConfiguration : IEntityTypeConfiguration<LoyaltyOwner>
{
    public void Configure(EntityTypeBuilder<LoyaltyOwner> builder)
    {
        builder.HasKey(e => e.Id).HasName("PK__loyalty___3213E83FEC734646");

        builder.ToTable("loyalty_owner");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("id");

        builder.Property(e => e.Address)
            .HasMaxLength(500)
            .HasColumnName("address");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(sysdatetime())")
            .HasColumnName("created_at");

        builder.Property(e => e.Metadata)
            .HasColumnName("metadata");

        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasColumnName("Description");

        // Seed data
        builder.HasData(
            new LoyaltyOwner
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "The Daily Grind Coffee",
                Description = "Premium artisan coffee shop chain",
                Address = "123 High Street, London, UK",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new LoyaltyOwner
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Smith-Jones Wedding",
                Description = "Private event",
                Address = "456 Market Square, Manchester, UK",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
