using System.Text.Json;
using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IGMS.Infrastructure.Persistence;

/// <summary>
/// Captures entity change state before/after EF SaveChanges for audit logging.
/// For Added entries the EntityId is only available after the insert completes,
/// so we hold the EntityEntry reference and resolve it post-save.
/// </summary>
internal sealed class AuditEntry
{
    private readonly EntityEntry _entry;

    public string       EntityName { get; }
    public string       Action     { get; }
    public string?      OldValues  { get; }
    public string?      NewValues  { get; private set; }

    /// <summary>True when the primary key is database-generated and not yet populated.</summary>
    public bool NeedsIdAfterSave { get; }

    public string EntityId { get; private set; } = string.Empty;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    // Properties that should never appear in audit snapshots
    private static readonly HashSet<string> _excluded = new(StringComparer.OrdinalIgnoreCase)
    {
        "ConcurrencyStamp", "PasswordHash", "SecurityStamp",
    };

    public AuditEntry(EntityEntry entry)
    {
        _entry = entry;
        EntityName = entry.Entity.GetType().Name;

        switch (entry.State)
        {
            case EntityState.Added:
                Action  = "Created";
                NeedsIdAfterSave = true;
                NewValues = null; // resolved post-save in Resolve()
                break;

            case EntityState.Deleted:
                Action    = "Deleted";
                EntityId  = GetKeyValue(entry);
                OldValues = CaptureProperties(entry.OriginalValues);
                break;

            default: // Modified
                Action    = "Updated";
                EntityId  = GetKeyValue(entry);
                OldValues = CaptureChangedOriginals(entry);
                NewValues = CaptureChangedCurrent(entry);
                break;
        }
    }

    /// <summary>
    /// Called after base.SaveChangesAsync() for Added entries.
    /// At this point the database-generated key is populated.
    /// </summary>
    public void Resolve()
    {
        EntityId  = GetKeyValue(_entry);
        NewValues = CaptureProperties(_entry.CurrentValues);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string GetKeyValue(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null) return string.Empty;

        var values = key.Properties
            .Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "")
            .ToArray();

        return values.Length == 1 ? values[0] : string.Join("|", values);
    }

    private static string? CaptureProperties(PropertyValues values)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in values.Properties)
        {
            if (_excluded.Contains(prop.Name)) continue;
            dict[prop.Name] = values[prop];
        }
        return dict.Count > 0 ? JsonSerializer.Serialize(dict, _jsonOpts) : null;
    }

    /// <summary>Old values only for properties that actually changed.</summary>
    private static string? CaptureChangedOriginals(EntityEntry entry)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            if (_excluded.Contains(prop.Metadata.Name)) continue;
            if (!prop.IsModified) continue;
            dict[prop.Metadata.Name] = prop.OriginalValue;
        }
        return dict.Count > 0 ? JsonSerializer.Serialize(dict, _jsonOpts) : null;
    }

    /// <summary>New values only for properties that actually changed.</summary>
    private static string? CaptureChangedCurrent(EntityEntry entry)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            if (_excluded.Contains(prop.Metadata.Name)) continue;
            if (!prop.IsModified) continue;
            dict[prop.Metadata.Name] = prop.CurrentValue;
        }
        return dict.Count > 0 ? JsonSerializer.Serialize(dict, _jsonOpts) : null;
    }
}
