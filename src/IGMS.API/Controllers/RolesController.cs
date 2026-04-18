using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Produces("application/json")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly TenantDbContext _db;

    public RolesController(TenantDbContext db) => _db = db;

    /// <summary>قائمة الأدوار – تُستخدم في نموذج المستخدم.</summary>
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
}
