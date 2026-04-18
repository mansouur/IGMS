using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(r => r.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Code).IsRequired().HasMaxLength(50);
        builder.Property(r => r.DescriptionAr).HasMaxLength(500);
        builder.Property(r => r.DescriptionEn).HasMaxLength(500);
        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.ModifiedBy).HasMaxLength(100);
        builder.Property(r => r.DeletedBy).HasMaxLength(100);

        builder.HasIndex(r => r.Code).IsUnique();
        builder.HasIndex(r => r.IsDeleted);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
