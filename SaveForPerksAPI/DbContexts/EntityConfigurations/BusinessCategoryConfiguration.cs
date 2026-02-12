using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaveForPerksAPI.Entities;

namespace SaveForPerksAPI.DbContexts.EntityConfigurations;

public class BusinessCategoryConfiguration : IEntityTypeConfiguration<BusinessCategory>
{
    public void Configure(EntityTypeBuilder<BusinessCategory> builder)
    {
        builder.HasKey(e => e.Id);

        builder.ToTable("business_category");

        builder.HasIndex(e => e.Name).IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("id");

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired()
            .HasColumnName("name");

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500)
            .IsRequired()
            .HasColumnName("image_url");

        // Seed initial categories
        builder.HasData(
            new BusinessCategory
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Cafe / Casual Dining",
                ImageUrl = "/images/categories/cafe.jpg"
            },
            new BusinessCategory
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Coffee Shop",
                ImageUrl = "/images/categories/coffee-shop.jpg"
            },
            new BusinessCategory
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Restaurant",
                ImageUrl = "/images/categories/restaurant.jpg"
            },
            new BusinessCategory
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Bakery",
                ImageUrl = "/images/categories/bakery.jpg"
            },
            new BusinessCategory
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Bar / Pub",
                ImageUrl = "/images/categories/bar-pub.jpg"
            },
            new BusinessCategory
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Other",
                ImageUrl = "/images/categories/other.jpg"
            }
        );
    }
}
