using System.ComponentModel.DataAnnotations;

namespace IGMS.Application.Common.Models;

// ─── List DTO ─────────────────────────────────────────────────────────────────

public class UserListDto
{
    public int     Id             { get; set; }
    public string  Username       { get; set; } = string.Empty;
    public string  FullNameAr     { get; set; } = string.Empty;
    public string  FullNameEn     { get; set; } = string.Empty;
    public string  Email          { get; set; } = string.Empty;
    public bool    IsActive       { get; set; }
    public int?    DepartmentId   { get; set; }
    public string? DepartmentNameAr { get; set; }
    public List<string> Roles     { get; set; } = [];
    public DateTime CreatedAt     { get; set; }
    public DateTime? LastLoginAt  { get; set; }
}

// ─── Detail DTO ───────────────────────────────────────────────────────────────

public class UserDetailDto : UserListDto
{
    public string? PhoneNumber    { get; set; }
    public string? AdObjectId     { get; set; }
    public string? UaePassSub     { get; set; }
    public string? EmiratesId     { get; set; }
}

// ─── Requests ─────────────────────────────────────────────────────────────────

public class CreateUserRequest
{
    [Required] public string Username   { get; set; } = string.Empty;
    [Required] public string FullNameAr { get; set; } = string.Empty;
    [Required] public string FullNameEn { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;

    /// <summary>Required for Local auth. Leave empty for AD/UaePass-only accounts.</summary>
    public string? Password     { get; set; }
    public string? PhoneNumber  { get; set; }
    public int?    DepartmentId { get; set; }
    public List<int> RoleIds    { get; set; } = [];
    public bool    IsActive     { get; set; } = true;
}

public class UpdateUserRequest
{
    public int     Id           { get; set; }
    [Required] public string FullNameAr { get; set; } = string.Empty;
    [Required] public string FullNameEn { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string? PhoneNumber  { get; set; }
    public int?    DepartmentId { get; set; }
    public List<int> RoleIds    { get; set; } = [];
    public bool    IsActive     { get; set; } = true;
}

// ─── Query ────────────────────────────────────────────────────────────────────

public class UserQuery
{
    public int     Page         { get; set; } = 1;
    public int     PageSize     { get; set; } = 20;
    public string? Search       { get; set; }
    public int?    DepartmentId { get; set; }
    public bool?   IsActive     { get; set; }
}
