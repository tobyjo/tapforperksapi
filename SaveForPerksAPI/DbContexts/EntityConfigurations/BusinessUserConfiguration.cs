using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaveForPerksAPI.Entities;

namespace SaveForPerksAPI.DbContexts.EntityConfigurations;

public class BusinessUserConfiguration : IEntityTypeConfiguration<BusinessUser>
{
    public void Configure(EntityTypeBuilder<BusinessUser> builder)
    {
        builder.HasKey(e => e.Id);

        builder.ToTable("business_user");

        builder.HasIndex(e => e.BusinessId);

        builder.HasIndex(e => e.AuthProviderId).IsUnique();

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

        builder.Property(e => e.BusinessId)
            .HasColumnName("business_id");

        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.HasOne(d => d.Business)
            .WithMany(p => p.BusinessUsers)
            .HasForeignKey(d => d.BusinessId);

        // Seed data
        /*
        builder.HasData(
            new BusinessUser
            {
                Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                BusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                AuthProviderId = "auth0|admin001",
                Email = "baristaone@dailygrind.com",
                Name = "Barista One",
                IsAdmin = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new BusinessUser
            {
                Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                BusinessId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                AuthProviderId = "auth0|admin002",
                Email = "host@wedding.com",
                Name = "Wedding Host",
                IsAdmin = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
        */
    }
}
