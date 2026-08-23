using System.Text.Json;
using DriveOS.Modules.RegulatoryIntegrations.Application.Administration;
using DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;
using DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Administration;

internal sealed class RegulatoryTrainingRecordAdministrationService(RegulatoryIntegrationsDbContext db)
    : IRegulatoryTrainingRecordAdministrationService
{
    public async Task<Result<RegulatoryTrainingRecordSubmissionPage>> SearchAsync(
        OrganizationId organizationId,
        RegulatoryTrainingRecordSubmissionFilter filter,
        CancellationToken cancellationToken = default)
    {
        int page = Math.Max(filter.Page, 1);
        int pageSize = Math.Clamp(filter.PageSize, 1, 200);

        IQueryable<RegulatoryTrainingRecordSubmission> query = db.RegulatoryTrainingRecordSubmissions
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId);

        if (filter.Status is { } status) query = query.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(filter.CountryCode))
        {
            string country = filter.CountryCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.CountryCode == country);
        }
        if (!string.IsNullOrWhiteSpace(filter.ProviderCode))
        {
            string provider = filter.ProviderCode.Trim();
            query = query.Where(x => x.ProviderCode == provider);
        }
        if (filter.StudentId is { } studentId && studentId != Guid.Empty)
            query = query.Where(x => x.StudentId == new PersonId(studentId));
        if (filter.TrainingPathId is { } trainingPathId && trainingPathId != Guid.Empty)
            query = query.Where(x => x.TrainingPathId == new TrainingPathId(trainingPathId));
        if (filter.SessionId is { } sessionId && sessionId != Guid.Empty)
            query = query.Where(x => x.SessionId == new TrainingSessionId(sessionId));
        if (filter.CreatedFromUtc is { } from) query = query.Where(x => x.CreatedAtUtc >= from.ToUniversalTime());
        if (filter.CreatedToUtc is { } to) query = query.Where(x => x.CreatedAtUtc <= to.ToUniversalTime());

        int total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Revision)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new RegulatoryTrainingRecordSubmissionListItem(
                x.Id.Value,
                x.ProjectionId,
                x.StudentId.Value,
                x.TrainingPathId.Value,
                x.SessionId.Value,
                x.CountryCode,
                x.ProviderCode,
                x.Status,
                x.Revision,
                x.AttemptCount,
                x.CreatedAtUtc,
                x.LastAttemptAtUtc,
                x.NextAttemptAtUtc,
                x.AcknowledgedAtUtc,
                x.ExternalReference,
                x.LastErrorCode,
                x.IssuesJson != "[]"))
            .ToListAsync(cancellationToken);

        return Result.Success(new RegulatoryTrainingRecordSubmissionPage(page, pageSize, total, rows));
    }

    public async Task<Result<RegulatoryTrainingRecordSubmissionDetail>> GetAsync(
        OrganizationId organizationId,
        Guid submissionId,
        CancellationToken cancellationToken = default)
    {
        var current = await db.RegulatoryTrainingRecordSubmissions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == new RegulatoryTrainingRecordSubmissionId(submissionId), cancellationToken);

        if (current is null)
            return Result.Failure<RegulatoryTrainingRecordSubmissionDetail>(NotFound());

        var revisions = await db.RegulatoryTrainingRecordSubmissions
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId
                     && x.ProjectionId == current.ProjectionId
                     && x.ProviderCode == current.ProviderCode)
            .OrderByDescending(x => x.Revision)
            .Select(x => new RegulatoryTrainingRecordSubmissionRevision(
                x.Id.Value,
                x.Revision,
                x.Status,
                x.PayloadHash,
                x.SupersedesSubmissionId == null ? null : x.SupersedesSubmissionId.Value.Value,
                x.CreatedAtUtc,
                x.SubmittedAtUtc,
                x.AcknowledgedAtUtc,
                x.ExternalReference,
                x.LastErrorCode,
                x.LastErrorDetail))
            .ToListAsync(cancellationToken);

        return Result.Success(new RegulatoryTrainingRecordSubmissionDetail(
            current.Id.Value,
            current.ProjectionId,
            current.ProjectionSchemaVersion,
            current.StudentId.Value,
            current.TrainingPathId.Value,
            current.SessionId.Value,
            current.CountryCode,
            current.ProviderCode,
            current.Status,
            current.Revision,
            current.AttemptCount,
            current.CreatedAtUtc,
            current.LastAttemptAtUtc,
            current.NextAttemptAtUtc,
            current.SubmittedAtUtc,
            current.AcknowledgedAtUtc,
            current.ExternalReference,
            current.LastErrorCode,
            current.LastErrorDetail,
            current.IssuesJson,
            revisions));
    }

    public async Task<Result<RegulatoryTrainingRecordSynchronizationSummary>> GetSummaryAsync(
        OrganizationId organizationId,
        string? countryCode,
        string? providerCode,
        CancellationToken cancellationToken = default)
    {
        IQueryable<RegulatoryTrainingRecordSubmission> query = db.RegulatoryTrainingRecordSubmissions
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(countryCode))
        {
            string country = countryCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.CountryCode == country);
        }
        if (!string.IsNullOrWhiteSpace(providerCode))
        {
            string provider = providerCode.Trim();
            query = query.Where(x => x.ProviderCode == provider);
        }

        int total = await query.CountAsync(cancellationToken);
        var grouped = await query
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        DateTimeOffset? lastAccepted = await query
            .Where(x => x.Status == RegulatoryTrainingRecordSubmissionStatus.Accepted)
            .MaxAsync(x => (DateTimeOffset?)x.AcknowledgedAtUtc, cancellationToken);
        DateTimeOffset? lastFailure = await query
            .Where(x => x.Status == RegulatoryTrainingRecordSubmissionStatus.Rejected || x.Status == RegulatoryTrainingRecordSubmissionStatus.Failed)
            .MaxAsync(x => (DateTimeOffset?)x.AcknowledgedAtUtc ?? x.LastAttemptAtUtc, cancellationToken);

        int Count(RegulatoryTrainingRecordSubmissionStatus s) => grouped.GetValueOrDefault(s);

        return Result.Success(new RegulatoryTrainingRecordSynchronizationSummary(
            total,
            Count(RegulatoryTrainingRecordSubmissionStatus.WaitingForData),
            Count(RegulatoryTrainingRecordSubmissionStatus.Pending),
            Count(RegulatoryTrainingRecordSubmissionStatus.Processing),
            Count(RegulatoryTrainingRecordSubmissionStatus.Accepted),
            Count(RegulatoryTrainingRecordSubmissionStatus.Rejected),
            Count(RegulatoryTrainingRecordSubmissionStatus.RetryPending),
            Count(RegulatoryTrainingRecordSubmissionStatus.Failed),
            Count(RegulatoryTrainingRecordSubmissionStatus.Superseded),
            lastAccepted,
            lastFailure));
    }

    public async Task<Result<StudentRegulatoryTrainingRecordOverview>> GetStudentOverviewAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId? trainingPathId,
        string countryCode,
        string providerCode,
        CancellationToken cancellationToken = default)
    {
        string country = string.IsNullOrWhiteSpace(countryCode) ? "FR" : countryCode.Trim().ToUpperInvariant();
        string provider = string.IsNullOrWhiteSpace(providerCode) ? "fr-livret-numerique" : providerCode.Trim();

        IQueryable<RegulatoryTrainingRecordSubmission> query = db.RegulatoryTrainingRecordSubmissions
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId
                     && x.StudentId == studentId
                     && x.CountryCode == country
                     && x.ProviderCode == provider);

        if (trainingPathId is { } pathId && !pathId.IsEmpty)
            query = query.Where(x => x.TrainingPathId == pathId);

        var rows = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Revision)
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
            return Result.Success(new StudentRegulatoryTrainingRecordOverview(
                studentId.Value, trainingPathId?.Value, country, provider, null,
                0, 0, 0, 0, 0, 0, 0, null, null, null, null,
                Array.Empty<string>(), Array.Empty<StudentRegulatoryTrainingRecordRecentSubmission>()));

        RegulatoryTrainingRecordSubmission current = rows[0];
        var issues = ParseIssueCodes(current.IssuesJson);
        var recent = rows.Take(10).Select(x => new StudentRegulatoryTrainingRecordRecentSubmission(
            x.Id.Value, x.SessionId.Value, x.Status, x.Revision, x.AttemptCount,
            x.CreatedAtUtc, x.AcknowledgedAtUtc, x.ExternalReference, x.LastErrorCode, x.IssuesJson != "[]")).ToArray();

        return Result.Success(new StudentRegulatoryTrainingRecordOverview(
            studentId.Value,
            trainingPathId?.Value,
            country,
            provider,
            current.Status,
            rows.Count,
            rows.Count(x => x.Status == RegulatoryTrainingRecordSubmissionStatus.Accepted),
            rows.Count(x => x.Status == RegulatoryTrainingRecordSubmissionStatus.WaitingForData),
            rows.Count(x => x.Status is RegulatoryTrainingRecordSubmissionStatus.Pending or RegulatoryTrainingRecordSubmissionStatus.Processing),
            rows.Count(x => x.Status == RegulatoryTrainingRecordSubmissionStatus.Rejected),
            rows.Count(x => x.Status == RegulatoryTrainingRecordSubmissionStatus.RetryPending),
            rows.Count(x => x.Status == RegulatoryTrainingRecordSubmissionStatus.Failed),
            rows.Max(x => (DateTimeOffset?)x.LastAttemptAtUtc ?? x.CreatedAtUtc),
            rows.Where(x => x.Status == RegulatoryTrainingRecordSubmissionStatus.Accepted).Max(x => (DateTimeOffset?)x.AcknowledgedAtUtc),
            current.LastErrorCode,
            current.LastErrorDetail,
            issues,
            recent));
    }

    private static IReadOnlyList<string> ParseIssueCodes(string issuesJson)
    {
        if (string.IsNullOrWhiteSpace(issuesJson) || issuesJson == "[]") return Array.Empty<string>();
        try
        {
            using JsonDocument document = JsonDocument.Parse(issuesJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array) return Array.Empty<string>();
            return document.RootElement.EnumerateArray()
                .Select(x =>
                {
                    if (x.ValueKind == JsonValueKind.String) return x.GetString();
                    if (x.ValueKind == JsonValueKind.Object && x.TryGetProperty("code", out JsonElement code)) return code.GetString();
                    if (x.ValueKind == JsonValueKind.Object && x.TryGetProperty("Code", out code)) return code.GetString();
                    return null;
                })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (JsonException) { return Array.Empty<string>(); }
    }

    public async Task<Result<string>> GetProjectionPayloadAsync(
        OrganizationId organizationId,
        Guid submissionId,
        CancellationToken cancellationToken = default)
    {
        string? payload = await db.RegulatoryTrainingRecordSubmissions
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.Id == new RegulatoryTrainingRecordSubmissionId(submissionId))
            .Select(x => x.PayloadJson)
            .FirstOrDefaultAsync(cancellationToken);

        return payload is null
            ? Result.Failure<string>(NotFound())
            : Result.Success(payload);
    }

    public async Task<Result> RetryAsync(
        OrganizationId organizationId,
        Guid submissionId,
        DateTimeOffset requestedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var submission = await db.RegulatoryTrainingRecordSubmissions
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == new RegulatoryTrainingRecordSubmissionId(submissionId), cancellationToken);

        if (submission is null) return Result.Failure(NotFound());

        Result retry = submission.RequestManualRetry(requestedAtUtc);
        if (retry.IsFailure) return retry;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static Error NotFound() =>
        Error.NotFound("RegulatoryIntegrations.Submission.NotFound", "errors.regulatoryIntegrations.submission.notFound");
}
