using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class PolicyAttachmentConfiguration : IEntityTypeConfiguration<PolicyAttachment>
{
    public void Configure(EntityTypeBuilder<PolicyAttachment> builder)
    {
        builder.ToTable("PolicyAttachments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
        builder.Property(a => a.StoredPath).IsRequired().HasMaxLength(500);
        builder.Property(a => a.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.UploadedBy).IsRequired().HasMaxLength(100);
        builder.Property(a => a.UploadedAt).IsRequired();

        builder.HasOne(a => a.Policy)
            .WithMany(p => p.Attachments)
            .HasForeignKey(a => a.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.PolicyId);
    }
}
