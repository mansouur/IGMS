using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class RaciParticipantConfiguration : IEntityTypeConfiguration<RaciParticipant>
{
    public void Configure(EntityTypeBuilder<RaciParticipant> builder)
    {
        builder.ToTable("RaciParticipants");

        builder.HasKey(p => new { p.RaciActivityId, p.UserId, p.Role });

        builder.Property(p => p.Role).IsRequired();

        builder.HasOne(p => p.Activity)
            .WithMany(a => a.Participants)
            .HasForeignKey(p => p.RaciActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.RaciActivityId, p.Role });

        // Match the query filter on RaciActivity (required end) to suppress EF warning
        builder.HasQueryFilter(p => !p.Activity.IsDeleted);
    }
}
