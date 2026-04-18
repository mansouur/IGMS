namespace IGMS.Application.Common.Models;

public class PolicyAttachmentDto
{
    public int      Id            { get; set; }
    public int      PolicyId      { get; set; }
    public string   FileName      { get; set; } = string.Empty;
    public string   ContentType   { get; set; } = string.Empty;
    public long     FileSizeBytes { get; set; }
    public string   UploadedBy    { get; set; } = string.Empty;
    public DateTime UploadedAt    { get; set; }

    /// <summary>Human-readable size e.g. "1.2 MB"</summary>
    public string FileSizeDisplay => FileSizeBytes switch
    {
        >= 1_048_576 => $"{FileSizeBytes / 1_048_576.0:F1} MB",
        >= 1_024     => $"{FileSizeBytes / 1_024.0:F0} KB",
        _            => $"{FileSizeBytes} B",
    };
}

public class DownloadResult
{
    public byte[] Data        { get; init; } = [];
    public string ContentType { get; init; } = "application/octet-stream";
    public string FileName    { get; init; } = string.Empty;
}
