using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;
using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IGMS.Infrastructure.Auth;

/// <summary>
/// UAE Pass OAuth 2.0 / OpenID Connect integration.
///
/// Sandbox:    stg-id.uaepass.ae  (current)
/// Production: id.uaepass.ae      (change UaePass:BaseUrl in config only)
///
/// Scope: urn:uae:digitalid:profile:general
/// Flow:  Authorization Code  (Basic Auth on token endpoint)
/// </summary>
public class UaePassService : IUaePassService
{
    private readonly string _baseUrl;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;
    private readonly HttpClient _httpClient;
    private readonly ILogger<UaePassService> _logger;

    // UAE Pass sandbox public default credentials
    // Replace with tenant-specific credentials upon contract signing
    private const string SandboxClientId     = "sandbox_stage";
    private const string SandboxClientSecret = "sandbox_stage";

    public UaePassService(IConfiguration configuration, HttpClient httpClient, ILogger<UaePassService> logger)
    {
        _baseUrl      = configuration["UaePass:BaseUrl"]      ?? "https://stg-id.uaepass.ae";
        _clientId     = configuration["UaePass:ClientId"]     ?? SandboxClientId;
        _clientSecret = configuration["UaePass:ClientSecret"] ?? SandboxClientSecret;
        _redirectUri  = configuration["UaePass:RedirectUri"]  ?? "http://localhost:5257/api/v1/auth/uaepass/callback";
        _httpClient   = httpClient;
        _logger       = logger;
    }

    /// <summary>
    /// Builds the UAE Pass authorization URL.
    /// State is a random GUID stored in session to prevent CSRF.
    /// </summary>
    public string BuildAuthorizationUrl(string state, string language)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["response_type"] = "code";
        query["client_id"]     = _clientId;
        query["redirect_uri"]  = _redirectUri;
        query["scope"]         = "urn:uae:digitalid:profile:general";
        query["state"]         = state;
        query["ui_locales"]    = language == "ar" ? "ar" : "en";
        query["acr_values"]    = "urn:safelayer:tws:policies:authentication:level:low";

        return $"{_baseUrl}/idshub/authorize?{query}";
    }

    /// <summary>
    /// Builds the UAE Pass logout URL.
    /// Browser navigates here to end the SSO session, then gets redirected back to our app.
    /// </summary>
    public string BuildLogoutUrl(string postLogoutRedirectUri) =>
        $"{_baseUrl}/idshub/logout?redirect_uri={Uri.EscapeDataString(postLogoutRedirectUri)}";

    /// <summary>
    /// Exchanges authorization code for user info.
    /// Called from the callback endpoint after UAE Pass redirects back.
    /// </summary>
    public async Task<Result<UaePassUserInfo>> ExchangeCodeAsync(string code)
    {
        try
        {
            // Step 1: Exchange code for access token
            var tokenResponse = await FetchTokenAsync(code);
            if (tokenResponse is null)
                return Result<UaePassUserInfo>.Failure("فشل الحصول على الـ token من UAE Pass.");

            // Step 2: Fetch user profile using access token
            var userInfo = await FetchUserInfoAsync(tokenResponse.AccessToken);
            if (userInfo is null)
                return Result<UaePassUserInfo>.Failure("فشل الحصول على بيانات المستخدم من UAE Pass.");

            return Result<UaePassUserInfo>.Success(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UAE Pass exchange code error");
            return Result<UaePassUserInfo>.Failure("خطأ في الاتصال بـ UAE Pass. حاول مجدداً.");
        }
    }

    private async Task<UaePassTokenResponse?> FetchTokenAsync(string code)
    {
        var url         = $"{_baseUrl}/idshub/token";
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));

        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, url);
        tokenRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        // OAuth 2.0 standard: params in body as application/x-www-form-urlencoded
        tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"]   = "authorization_code",
            ["code"]         = code,
            ["redirect_uri"] = _redirectUri,
        });

        var response = await _httpClient.SendAsync(tokenRequest);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("UAE Pass token endpoint returned {Status}: {Body}", response.StatusCode, body);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<UaePassTokenResponse>();
    }

    private async Task<UaePassUserInfo?> FetchUserInfoAsync(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/idshub/userinfo");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("UAE Pass userinfo endpoint returned {Status}", response.StatusCode);
            return null;
        }

        var raw = await response.Content.ReadFromJsonAsync<UaePassRawUserInfo>();
        if (raw is null) return null;

        return new UaePassUserInfo
        {
            Sub           = raw.Sub ?? string.Empty,
            EmiratesId    = raw.Idn ?? string.Empty,
            FullNameAr    = raw.FullnameAR ?? raw.FirstnameAR ?? string.Empty,
            FullNameEn    = raw.FullnameEN ?? raw.FirstnameEN ?? string.Empty,
            Email         = raw.Email ?? string.Empty,
            Mobile        = raw.Mobile ?? string.Empty,
            Gender        = raw.Gender ?? string.Empty,
            Nationality   = raw.NationalityEN ?? string.Empty
        };
    }

    // Internal DTO models matching UAE Pass API response shape
    private class UaePassTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    private class UaePassRawUserInfo
    {
        [JsonPropertyName("sub")]         public string? Sub { get; set; }
        [JsonPropertyName("idn")]         public string? Idn { get; set; }
        [JsonPropertyName("fullnameAR")]  public string? FullnameAR { get; set; }
        [JsonPropertyName("fullnameEN")]  public string? FullnameEN { get; set; }
        [JsonPropertyName("firstnameAR")] public string? FirstnameAR { get; set; }
        [JsonPropertyName("firstnameEN")] public string? FirstnameEN { get; set; }
        [JsonPropertyName("email")]       public string? Email { get; set; }
        [JsonPropertyName("mobile")]      public string? Mobile { get; set; }
        [JsonPropertyName("gender")]      public string? Gender { get; set; }
        [JsonPropertyName("nationalityEN")] public string? NationalityEN { get; set; }
    }
}
