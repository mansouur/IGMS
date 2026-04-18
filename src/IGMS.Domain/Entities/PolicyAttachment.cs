namespace IGMS.Domain.Entities;

/// <summary>
/// A file (PDF, Word, etc.) attached to a Policy.
/// Not soft-deleted – physical file is removed on delete.
/// </summary>
public class PolicyAttachment
{
    public int    Id            { get; set; }
    public int    PolicyId      { get; set; }
    public Policy Policy        { get; set; } = null!;

    /// <summary>Original file name shown to the user.</summary>
    public string FileName      { get; set; } = string.Empty;

    /// <summary>Relative path on disk (relative to uploads root).</summary>
    public string StoredPath    { get; set; } = string.Empty;

    public string ContentType   { get; set; } = string.Empty;
    public long   FileSizeBytes { get; set; }

    public string   UploadedBy  { get; set; } = string.Empty;
    public DateTime UploadedAt  { get; set; } = DateTime.UtcNow;
}
