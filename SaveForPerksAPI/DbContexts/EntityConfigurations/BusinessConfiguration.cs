using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaveForPerksAPI.Entities;

namespace SaveForPerksAPI.DbContexts.EntityConfigurations;

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.HasKey(e => e.Id);

        builder.ToTable("business");

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

        builder.Property(e => e.CategoryId)
            .HasColumnName("category_id");

        // Foreign key relationship
        builder.HasOne(e => e.Category)
            .WithMany(c => c.Businesses)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed data
        /*
        builder.HasData(
            new Business
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "The Daily Grind Coffee",
                Description = "Premium artisan coffee shop chain",
                Address = "123 High Street, London, UK",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Business
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Smith-Jones Wedding",
                Description = "Private event",
                Address = "456 Market Square, Manchester, UK",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
        */
    }
}
