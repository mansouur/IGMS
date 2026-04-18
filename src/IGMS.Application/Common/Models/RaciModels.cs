using IGMS.Domain.Entities;

namespace IGMS.Application.Common.Models;

// ─── List DTO (عرض خفيف في القائمة) ─────────────────────────────────────────

public class RaciMatrixListDto
{
    public int    Id             { get; set; }
    public string TitleAr        { get; set; } = string.Empty;
    public string TitleEn        { get; set; } = string.Empty;
    public string? DepartmentAr  { get; set; }
    public string? DepartmentEn  { get; set; }
    public RaciStatus Status     { get; set; }
    public string StatusLabel    { get; set; } = string.Empty;
    public int    ActivityCount  { get; set; }
    public string CreatedBy      { get; set; } = string.Empty;
    public DateTime CreatedAt    { get; set; }
    public DateTime? ApprovedAt  { get; set; }
}

// ─── Detail DTO (عرض كامل مع الأنشطة) ───────────────────────────────────────

public class RaciMatrixDetailDto : RaciMatrixListDto
{
    public string? DescriptionAr      { get; set; }
    public string? DescriptionEn      { get; set; }
    public string? ApprovedByName     { get; set; }
    public List<RaciActivityDto> Activities { get; set; } = [];
}

public class RaciActivityDto
{
    public int    Id               { get; set; }
    public string NameAr           { get; set; } = string.Empty;
    public string NameEn           { get; set; } = string.Empty;
    public int    DisplayOrder      { get; set; }
    // R: متعدد (مخزّن في Participants)
    public List<UserRefDto> Responsible { get; set; } = [];
    // A: واحد فقط (FK مباشر على النشاط)
    public UserRefDto? Accountable  { get; set; }
    public List<UserRefDto> Consulted { get; set; } = [];
    public List<UserRefDto> Informed  { get; set; } = [];
}

public class UserRefDto
{
    public int    Id          { get; set; }
    public string FullNameAr  { get; set; } = string.Empty;
    public string FullNameEn  { get; set; } = string.Empty;
    public string Username    { get; set; } = string.Empty;
}

// ─── Create / Update Requests ────────────────────────────────────────────────

public class CreateRaciMatrixRequest
{
    public string TitleAr         { get; set; } = string.Empty;
    public string TitleEn         { get; set; } = string.Empty;
    public string? DescriptionAr  { get; set; }
    public string? DescriptionEn  { get; set; }
    public int? DepartmentId      { get; set; }
    public List<CreateRaciActivityRequest> Activities { get; set; } = [];
}

public class UpdateRaciMatrixRequest : CreateRaciMatrixRequest
{
    public int Id { get; set; }
}

public class CreateRaciActivityRequest
{
    public string NameAr              { get; set; } = string.Empty;
    public string NameEn              { get; set; } = string.Empty;
    public int    DisplayOrder         { get; set; }
    // R: متعدد
    public List<int> ResponsibleUserIds { get; set; } = [];
    // A: واحد فقط
    public int?   AccountableUserId   { get; set; }
    public List<int> ConsultedUserIds  { get; set; } = [];
    public List<int> InformedUserIds   { get; set; } = [];
}

// ─── Query / Filter ───────────────────────────────────────────────────────────

public class RaciMatrixQuery
{
    public int    Page          { get; set; } = 1;
    public int    PageSize      { get; set; } = 10;
    public string? Search       { get; set; }
    public RaciStatus? Status   { get; set; }
    public int? DepartmentId    { get; set; }
}
