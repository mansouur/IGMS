using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Module).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Action).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Code).IsRequired().HasMaxLength(100);
        builder.Property(p => p.DescriptionAr).IsRequired().HasMaxLength(300);
        builder.Property(p => p.DescriptionEn).IsRequired().HasMaxLength(300);
        builder.Property(p => p.CreatedBy).HasMaxLength(100);
        builder.Property(p => p.ModifiedBy).HasMaxLength(100);
        builder.Property(p => p.DeletedBy).HasMaxLength(100);

        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.Module);
        builder.HasIndex(p => p.IsDeleted);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
