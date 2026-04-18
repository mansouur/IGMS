using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IGMS.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IGMS.Infrastructure.Auth;

/// <summary>
/// Generates JWT tokens with tenant-aware claims.
/// Token payload: UserId, Username, TenantKey, Roles, Language.
/// All values come from the tenant config and user record – no hardcoding.
/// </summary>
public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryHours;

    public JwtService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        _issuer = configuration["Jwt:Issuer"] ?? "IGMS";
        _audience = configuration["Jwt:Audience"] ?? "IGMS.Clients";
        _expiryHours = int.Parse(configuration["Jwt:ExpiryHours"] ?? "8");
    }

    public string GenerateToken(string userId, string username, List<string> roles, List<string> permissions, string tenantKey, string language)
    {
        var claims = BuildClaims(userId, username, roles, permissions, tenantKey, language);
        var credentials = BuildSigningCredentials();
        var expiry = GetTokenExpiry();

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetTokenExpiry() => DateTime.UtcNow.AddHours(_expiryHours);

    private static List<Claim> BuildClaims(
        string userId, string username, List<string> roles, List<string> permissions,
        string tenantKey, string language)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username),
            new("tenant_key", tenantKey),
            new("language", language),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Roles – used by [Authorize(Roles="ADMIN")]
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Permissions – used by [Authorize(Policy="RACI.APPROVE")] (fine-grained)
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        return claims;
    }

    private SigningCredentials BuildSigningCredentials()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }
}
