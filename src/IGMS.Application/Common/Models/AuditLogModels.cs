namespace IGMS.Application.Common.Models;

public class AuditLogListDto
{
    public long    Id         { get; set; }
    public string  EntityName { get; set; } = string.Empty;
    public string  EntityId   { get; set; } = string.Empty;
    public string  Action     { get; set; } = string.Empty;
    public string? OldValues  { get; set; }
    public string? NewValues  { get; set; }
    public int?    UserId     { get; set; }
    public string  Username   { get; set; } = string.Empty;
    public string? IpAddress  { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AuditLogQuery
{
    public string?   EntityName { get; set; }
    public string?   EntityId   { get; set; }
    public string?   Action     { get; set; }
    public int?      UserId     { get; set; }
    public DateTime? From       { get; set; }
    public DateTime? To         { get; set; }
    public int       Page       { get; set; } = 1;
    public int       PageSize   { get; set; } = 25;
}
