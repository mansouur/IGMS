using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class PdplRecordConfiguration : IEntityTypeConfiguration<PdplRecord>
{
    public void Configure(EntityTypeBuilder<PdplRecord> builder)
    {
        builder.ToTable("PdplRecords");

        builder.Property(r => r.TitleAr).HasMaxLength(400).IsRequired();
        builder.Property(r => r.TitleEn).HasMaxLength(400);
        builder.Property(r => r.PurposeAr).HasColumnType("nvarchar(max)");
        builder.Property(r => r.DataSubjectsAr).HasColumnType("nvarchar(max)");
        builder.Property(r => r.RetentionPeriod).HasMaxLength(200);
        builder.Property(r => r.SecurityMeasures).HasColumnType("nvarchar(max)");
        builder.Property(r => r.ThirdPartyDetails).HasColumnType("nvarchar(max)");
        builder.Property(r => r.TransferCountry).HasMaxLength(200);
        builder.Property(r => r.TransferSafeguards).HasColumnType("nvarchar(max)");

        builder.Property(r => r.DataCategory).HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.LegalBasis).HasConversion<string>().HasMaxLength(40);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(r => r.Department)
            .WithMany().HasForeignKey(r => r.DepartmentId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Owner)
            .WithMany().HasForeignKey(r => r.OwnerId).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Consents)
            .WithOne(c => c.PdplRecord).HasForeignKey(c => c.PdplRecordId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.DataRequests)
            .WithOne(d => d.PdplRecord).HasForeignKey(d => d.PdplRecordId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}

public class PdplConsentConfiguration : IEntityTypeConfiguration<PdplConsent>
{
    public void Configure(EntityTypeBuilder<PdplConsent> builder)
    {
        builder.ToTable("PdplConsents");

        builder.Property(c => c.SubjectNameAr).HasMaxLength(300).IsRequired();
        builder.Property(c => c.SubjectEmail).HasMaxLength(200);
        builder.Property(c => c.SubjectIdNumber).HasMaxLength(50);
        builder.Property(c => c.Notes).HasMaxLength(1000);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class PdplDataRequestConfiguration : IEntityTypeConfiguration<PdplDataRequest>
{
    public void Configure(EntityTypeBuilder<PdplDataRequest> builder)
    {
        builder.ToTable("PdplDataRequests");

        builder.Property(d => d.RequestType).HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.SubjectNameAr).HasMaxLength(300).IsRequired();
        builder.Property(d => d.SubjectEmail).HasMaxLength(200);
        builder.Property(d => d.DetailsAr).HasColumnType("nvarchar(max)");
        builder.Property(d => d.ResolutionAr).HasColumnType("nvarchar(max)");

        builder.HasOne(d => d.AssignedTo)
            .WithMany().HasForeignKey(d => d.AssignedToId).OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
