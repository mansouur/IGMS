using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PasswordHash).HasMaxLength(256);
        builder.Property(u => u.FullNameAr).IsRequired().HasMaxLength(200);
        builder.Property(u => u.FullNameEn).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.ProfileImagePath).HasMaxLength(500);
        builder.Property(u => u.AdObjectId).HasMaxLength(100);
        builder.Property(u => u.UaePassSub).HasMaxLength(200);
        builder.Property(u => u.EmiratesId).HasMaxLength(20);
        builder.Property(u => u.CreatedBy).HasMaxLength(100);
        builder.Property(u => u.ModifiedBy).HasMaxLength(100);
        builder.Property(u => u.DeletedBy).HasMaxLength(100);

        // Unique constraints
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.EmiratesId).IsUnique().HasFilter("[EmiratesId] IS NOT NULL");
        builder.HasIndex(u => u.UaePassSub).IsUnique().HasFilter("[UaePassSub] IS NOT NULL");
        builder.HasIndex(u => u.AdObjectId).IsUnique().HasFilter("[AdObjectId] IS NOT NULL");

        // Performance indexes
        builder.HasIndex(u => u.DepartmentId);
        builder.HasIndex(u => u.IsActive);
        builder.HasIndex(u => u.IsDeleted);

        // Department FK
        builder.HasOne(u => u.Department)
            .WithMany(d => d.Members)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
