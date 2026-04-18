using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class RaciActivityConfiguration : IEntityTypeConfiguration<RaciActivity>
{
    public void Configure(EntityTypeBuilder<RaciActivity> builder)
    {
        builder.ToTable("RaciActivities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.NameAr).IsRequired().HasMaxLength(500);
        builder.Property(a => a.NameEn).IsRequired().HasMaxLength(500);
        builder.Property(a => a.DisplayOrder).IsRequired();
        builder.Property(a => a.CreatedBy).HasMaxLength(100);
        builder.Property(a => a.ModifiedBy).HasMaxLength(100);
        builder.Property(a => a.DeletedBy).HasMaxLength(100);

        builder.HasOne(a => a.Matrix)
            .WithMany(m => m.Activities)
            .HasForeignKey(a => a.RaciMatrixId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.AccountableUser)
            .WithMany()
            .HasForeignKey(a => a.AccountableUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(a => a.RaciMatrixId);
        builder.HasIndex(a => a.DisplayOrder);

        builder.HasQueryFilter(a => !a.IsDeleted && !a.Matrix.IsDeleted);
    }
}
