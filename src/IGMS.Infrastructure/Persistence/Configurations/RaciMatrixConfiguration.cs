using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class RaciMatrixConfiguration : IEntityTypeConfiguration<RaciMatrix>
{
    public void Configure(EntityTypeBuilder<RaciMatrix> builder)
    {
        builder.ToTable("RaciMatrices");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.TitleAr).IsRequired().HasMaxLength(300);
        builder.Property(r => r.TitleEn).IsRequired().HasMaxLength(300);
        builder.Property(r => r.DescriptionAr).HasMaxLength(2000);
        builder.Property(r => r.DescriptionEn).HasMaxLength(2000);
        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.ModifiedBy).HasMaxLength(100);
        builder.Property(r => r.DeletedBy).HasMaxLength(100);

        builder.HasOne(r => r.Department)
            .WithMany()
            .HasForeignKey(r => r.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.ApprovedBy)
            .WithMany()
            .HasForeignKey(r => r.ApprovedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.DepartmentId);
        builder.HasIndex(r => r.IsDeleted);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
