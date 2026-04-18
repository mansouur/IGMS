namespace IGMS.Application.Common.Models;

// ─── List DTO ─────────────────────────────────────────────────────────────────

public class DepartmentListDto
{
    public int     Id            { get; set; }
    public string  NameAr        { get; set; } = string.Empty;
    public string  NameEn        { get; set; } = string.Empty;
    public string  Code          { get; set; } = string.Empty;
    public int     Level         { get; set; }
    public bool    IsActive      { get; set; }
    public int?    ParentId      { get; set; }
    public string? ParentNameAr  { get; set; }
    public string? ManagerNameAr { get; set; }
    public int     ChildCount    { get; set; }
    public int     MemberCount   { get; set; }
    public DateTime CreatedAt    { get; set; }
}

// ─── Detail DTO ───────────────────────────────────────────────────────────────

public class DepartmentDetailDto : DepartmentListDto
{
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public int?    ManagerId     { get; set; }
    public string? ManagerNameEn { get; set; }
    public List<DepartmentListDto> Children { get; set; } = [];
}

// ─── Tree (للعرض الشجري في الـ UI) ──────────────────────────────────────────

public class DepartmentTreeDto
{
    public int     Id       { get; set; }
    public string  NameAr   { get; set; } = string.Empty;
    public string  NameEn   { get; set; } = string.Empty;
    public string  Code     { get; set; } = string.Empty;
    public int     Level    { get; set; }
    public bool    IsActive { get; set; }
    public int?    ParentId { get; set; }
    public List<DepartmentTreeDto> Children { get; set; } = [];
}

// ─── Requests ─────────────────────────────────────────────────────────────────

public class CreateDepartmentRequest
{
    public string  NameAr        { get; set; } = string.Empty;
    public string  NameEn        { get; set; } = string.Empty;
    public string  Code          { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public int?    ParentId      { get; set; }
    public int?    ManagerId     { get; set; }
    public bool    IsActive      { get; set; } = true;
}

public class UpdateDepartmentRequest : CreateDepartmentRequest
{
    public int Id { get; set; }
}

// ─── Query ────────────────────────────────────────────────────────────────────

public class DepartmentQuery
{
    public int     Page     { get; set; } = 1;
    public int     PageSize { get; set; } = 20;
    public string? Search   { get; set; }
    public int?    ParentId { get; set; }
    public bool?   IsActive { get; set; }
}
