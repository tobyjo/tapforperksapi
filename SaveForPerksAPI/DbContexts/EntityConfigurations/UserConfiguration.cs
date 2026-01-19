using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaveForPerksAPI.Entities;

namespace SaveForPerksAPI.DbContexts.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id).HasName("PK__users__3213E83F4461F44E");

        builder.ToTable("users");

        builder.HasIndex(e => e.Email, "UQ__users__AB6E6164CB0A1AE0").IsUnique();
        builder.HasIndex(e => e.AuthProviderId, "UQ__users__C82CBBE99CDF45A3").IsUnique();
        builder.HasIndex(e => e.QrCodeValue, "UQ__users__C8EB4B8153934E5A").IsUnique();

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

        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.QrCodeValue)
            .HasMaxLength(255)
            .HasColumnName("qr_code_value");

        // Seed data
        builder.HasData(
            new User
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                AuthProviderId = "auth0|user001",
                Email = "alice@example.com",
                Name = "Alice Customer",
                QrCodeValue = "QR001-ALICE-9999",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                AuthProviderId = "auth0|user002",
                Email = "bob@example.com",
                Name = "Bob Customer",
                QrCodeValue = "QR002-BOB-AAAA",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                AuthProviderId = "auth0|user003",
                Email = "charlie@example.com",
                Name = "Charlie Customer",
                QrCodeValue = "QR003-CHARLIE-BBBB",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
