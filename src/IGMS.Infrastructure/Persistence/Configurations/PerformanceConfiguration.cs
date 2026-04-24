using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class PerformanceReviewConfiguration : IEntityTypeConfiguration<PerformanceReview>
{
    public void Configure(EntityTypeBuilder<PerformanceReview> builder)
    {
        builder.ToTable("PerformanceReviews");

        builder.Property(r => r.Period).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.OverallRating).HasPrecision(3, 2);
        builder.Property(r => r.StrengthsAr).HasColumnType("nvarchar(max)");
        builder.Property(r => r.AreasForImprovementAr).HasColumnType("nvarchar(max)");
        builder.Property(r => r.CommentsAr).HasColumnType("nvarchar(max)");
        builder.Property(r => r.EmployeeCommentsAr).HasColumnType("nvarchar(max)");
        builder.Property(r => r.RejectReason).HasMaxLength(1000);

        builder.HasOne(r => r.Employee)
            .WithMany()
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Department)
            .WithMany()
            .HasForeignKey(r => r.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Goals)
            .WithOne(g => g.Review)
            .HasForeignKey(g => g.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}

public class PerformanceGoalConfiguration : IEntityTypeConfiguration<PerformanceGoal>
{
    public void Configure(EntityTypeBuilder<PerformanceGoal> builder)
    {
        builder.ToTable("PerformanceGoals");

        builder.Property(g => g.TitleAr).HasMaxLength(500).IsRequired();
        builder.Property(g => g.DescriptionAr).HasColumnType("nvarchar(max)");
        builder.Property(g => g.Weight).HasPrecision(5, 2);
        builder.Property(g => g.TargetValue).HasPrecision(18, 4);
        builder.Property(g => g.ActualValue).HasPrecision(18, 4);
        builder.Property(g => g.Rating).HasPrecision(3, 2);
        builder.Property(g => g.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasQueryFilter(g => !g.IsDeleted);
    }
}
