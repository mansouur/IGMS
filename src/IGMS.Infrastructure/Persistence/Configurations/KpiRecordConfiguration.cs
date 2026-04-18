using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class KpiRecordConfiguration : IEntityTypeConfiguration<KpiRecord>
{
    public void Configure(EntityTypeBuilder<KpiRecord> builder)
    {
        builder.ToTable("KpiRecords");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.TargetValue).HasColumnType("decimal(18,4)");
        builder.Property(r => r.ActualValue).HasColumnType("decimal(18,4)");
        builder.Property(r => r.Notes).HasMaxLength(1000);
        builder.Property(r => r.RecordedBy).HasMaxLength(100);
        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.ModifiedBy).HasMaxLength(100);
        builder.Property(r => r.DeletedBy).HasMaxLength(100);

        // One Kpi → many KpiRecords; deleting a KPI cascades to its records
        builder.HasOne(r => r.Kpi)
            .WithMany(k => k.Records)
            .HasForeignKey(r => r.KpiId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: one record per KPI per period
        builder.HasIndex(r => new { r.KpiId, r.Year, r.Quarter }).IsUnique();

        builder.HasIndex(r => r.KpiId);
        builder.HasIndex(r => r.IsDeleted);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
