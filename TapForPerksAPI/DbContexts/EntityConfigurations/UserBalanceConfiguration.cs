using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TapForPerksAPI.Entities;

namespace TapForPerksAPI.DbContexts.EntityConfigurations;

public class UserBalanceConfiguration : IEntityTypeConfiguration<UserBalance>
{
    public void Configure(EntityTypeBuilder<UserBalance> builder)
    {
        builder.HasKey(e => e.Id).HasName("PK__user_bal__3213E83F23104F90");

        builder.ToTable("user_balance");

        builder.HasIndex(e => e.LoyaltyProgrammeId, "idx_user_balance_programme_id");
        builder.HasIndex(e => e.UserId, "idx_user_balance_user_id");
        builder.HasIndex(e => new { e.UserId, e.LoyaltyProgrammeId }, "uq_user_balance").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("id");

        builder.Property(e => e.Balance)
            .HasColumnName("balance");

        builder.Property(e => e.LastUpdated)
            .HasDefaultValueSql("(sysdatetime())")
            .HasColumnName("last_updated");

        builder.Property(e => e.LoyaltyProgrammeId)
            .HasColumnName("loyalty_programme_id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.HasOne(d => d.LoyaltyProgramme)
            .WithMany(p => p.UserBalances)
            .HasForeignKey(d => d.LoyaltyProgrammeId)
            .HasConstraintName("fk_user_balance_programme");

        builder.HasOne(d => d.User)
            .WithMany(p => p.UserBalances)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("fk_user_balance_user");

        // Seed data
        builder.HasData(
            new UserBalance
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                UserId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                LoyaltyProgrammeId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Balance = 7,
                LastUpdated = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc)
            },
            new UserBalance
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                LoyaltyProgrammeId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Balance = 12,
                LastUpdated = new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc)
            },
            new UserBalance
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                LoyaltyProgrammeId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Balance = 5,
                LastUpdated = new DateTime(2026, 1, 7, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
