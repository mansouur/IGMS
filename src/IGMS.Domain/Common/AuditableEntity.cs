namespace IGMS.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Provides audit tracking and soft-delete support.
/// All entities inherit from this – never delete records physically.
/// </summary>
public abstract class AuditableEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    // Soft delete – set to true instead of removing the row
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
