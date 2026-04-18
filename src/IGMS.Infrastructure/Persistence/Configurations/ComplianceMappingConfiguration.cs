using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class ComplianceMappingConfiguration : IEntityTypeConfiguration<ComplianceMapping>
{
    public void Configure(EntityTypeBuilder<ComplianceMapping> builder)
    {
        builder.ToTable("ComplianceMappings");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.EntityType).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Clause).HasMaxLength(100);
        builder.Property(c => c.Notes).HasMaxLength(500);
        builder.Property(c => c.CreatedBy).HasMaxLength(100);
        builder.Property(c => c.ModifiedBy).HasMaxLength(100);
        builder.Property(c => c.DeletedBy).HasMaxLength(100);

        builder.HasIndex(c => new { c.EntityType, c.EntityId, c.Framework, c.Clause }).IsUnique();
        builder.HasIndex(c => new { c.EntityType, c.EntityId });
        builder.HasIndex(c => c.IsDeleted);
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
