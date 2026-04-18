using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TitleAr).IsRequired().HasMaxLength(500);
        builder.Property(p => p.TitleEn).HasMaxLength(500);
        builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
        builder.Property(p => p.DescriptionAr).HasMaxLength(4000);
        builder.Property(p => p.DescriptionEn).HasMaxLength(4000);
        builder.Property(p => p.CreatedBy).HasMaxLength(100);
        builder.Property(p => p.ModifiedBy).HasMaxLength(100);
        builder.Property(p => p.DeletedBy).HasMaxLength(100);

        builder.HasIndex(p => p.Code);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.IsDeleted);

        // Policy → Approver (optional — recorded automatically on publish)
        builder.HasOne(p => p.Approver)
            .WithMany()
            .HasForeignKey(p => p.ApproverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
