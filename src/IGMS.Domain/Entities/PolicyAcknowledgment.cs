namespace IGMS.Domain.Entities;

/// <summary>
/// Records that a specific user has read and acknowledged a policy.
/// One record per (PolicyId, UserId) – re-acknowledging updates the timestamp.
/// Immutable audit proof: never deleted, never soft-deleted.
/// </summary>
public class PolicyAcknowledgment
{
    public int         Id              { get; set; }
    public int         PolicyId        { get; set; }
    public Policy      Policy          { get; set; } = null!;

    public int         UserId          { get; set; }
    public UserProfile User            { get; set; } = null!;

    public DateTime    AcknowledgedAt  { get; set; } = DateTime.UtcNow;
    public string?     IpAddress       { get; set; }
}
