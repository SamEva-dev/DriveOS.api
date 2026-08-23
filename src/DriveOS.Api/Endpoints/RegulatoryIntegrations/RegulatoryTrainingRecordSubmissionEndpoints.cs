using System.Text.Json;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using DriveOS.Modules.RegulatoryIntegrations.Application.Administration;
using DriveOS.Modules.RegulatoryIntegrations.Application.Submissions;
using DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.RegulatoryIntegrations;

public static class RegulatoryTrainingRecordSubmissionEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IEndpointRouteBuilder MapRegulatoryTrainingRecordSubmissionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/regulatory-integrations/training-record-submissions")
            .WithTags("Regulatory integrations - Training record submissions");

        group.MapGet("/", SearchAsync)
            .RequireAuthorization(DriveOsPermissionCodes.RegulatoryIntegrations.SubmissionsRead);
        group.MapGet("/summary", GetSummaryAsync)
            .RequireAuthorization(DriveOsPermissionCodes.RegulatoryIntegrations.SubmissionsRead);
        group.MapGet("/students/{studentId:guid}/overview", GetStudentOverviewAsync)
            .RequireAuthorization("Pedagogy.Summary.Read");
        group.MapGet("/{submissionId:guid}", GetAsync)
            .RequireAuthorization(DriveOsPermissionCodes.RegulatoryIntegrations.SubmissionsRead);
        group.MapPost("/{submissionId:guid}/reconcile", ReconcileAsync)
            .RequireAuthorization(DriveOsPermissionCodes.RegulatoryIntegrations.SubmissionsManage);
        group.MapPost("/{submissionId:guid}/retry", RetryAsync)
            .RequireAuthorization(DriveOsPermissionCodes.RegulatoryIntegrations.SubmissionsManage);

        return endpoints;
    }

    private static async Task<IResult> SearchAsync(
        string? status,
        string? countryCode,
        string? providerCode,
        Guid? studentId,
        Guid? trainingPathId,
        Guid? sessionId,
        DateTimeOffset? createdFromUtc,
        DateTimeOffset? createdToUtc,
        int page,
        int pageSize,
        IRegulatoryTrainingRecordAdministrationService administration,
        ICurrentTenant tenant,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Forbid();

        RegulatoryTrainingRecordSubmissionStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse(status, true, out RegulatoryTrainingRecordSubmissionStatus value))
            {
                return Error.Validation(
                    "RegulatoryIntegrations.Submission.StatusInvalid",
                    "errors.regulatoryIntegrations.submission.statusInvalid").ToHttpResult(http);
            }
            parsedStatus = value;
        }

        var filter = new RegulatoryTrainingRecordSubmissionFilter(
            parsedStatus,
            countryCode,
            providerCode,
            studentId,
            trainingPathId,
            sessionId,
            createdFromUtc,
            createdToUtc,
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 50 : pageSize);

        Result<RegulatoryTrainingRecordSubmissionPage> result =
            await administration.SearchAsync(organizationId, filter, cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToHttpResult(http);
    }

    private static async Task<IResult> GetSummaryAsync(
        string? countryCode,
        string? providerCode,
        IRegulatoryTrainingRecordAdministrationService administration,
        ICurrentTenant tenant,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Forbid();
        Result<RegulatoryTrainingRecordSynchronizationSummary> result =
            await administration.GetSummaryAsync(organizationId, countryCode, providerCode, cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToHttpResult(http);
    }

    private static async Task<IResult> GetStudentOverviewAsync(
        Guid studentId,
        Guid? trainingPathId,
        string? countryCode,
        string? providerCode,
        IRegulatoryTrainingRecordAdministrationService administration,
        ICurrentTenant tenant,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Forbid();

        Result<StudentRegulatoryTrainingRecordOverview> result = await administration.GetStudentOverviewAsync(
            organizationId,
            new PersonId(studentId),
            trainingPathId.HasValue ? new TrainingPathId(trainingPathId.Value) : null,
            string.IsNullOrWhiteSpace(countryCode) ? "FR" : countryCode,
            string.IsNullOrWhiteSpace(providerCode) ? "fr-livret-numerique" : providerCode,
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToHttpResult(http);
    }

    private static async Task<IResult> GetAsync(
        Guid submissionId,
        IRegulatoryTrainingRecordAdministrationService administration,
        ICurrentTenant tenant,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Forbid();
        Result<RegulatoryTrainingRecordSubmissionDetail> result =
            await administration.GetAsync(organizationId, submissionId, cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToHttpResult(http);
    }

    private static async Task<IResult> RetryAsync(
        Guid submissionId,
        IRegulatoryTrainingRecordAdministrationService administration,
        ICurrentTenant tenant,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Forbid();
        Result result = await administration.RetryAsync(
            organizationId,
            submissionId,
            DateTimeOffset.UtcNow,
            cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(http);
    }

    private static async Task<IResult> ReconcileAsync(
        Guid submissionId,
        IRegulatoryTrainingRecordAdministrationService administration,
        IRegulatoryTrainingSessionProjector projector,
        IRegulatoryTrainingRecordSubmissionService submissions,
        ICurrentTenant tenant,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Forbid();

        Result<string> payloadResult = await administration.GetProjectionPayloadAsync(
            organizationId,
            submissionId,
            cancellationToken);
        if (payloadResult.IsFailure) return payloadResult.Error.ToHttpResult(http);

        RegulatoryTrainingSessionProjection? previous;
        try
        {
            previous = JsonSerializer.Deserialize<RegulatoryTrainingSessionProjection>(payloadResult.Value, JsonOptions);
        }
        catch (JsonException)
        {
            return Error.Failure(
                "RegulatoryIntegrations.Submission.PayloadUnreadable",
                "errors.regulatoryIntegrations.submission.payloadUnreadable").ToHttpResult(http);
        }

        if (previous is null)
        {
            return Error.Failure(
                "RegulatoryIntegrations.Submission.PayloadUnreadable",
                "errors.regulatoryIntegrations.submission.payloadUnreadable").ToHttpResult(http);
        }

        var source = new RegulatoryTrainingSessionProjectionSource(
            previous.OrganizationId,
            previous.StudentOwnerOrganizationId,
            previous.PerformingOrganizationId,
            previous.SessionId,
            previous.StudentId,
            previous.TrainingPathId,
            previous.InstructorId,
            previous.BranchId,
            previous.VehicleId,
            previous.TrainingCategory,
            previous.ActualStartAtUtc,
            previous.ActualEndAtUtc,
            previous.DeliveredDurationMinutes,
            previous.CompletedAtUtc);

        Result<RegulatoryTrainingSessionProjection> projected = await projector.ProjectAsync(source, cancellationToken);
        if (projected.IsFailure) return projected.Error.ToHttpResult(http);

        _ = await submissions.ReconcileAsync(projected.Value, cancellationToken);

        Result<RegulatoryTrainingRecordSubmissionDetail> detail =
            await administration.GetAsync(organizationId, submissionId, cancellationToken);
        return detail.IsSuccess ? Results.Ok(detail.Value) : Results.NoContent();
    }
}
