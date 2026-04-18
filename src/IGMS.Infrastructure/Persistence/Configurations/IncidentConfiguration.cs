using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents");

        builder.Property(i => i.Severity)
            .HasConversion<string>().HasMaxLength(20);

        builder.Property(i => i.Status)
            .HasConversion<string>().HasMaxLength(20);

        builder.HasOne(i => i.Department)
            .WithMany()
            .HasForeignKey(i => i.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.ReportedBy)
            .WithMany()
            .HasForeignKey(i => i.ReportedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Avoid cascade path: Incident → Risk (Risk already has its own delete rules)
        builder.HasOne(i => i.Risk)
            .WithMany()
            .HasForeignKey(i => i.RiskId)
            .OnDelete(DeleteBehavior.SetNull);

        // Avoid cascade path: Incident → GovernanceTask
        builder.HasOne(i => i.Task)
            .WithMany()
            .HasForeignKey(i => i.TaskId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}
