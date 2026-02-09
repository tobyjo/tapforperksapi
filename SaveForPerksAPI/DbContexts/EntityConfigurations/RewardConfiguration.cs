using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaveForPerksAPI.Entities;
using SaveForPerksAPI.Extensions;

namespace SaveForPerksAPI.DbContexts.EntityConfigurations;

public class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> builder)
    {
        builder.HasKey(e => e.Id);

        builder.ToTable("reward");

        builder.HasIndex(e => e.BusinessId);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("id");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(sysdatetime())")
            .HasColumnName("created_at");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active");

        builder.Property(e => e.BusinessId)
            .HasColumnName("business_id");

        builder.Property(e => e.CostPoints)
            .HasColumnName("cost_points");

        // Store enum as string with snake_case conversion
        builder.Property(e => e.RewardType)
            .HasMaxLength(100)
            .HasColumnName("reward_type")
            .HasConversion(
                v => v.ToString().ToSnakeCase(), // Enum to DB
                v => Enum.Parse<RewardType>(v.ToPascalCase())); // DB to Enum

        builder.Property(e => e.Metadata)
            .HasColumnName("metadata");

        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.HasOne(d => d.Business)
            .WithMany(p => p.Rewards)
            .HasForeignKey(d => d.BusinessId);


        // Seed data using enum
         /*
        builder.HasData(
            new Reward
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                BusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Pay for 5 coffees, get sixth free",
                CostPoints = 5,
                RewardType = RewardType.IncrementalPoints,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
           
            ,
            new Reward
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                RewardOwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Wedding Drink Allowance of 2 drinks",
                CostPoints = 2,
                RewardType = RewardType.AllowanceLimit,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
            
        );
         */
    }
}
