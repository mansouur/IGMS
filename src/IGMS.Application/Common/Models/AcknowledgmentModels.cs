namespace IGMS.Application.Common.Models;

public class AcknowledgmentStatusDto
{
    /// <summary>Whether the current user has acknowledged this policy.</summary>
    public bool      HasAcknowledged  { get; set; }
    public DateTime? AcknowledgedAt   { get; set; }

    /// <summary>Total users who have acknowledged (for managers).</summary>
    public int TotalAcknowledged      { get; set; }
}

public class AcknowledgmentRecordDto
{
    public int      Id             { get; set; }
    public int      UserId         { get; set; }
    public string   FullNameAr     { get; set; } = string.Empty;
    public string   Username       { get; set; } = string.Empty;
    public DateTime AcknowledgedAt { get; set; }
}
