using System.Security.Cryptography;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Documents;
using DriveOS.Modules.Students.Domain.Documents;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Documents;

internal sealed class StudentDocumentService(
    StudentsDbContext db,
    IClock clock,
    IStudentDocumentStorage storage,
    IStudentDocumentSecurityScanner scanner
) : IStudentDocumentService
{
    private const int MaxBytes = 15 * 1024 * 1024;

    public async Task<StudentDocumentListResponse?> GetAsync(
        GetStudentDocumentsQuery q,
        CancellationToken ct = default
    )
    {
        if (
            !await db
                .Students.AsNoTracking()
                .AnyAsync(x => x.OrganizationId == q.OrganizationId && x.Id == q.StudentId, ct)
        )
            return null;
        var docs = await db
            .StudentDocuments.AsNoTracking()
            .Include(x => x.Versions)
            .Where(x =>
                x.OrganizationId == q.OrganizationId
                && x.StudentId == q.StudentId
                && (!q.EnrollmentId.HasValue || x.EnrollmentId == q.EnrollmentId)
            )
            .OrderBy(x => x.Category)
            .ThenBy(x => x.DocumentType)
            .ToListAsync(ct);
        DateOnly today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        return new(q.StudentId.Value, docs.Select(x => Map(x, today)).ToArray());
    }

    public async Task<Result<Guid>> RequestAsync(
        RequestStudentDocumentCommand x,
        CancellationToken ct = default
    )
    {
        if (
            !await db
                .Students.AsNoTracking()
                .AnyAsync(s => s.OrganizationId == x.OrganizationId && s.Id == x.StudentId, ct)
        )
            return Result.Failure<Guid>(StudentDocumentApplicationErrors.StudentNotFound);
        if (
            x.EnrollmentId.HasValue
            && !await db
                .Enrollments.AsNoTracking()
                .AnyAsync(
                    e =>
                        e.OrganizationId == x.OrganizationId
                        && e.StudentId == x.StudentId
                        && e.Id == x.EnrollmentId.Value,
                    ct
                )
        )
            return Result.Failure<Guid>(StudentDocumentApplicationErrors.EnrollmentNotFound);
        var r = StudentDocument.Request(
            x.OrganizationId,
            x.StudentId,
            x.EnrollmentId,
            x.DocumentType,
            x.Category,
            x.Visibility,
            x.ExpiresOn,
            x.ActorUserId,
            clock.UtcNow
        );
        if (r.IsFailure)
            return Result.Failure<Guid>(r.Error);
        db.StudentDocuments.Add(r.Value);
        await db.SaveChangesAsync(ct);
        return Result.Success(r.Value.Id.Value);
    }

    public async Task<Result<Guid>> UploadAsync(
        UploadStudentDocumentCommand x,
        CancellationToken ct = default
    )
    {
        if (x.Length is <= 0 or > MaxBytes)
            return Result.Failure<Guid>(StudentDocumentErrors.FileTooLarge);
        var document = await Find(x.OrganizationId, x.StudentId, x.DocumentId, ct);
        if (document is null)
            return Result.Failure<Guid>(StudentDocumentErrors.NotFound);
        using var buffer = new MemoryStream((int)x.Length);
        await x.Content.CopyToAsync(buffer, ct);
        if (buffer.Length != x.Length || buffer.Length > MaxBytes)
            return Result.Failure<Guid>(StudentDocumentErrors.FileTooLarge);
        byte[] bytes = buffer.ToArray();
        if (!await scanner.IsSafeAsync(x.FileName, x.ContentType, bytes, ct))
            return Result.Failure<Guid>(StudentDocumentErrors.FileUnsafe);
        string checksum = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        Guid storageVersion = Guid.NewGuid();
        await using var input = new MemoryStream(bytes, false);
        string reference = await storage.StoreAsync(
            x.OrganizationId,
            x.DocumentId,
            storageVersion,
            input,
            ct
        );
        var result = document.AddVersion(
            Path.GetFileName(x.FileName),
            x.ContentType,
            x.Length,
            checksum,
            reference,
            x.ActorUserId,
            clock.UtcNow
        );
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public Task<Result> ValidateAsync(
        ValidateStudentDocumentCommand x,
        CancellationToken ct = default
    ) =>
        Change(
            x.OrganizationId,
            x.StudentId,
            x.DocumentId,
            d => d.Validate(x.Approve, x.Reason ?? string.Empty, x.ActorUserId, clock.UtcNow),
            ct
        );

    public Task<Result> ShareAsync(ShareStudentDocumentCommand x, CancellationToken ct = default) =>
        Change(
            x.OrganizationId,
            x.StudentId,
            x.DocumentId,
            d => d.Share(x.Visibility, x.ActorUserId, clock.UtcNow),
            ct
        );

    public Task<Result> ArchiveAsync(
        ArchiveStudentDocumentCommand x,
        CancellationToken ct = default
    ) =>
        Change(
            x.OrganizationId,
            x.StudentId,
            x.DocumentId,
            d => d.Archive(x.Reason, x.ActorUserId, clock.UtcNow),
            ct
        );

    public async Task<Result<StudentDocumentDownload>> DownloadAsync(
        DownloadStudentDocumentQuery q,
        CancellationToken ct = default
    )
    {
        var document = await Find(q.OrganizationId, q.StudentId, q.DocumentId, ct);
        if (document is null)
            return Result.Failure<StudentDocumentDownload>(StudentDocumentErrors.NotFound);
        var version = q.Version.HasValue
            ? document.Versions.SingleOrDefault(v => v.VersionNumber == q.Version)
            : document.Versions.SingleOrDefault(v => v.IsCurrent);
        if (version is null)
            return Result.Failure<StudentDocumentDownload>(StudentDocumentErrors.VersionNotFound);
        Stream? content = await storage.OpenReadAsync(version.StorageReference, ct);
        if (content is null)
            return Result.Failure<StudentDocumentDownload>(
                StudentDocumentApplicationErrors.FileUnavailable
            );
        var logged = document.LogDownload(version.Id, q.ActorUserId, clock.UtcNow);
        if (logged.IsSuccess)
            await db.SaveChangesAsync(ct);
        return Result.Success(
            new StudentDocumentDownload(
                content,
                version.FileName,
                version.ContentType,
                version.SizeBytes
            )
        );
    }

    private async Task<Result> Change(
        OrganizationId org,
        PersonId studentId,
        Guid id,
        Func<StudentDocument, Result> action,
        CancellationToken ct
    )
    {
        var d = await Find(org, studentId, id, ct);
        if (d is null)
            return Result.Failure(StudentDocumentErrors.NotFound);
        var r = action(d);
        if (r.IsSuccess)
            await db.SaveChangesAsync(ct);
        return r;
    }

    private Task<StudentDocument?> Find(
        OrganizationId org,
        PersonId studentId,
        Guid id,
        CancellationToken ct
    ) =>
        db
            .StudentDocuments.Include(x => x.Versions)
            .Include(x => x.AccessLogs)
            .SingleOrDefaultAsync(
                x => x.OrganizationId == org && x.StudentId == studentId && x.Id == new StudentDocumentId(id),
                ct
            );

    private static StudentDocumentItem Map(StudentDocument x, DateOnly today)
    {
        var current = x.Versions.SingleOrDefault(v => v.IsCurrent);
        var status = x.Status;
        if (status == StudentDocumentStatus.Approved && x.ExpiresOn.HasValue)
            status =
                x.ExpiresOn < today ? StudentDocumentStatus.Expired
                : x.ExpiresOn <= today.AddDays(30) ? StudentDocumentStatus.Expiring
                : status;
        return new(
            x.Id,
            x.EnrollmentId?.Value,
            x.DocumentType,
            x.Category,
            status,
            x.CurrentVersion,
            current?.UploadedAtUtc,
            x.ExpiresOn,
            x.Visibility,
            x.DecisionReason
        );
    }
}
