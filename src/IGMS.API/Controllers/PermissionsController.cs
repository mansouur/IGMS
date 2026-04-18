using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/permissions")]
[Produces("application/json")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly TenantDbContext _db;
    public PermissionsController(TenantDbContext db) => _db = db;

    /// <summary>
    /// كل الصلاحيات مجمّعة حسب الوحدة — لواجهة إدارة الأدوار.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var permissions = await _db.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module).ThenBy(p => p.Action)
            .Select(p => new
            {
                p.Id,
                p.Module,
                p.Action,
                p.Code,
                p.DescriptionAr,
                p.DescriptionEn,
            })
            .ToListAsync();

        var grouped = permissions
            .GroupBy(p => p.Module)
            .Select(g => new
            {
                Module      = g.Key,
                Permissions = g.ToList(),
            })
            .OrderBy(g => g.Module)
            .ToList();

        return Ok(ApiResponse<object>.Ok(grouped));
    }
}
