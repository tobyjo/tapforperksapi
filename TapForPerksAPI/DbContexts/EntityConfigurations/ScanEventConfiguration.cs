using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TapForPerksAPI.Entities;

namespace TapForPerksAPI.DbContexts.EntityConfigurations;

public class ScanEventConfiguration : IEntityTypeConfiguration<ScanEvent>
{
    public void Configure(EntityTypeBuilder<ScanEvent> builder)
    {
        builder.HasKey(e => e.Id).HasName("PK__scan_eve__3213E83F45201827");

        builder.ToTable("scan_event");

        builder.HasIndex(e => e.RewardId, "idx_scan_event_programme_id");
        builder.HasIndex(e => e.UserId, "idx_scan_event_user_id");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("id");

        builder.Property(e => e.LoyaltyOwnerUserId)
            .HasColumnName("loyalty_owner_user_id");

        builder.Property(e => e.RewardId)
            .HasColumnName("reward_id");

        builder.Property(e => e.PointsChange)
            .HasDefaultValue(1)
            .HasColumnName("points_change");

        builder.Property(e => e.ScannedAt)
            .HasDefaultValueSql("(sysdatetime())")
            .HasColumnName("scanned_at");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.QrCodeValue)
         .HasColumnName("qr_code_value");

        builder.HasOne(d => d.LoyaltyOwnerUser)
            .WithMany(p => p.ScanEvents)
            .HasForeignKey(d => d.LoyaltyOwnerUserId)
            .HasConstraintName("fk_scan_event_owner_user");

        builder.HasOne(d => d.Reward)
            .WithMany(p => p.ScanEvents)
            .HasForeignKey(d => d.RewardId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_scan_event_programme");

        builder.HasOne(d => d.User)
            .WithMany(p => p.ScanEvents)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("fk_scan_event_user");

        // No seed data for scan events initially
    }
}
