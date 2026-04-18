using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>
/// ملف دليل مرفوع على اختبار ضابط رقابي.
/// </summary>
public class ControlEvidence : AuditableEntity
{
    public int         ControlTestId { get; set; }
    public ControlTest ControlTest   { get; set; } = null!;

    public string  FileName      { get; set; } = string.Empty;
    public string  StoredPath    { get; set; } = string.Empty;
    public string  ContentType   { get; set; } = string.Empty;
    public long    FileSizeBytes { get; set; }
    public string  UploadedBy    { get; set; } = string.Empty;
    public DateTime UploadedAt   { get; set; } = DateTime.UtcNow;
}
