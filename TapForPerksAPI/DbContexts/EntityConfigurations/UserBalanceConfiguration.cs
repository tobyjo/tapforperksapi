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

        builder.HasIndex(e => e.RewardId, "idx_user_balance_programme_id");
        builder.HasIndex(e => e.UserId, "idx_user_balance_user_id");
        builder.HasIndex(e => new { e.UserId, e.RewardId }, "uq_user_balance").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("id");

        builder.Property(e => e.Balance)
            .HasColumnName("balance");

        builder.Property(e => e.LastUpdated)
            .HasDefaultValueSql("(sysdatetime())")
            .HasColumnName("last_updated");

        builder.Property(e => e.RewardId)
            .HasColumnName("reward_id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.HasOne(d => d.Reward)
            .WithMany(p => p.UserBalances)
            .HasForeignKey(d => d.RewardId)
            .HasConstraintName("fk_user_balance_programme");

        builder.HasOne(d => d.User)
            .WithMany(p => p.UserBalances)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("fk_user_balance_user");


      
    }
}
