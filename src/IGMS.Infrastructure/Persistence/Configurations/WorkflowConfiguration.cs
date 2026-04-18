using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IGMS.Infrastructure.Persistence.Configurations;

public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("WorkflowInstances");

        builder.HasOne(i => i.Definition)
            .WithMany(d => d.Instances)
            .HasForeignKey(i => i.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict to avoid multiple cascade paths through UserProfile
        builder.HasOne(i => i.SubmittedBy)
            .WithMany()
            .HasForeignKey(i => i.SubmittedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}

public class WorkflowInstanceActionConfiguration : IEntityTypeConfiguration<WorkflowInstanceAction>
{
    public void Configure(EntityTypeBuilder<WorkflowInstanceAction> builder)
    {
        builder.ToTable("WorkflowInstanceActions");

        builder.HasOne(a => a.Instance)
            .WithMany(i => i.Actions)
            .HasForeignKey(a => a.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict to avoid multiple cascade paths through UserProfile
        builder.HasOne(a => a.Actor)
            .WithMany()
            .HasForeignKey(a => a.ActorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.Decision)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}

public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("WorkflowDefinitions");
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}

public class WorkflowStageConfiguration : IEntityTypeConfiguration<WorkflowStage>
{
    public void Configure(EntityTypeBuilder<WorkflowStage> builder)
    {
        builder.ToTable("WorkflowStages");

        builder.HasOne(s => s.RequiredRole)
            .WithMany()
            .HasForeignKey(s => s.RequiredRoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
