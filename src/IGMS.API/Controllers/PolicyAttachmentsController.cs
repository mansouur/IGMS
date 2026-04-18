using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/policies/{policyId:int}/attachments")]
[Produces("application/json")]
[Authorize]
public class PolicyAttachmentsController : ControllerBase
{
    private readonly IAttachmentService  _svc;
    private readonly ICurrentUserService _cu;

    public PolicyAttachmentsController(IAttachmentService svc, ICurrentUserService cu)
    {
        _svc = svc;
        _cu  = cu;
    }

    /// <summary>List all attachments for a policy.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(int policyId)
    {
        var items = await _svc.GetByPolicyAsync(policyId);
        return Ok(ApiResponse<List<PolicyAttachmentDto>>.Ok(items));
    }

    /// <summary>Upload a new attachment (multipart/form-data).</summary>
    [HttpPost]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
    public async Task<IActionResult> Upload(int policyId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("الملف مطلوب."));

        await using var stream = file.OpenReadStream();
        var r = await _svc.UploadAsync(
            policyId, stream, file.FileName,
            file.ContentType, file.Length,
            _cu.TenantKey, _cu.Username);

        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return Ok(ApiResponse<PolicyAttachmentDto>.Ok(r.Value!));
    }

    /// <summary>Download an attachment as a file stream.</summary>
    [HttpGet("{attachmentId:int}/download")]
    public async Task<IActionResult> Download(int policyId, int attachmentId)
    {
        var r = await _svc.DownloadAsync(attachmentId);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));

        return File(r.Value!.Data, r.Value!.ContentType, r.Value!.FileName);
    }

    /// <summary>Delete an attachment (removes file from disk too).</summary>
    [HttpDelete("{attachmentId:int}")]
    public async Task<IActionResult> Delete(int policyId, int attachmentId)
    {
        var r = await _svc.DeleteAsync(attachmentId);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<object>.Ok(null));
    }
}
