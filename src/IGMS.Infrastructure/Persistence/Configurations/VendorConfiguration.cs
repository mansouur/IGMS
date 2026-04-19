using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");

        builder.Property(v => v.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(v => v.NameEn).HasMaxLength(200);
        builder.Property(v => v.Category).HasMaxLength(100);
        builder.Property(v => v.ContactName).HasMaxLength(100);
        builder.Property(v => v.ContactEmail).HasMaxLength(150);
        builder.Property(v => v.ContactPhone).HasMaxLength(30);
        builder.Property(v => v.Website).HasMaxLength(200);
        builder.Property(v => v.RiskNotes).HasMaxLength(1000);
        builder.Property(v => v.Notes).HasMaxLength(2000);

        builder.Property(v => v.Type)
            .HasConversion<string>().HasMaxLength(30);

        builder.Property(v => v.Status)
            .HasConversion<string>().HasMaxLength(20);

        builder.Property(v => v.RiskLevel)
            .HasConversion<string>().HasMaxLength(20);

        builder.Property(v => v.ContractValue)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(v => v.Department)
            .WithMany()
            .HasForeignKey(v => v.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}
