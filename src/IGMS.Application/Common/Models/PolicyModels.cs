using System.ComponentModel.DataAnnotations;
using IGMS.Domain.Entities;

namespace IGMS.Application.Common.Models;

public class PolicyListDto
{
    public int            Id               { get; set; }
    public string         TitleAr          { get; set; } = string.Empty;
    public string         TitleEn          { get; set; } = string.Empty;
    public string         Code             { get; set; } = string.Empty;
    public PolicyCategory Category         { get; set; }
    public PolicyStatus   Status           { get; set; }
    public DateTime?      EffectiveDate    { get; set; }
    public DateTime?      ExpiryDate       { get; set; }
    public string?        DepartmentNameAr { get; set; }
    public string?        OwnerNameAr      { get; set; }
    public DateTime       CreatedAt        { get; set; }
}

public class PolicyDetailDto : PolicyListDto
{
    public string?   DescriptionAr  { get; set; }
    public string?   DescriptionEn  { get; set; }
    public int?      DepartmentId   { get; set; }
    public int?      OwnerId        { get; set; }
    public int?      ApproverId     { get; set; }
    public string?   ApproverNameAr { get; set; }
    public DateTime? ApprovedAt     { get; set; }
}

public class SavePolicyRequest
{
    public int    Id    { get; set; }
    [Required] public string TitleAr  { get; set; } = string.Empty;
    public string TitleEn             { get; set; } = string.Empty;
    [Required] public string Code     { get; set; } = string.Empty;
    public string?        DescriptionAr { get; set; }
    public string?        DescriptionEn { get; set; }
    public PolicyCategory Category      { get; set; }
    public PolicyStatus   Status        { get; set; }
    public DateTime?      EffectiveDate { get; set; }
    public DateTime?      ExpiryDate    { get; set; }
    public int?           DepartmentId  { get; set; }
    public int?           OwnerId       { get; set; }
    public int?           ApproverId    { get; set; }
}

public class PolicyQuery
{
    public int             Page     { get; set; } = 1;
    public int             PageSize { get; set; } = 20;
    public string?         Search   { get; set; }
    public PolicyStatus?   Status   { get; set; }
    public PolicyCategory? Category { get; set; }
}

/// <summary>Lightweight entry in the policy version history list.</summary>
public class PolicyVersionDto
{
    public int           Id             { get; set; }
    public string        Code           { get; set; } = string.Empty;
    public string        TitleAr        { get; set; } = string.Empty;
    public PolicyStatus  Status         { get; set; }
    public DateTime      CreatedAt      { get; set; }
    public DateTime?     ApprovedAt     { get; set; }
    public string?       ApproverNameAr { get; set; }
    public DateTime?     EffectiveDate  { get; set; }
    public DateTime?     ExpiryDate     { get; set; }
    public bool          IsCurrent      { get; set; }
    public int           Version        { get; set; }  // 1 = original, 2 = first renewal…
}
