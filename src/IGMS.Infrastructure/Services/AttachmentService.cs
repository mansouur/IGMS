using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly TenantDbContext _db;
    private readonly string          _uploadsRoot;

    private static readonly HashSet<string> _allowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "image/png", "image/jpeg", "image/gif",
    };

    private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

    public AttachmentService(TenantDbContext db, IWebHostEnvironment env)
    {
        _db          = db;
        _uploadsRoot = Path.Combine(env.ContentRootPath, "uploads");
    }

    public async Task<Result<PolicyAttachmentDto>> UploadAsync(
        int policyId, Stream stream, string fileName,
        string contentType, long fileSize,
        string tenantKey, string uploadedBy)
    {
        if (fileSize == 0)
            return Result<PolicyAttachmentDto>.Failure("الملف فارغ.");

        if (fileSize > MaxFileSizeBytes)
            return Result<PolicyAttachmentDto>.Failure("حجم الملف يتجاوز 20 ميغابايت.");

        if (!_allowedTypes.Contains(contentType))
            return Result<PolicyAttachmentDto>.Failure("نوع الملف غير مسموح به.");

        var policyExists = await _db.Policies.AnyAsync(p => p.Id == policyId && !p.IsDeleted);
        if (!policyExists)
            return Result<PolicyAttachmentDto>.Failure("السياسة غير موجودة.");

        // Build storage path: uploads/{tenantKey}/policies/{policyId}/{guid}_{safeName}
        var safeFileName = Path.GetFileName(fileName); // strip directory traversal
        var storedName   = $"{Guid.NewGuid():N}_{safeFileName}";
        var dir          = Path.Combine(_uploadsRoot, tenantKey, "policies", policyId.ToString());
        Directory.CreateDirectory(dir);

        var fullPath   = Path.Combine(dir, storedName);
        var storedPath = Path.Combine(tenantKey, "policies", policyId.ToString(), storedName);

        await using (var fs = File.Create(fullPath))
            await stream.CopyToAsync(fs);

        var attachment = new PolicyAttachment
        {
            PolicyId      = policyId,
            FileName      = fileName,
            StoredPath    = storedPath,
            ContentType   = contentType,
            FileSizeBytes = fileSize,
            UploadedBy    = uploadedBy,
            UploadedAt    = DateTime.UtcNow,
        };

        _db.PolicyAttachments.Add(attachment);
        await _db.SaveChangesAsync();

        return Result<PolicyAttachmentDto>.Success(Map(attachment));
    }

    public async Task<List<PolicyAttachmentDto>> GetByPolicyAsync(int policyId) =>
        await _db.PolicyAttachments
            .AsNoTracking()
            .Where(a => a.PolicyId == policyId)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new PolicyAttachmentDto
            {
                Id            = a.Id,
                PolicyId      = a.PolicyId,
                FileName      = a.FileName,
                ContentType   = a.ContentType,
                FileSizeBytes = a.FileSizeBytes,
                UploadedBy    = a.UploadedBy,
                UploadedAt    = a.UploadedAt,
            })
            .ToListAsync();

    public async Task<Result<DownloadResult>> DownloadAsync(int attachmentId)
    {
        var att = await _db.PolicyAttachments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (att is null)
            return Result<DownloadResult>.Failure("المرفق غير موجود.");

        var fullPath = Path.Combine(_uploadsRoot, att.StoredPath);
        if (!File.Exists(fullPath))
            return Result<DownloadResult>.Failure("الملف غير موجود على الخادم.");

        var data = await File.ReadAllBytesAsync(fullPath);
        return Result<DownloadResult>.Success(new DownloadResult
        {
            Data        = data,
            ContentType = att.ContentType,
            FileName    = att.FileName,
        });
    }

    public async Task<Result<bool>> DeleteAsync(int attachmentId)
    {
        var att = await _db.PolicyAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId);
        if (att is null)
            return Result<bool>.Failure("المرفق غير موجود.");

        // Delete physical file
        var fullPath = Path.Combine(_uploadsRoot, att.StoredPath);
        if (File.Exists(fullPath)) File.Delete(fullPath);

        _db.PolicyAttachments.Remove(att);
        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static PolicyAttachmentDto Map(PolicyAttachment a) => new()
    {
        Id            = a.Id,
        PolicyId      = a.PolicyId,
        FileName      = a.FileName,
        ContentType   = a.ContentType,
        FileSizeBytes = a.FileSizeBytes,
        UploadedBy    = a.UploadedBy,
        UploadedAt    = a.UploadedAt,
    };
}
