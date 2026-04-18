using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.Property(ur => ur.AssignedBy).IsRequired().HasMaxLength(100);
        builder.Property(ur => ur.AssignedAt).IsRequired();

        builder.HasIndex(ur => ur.RoleId);
        builder.HasIndex(ur => ur.ExpiresAt);

        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Match the query filter on the required end (Role) to avoid filtered-out surprises
        builder.HasQueryFilter(ur => !ur.Role.IsDeleted && !ur.User.IsDeleted);
    }
}
