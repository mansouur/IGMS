using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Produces("application/json")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly TenantDbContext    _db;
    private readonly ICurrentUserService _cu;

    public RolesController(TenantDbContext db, ICurrentUserService cu)
    {
        _db = db;
        _cu = cu;
    }

    // ── Lookup (for user form dropdowns) ─────────────────────────────────────

    [HttpGet("lookup")]
    public async Task<IActionResult> GetLookup()
    {
        var roles = await _db.Roles
            .Where(r => r.IsActive && !r.IsDeleted)
            .OrderBy(r => r.Id)
            .Select(r => new { r.Id, r.NameAr, r.NameEn, r.Code })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(roles));
    }

    // ── List all roles ────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _db.Roles
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Id)
            .Select(r => new RoleListDto
            {
                Id              = r.Id,
                NameAr          = r.NameAr,
                NameEn          = r.NameEn,
                Code            = r.Code,
                DescriptionAr   = r.DescriptionAr,
                IsSystemRole    = r.IsSystemRole,
                IsActive        = r.IsActive,
                PermissionCount = r.RolePermissions.Count,
                UserCount       = r.UserRoles.Count(ur => !ur.User.IsDeleted),
            })
            .ToListAsync();

        return Ok(ApiResponse<List<RoleListDto>>.Ok(roles));
    }

    // ── Role detail with permissions ──────────────────────────────────────────

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await _db.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (role is null)
            return NotFound(ApiResponse<object>.NotFound("الدور غير موجود."));

        var dto = new RoleDetailDto
        {
            Id            = role.Id,
            NameAr        = role.NameAr,
            NameEn        = role.NameEn,
            Code          = role.Code,
            DescriptionAr = role.DescriptionAr,
            DescriptionEn = role.DescriptionEn,
            IsSystemRole  = role.IsSystemRole,
            IsActive      = role.IsActive,
            PermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList(),
        };

        return Ok(ApiResponse<RoleDetailDto>.Ok(dto));
    }

    // ── Create role ───────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveRoleRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(GetErrors()));

        var codeTaken = await _db.Roles.AnyAsync(r => r.Code == req.Code.ToUpper() && !r.IsDeleted);
        if (codeTaken)
            return BadRequest(ApiResponse<object>.Fail("الرمز مستخدم بالفعل."));

        var role = new Role
        {
            NameAr        = req.NameAr,
            NameEn        = req.NameEn ?? string.Empty,
            Code          = req.Code.ToUpper(),
            DescriptionAr = req.DescriptionAr,
            DescriptionEn = req.DescriptionEn,
            IsSystemRole  = false,
            IsActive      = true,
            CreatedAt     = DateTime.UtcNow,
            CreatedBy     = _cu.Username,
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = role.Id },
            ApiResponse<object>.Created(new { role.Id }));
    }

    // ── Update role ───────────────────────────────────────────────────────────

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveRoleRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(GetErrors()));

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (role is null)
            return NotFound(ApiResponse<object>.NotFound("الدور غير موجود."));

        if (role.IsSystemRole)
            return BadRequest(ApiResponse<object>.Fail("لا يمكن تعديل الأدوار الأساسية للنظام."));

        role.NameAr        = req.NameAr;
        role.NameEn        = req.NameEn ?? string.Empty;
        role.DescriptionAr = req.DescriptionAr;
        role.DescriptionEn = req.DescriptionEn;
        role.ModifiedAt    = DateTime.UtcNow;
        role.ModifiedBy    = _cu.Username;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null, "تم التحديث."));
    }

    // ── Delete role ───────────────────────────────────────────────────────────

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _db.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (role is null)
            return NotFound(ApiResponse<object>.NotFound("الدور غير موجود."));

        if (role.IsSystemRole)
            return BadRequest(ApiResponse<object>.Fail("لا يمكن حذف الأدوار الأساسية للنظام."));

        if (role.UserRoles.Any())
            return BadRequest(ApiResponse<object>.Fail($"لا يمكن حذف الدور — مرتبط بـ {role.UserRoles.Count} مستخدم."));

        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        role.DeletedBy = _cu.Username;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "تم الحذف."));
    }

    // ── Set permissions for a role ────────────────────────────────────────────

    [HttpPut("{id:int}/permissions")]
    public async Task<IActionResult> SetPermissions(int id, [FromBody] SetRolePermissionsRequest req)
    {
        var role = await _db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (role is null)
            return NotFound(ApiResponse<object>.NotFound("الدور غير موجود."));

        // Validate all permission IDs exist
        var validIds = await _db.Permissions
            .Where(p => req.PermissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        // Remove old permissions
        _db.RolePermissions.RemoveRange(role.RolePermissions);

        // Add new permissions
        var newPerms = validIds.Select(pid => new RolePermission
        {
            RoleId       = id,
            PermissionId = pid,
            GrantedAt    = DateTime.UtcNow,
            GrantedBy    = _cu.Username,
        });

        await _db.RolePermissions.AddRangeAsync(newPerms);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, $"تم تحديث {validIds.Count} صلاحية."));
    }

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class RoleListDto
{
    public int     Id              { get; set; }
    public string  NameAr          { get; set; } = string.Empty;
    public string  NameEn          { get; set; } = string.Empty;
    public string  Code            { get; set; } = string.Empty;
    public string? DescriptionAr   { get; set; }
    public bool    IsSystemRole    { get; set; }
    public bool    IsActive        { get; set; }
    public int     PermissionCount { get; set; }
    public int     UserCount       { get; set; }
}

public class RoleDetailDto : RoleListDto
{
    public string?    DescriptionEn { get; set; }
    public List<int>  PermissionIds { get; set; } = [];
}

public class SaveRoleRequest
{
    [Required, MaxLength(100)]
    public string NameAr { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameEn { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
}

public class SetRolePermissionsRequest
{
    public List<int> PermissionIds { get; set; } = [];
}
