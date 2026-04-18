using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(d => d.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Code).IsRequired().HasMaxLength(20);
        builder.Property(d => d.DescriptionAr).HasMaxLength(1000);
        builder.Property(d => d.DescriptionEn).HasMaxLength(1000);
        builder.Property(d => d.Level).IsRequired();
        builder.Property(d => d.CreatedBy).HasMaxLength(100);
        builder.Property(d => d.ModifiedBy).HasMaxLength(100);
        builder.Property(d => d.DeletedBy).HasMaxLength(100);

        // Indexes
        builder.HasIndex(d => d.Code).IsUnique();
        builder.HasIndex(d => d.ParentId);
        builder.HasIndex(d => d.IsDeleted);

        // Self-referencing hierarchy
        builder.HasOne(d => d.Parent)
            .WithMany(d => d.Children)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Manager FK – optional, no cascade to avoid cycles
        builder.HasOne(d => d.Manager)
            .WithMany()
            .HasForeignKey(d => d.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Global query filter – always exclude soft-deleted
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
