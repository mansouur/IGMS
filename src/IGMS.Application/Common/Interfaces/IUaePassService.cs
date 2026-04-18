using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Handles UAE Pass OAuth 2.0 / OpenID Connect flow.
/// Sandbox: stg-id.uaepass.ae
/// Production: id.uaepass.ae  (swap base URL in config only)
/// </summary>
public interface IUaePassService
{
    /// <summary>Builds the authorization URL to redirect the user to UAE Pass.</summary>
    string BuildAuthorizationUrl(string state, string language);

    /// <summary>Exchanges the authorization code for a UAE Pass user profile.</summary>
    Task<Result<UaePassUserInfo>> ExchangeCodeAsync(string code);
}

public class UaePassUserInfo
{
    public string Sub { get; set; } = string.Empty;       // UAE Pass unique ID
    public string EmiratesId { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
}
