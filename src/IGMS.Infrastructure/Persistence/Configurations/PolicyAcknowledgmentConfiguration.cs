using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class PolicyAcknowledgmentConfiguration : IEntityTypeConfiguration<PolicyAcknowledgment>
{
    public void Configure(EntityTypeBuilder<PolicyAcknowledgment> builder)
    {
        builder.ToTable("PolicyAcknowledgments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AcknowledgedAt).IsRequired();
        builder.Property(a => a.IpAddress).HasMaxLength(50);

        // One acknowledgment per (Policy, User) – upsert on re-acknowledge
        builder.HasIndex(a => new { a.PolicyId, a.UserId }).IsUnique();

        builder.HasOne(a => a.Policy)
            .WithMany()
            .HasForeignKey(a => a.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
