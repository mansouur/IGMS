using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.ToTable("Meetings");

        builder.Property(m => m.TitleAr).HasMaxLength(300).IsRequired();
        builder.Property(m => m.TitleEn).HasMaxLength(300);
        builder.Property(m => m.Location).HasMaxLength(300);
        builder.Property(m => m.AgendaAr).HasColumnType("nvarchar(max)");
        builder.Property(m => m.MinutesAr).HasColumnType("nvarchar(max)");
        builder.Property(m => m.NotesAr).HasMaxLength(2000);

        builder.Property(m => m.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(m => m.Department)
            .WithMany()
            .HasForeignKey(m => m.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Organizer)
            .WithMany()
            .HasForeignKey(m => m.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Attendees)
            .WithOne(a => a.Meeting)
            .HasForeignKey(a => a.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.ActionItems)
            .WithOne(ai => ai.Meeting)
            .HasForeignKey(ai => ai.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}

public class MeetingAttendeeConfiguration : IEntityTypeConfiguration<MeetingAttendee>
{
    public void Configure(EntityTypeBuilder<MeetingAttendee> builder)
    {
        builder.ToTable("MeetingAttendees");
        builder.Property(a => a.RoleInMeeting).HasMaxLength(50);
        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}

public class MeetingActionItemConfiguration : IEntityTypeConfiguration<MeetingActionItem>
{
    public void Configure(EntityTypeBuilder<MeetingActionItem> builder)
    {
        builder.ToTable("MeetingActionItems");
        builder.Property(ai => ai.TitleAr).HasMaxLength(500).IsRequired();
        builder.Property(ai => ai.DescriptionAr).HasMaxLength(2000);
        builder.HasOne(ai => ai.AssignedTo)
            .WithMany()
            .HasForeignKey(ai => ai.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(ai => !ai.IsDeleted);
    }
}
