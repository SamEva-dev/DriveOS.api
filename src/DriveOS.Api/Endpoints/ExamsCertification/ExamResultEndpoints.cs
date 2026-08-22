using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Results;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamResultEndpoints
{
    internal static IEndpointRouteBuilder MapExamResultEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder attempts = app.MapGroup("/api/exams/attempts/{attemptId:guid}/result").WithTags("Exams & Certification");
        attempts.MapGet("/", GetByAttempt).RequireAuthorization(DriveOsPermissionCodes.Exams.ResultsRead);
        attempts.MapPost("/", Record).RequireAuthorization(DriveOsPermissionCodes.Exams.ResultsRecord);
        attempts.MapPost("/import", Import).RequireAuthorization(DriveOsPermissionCodes.Exams.ResultsImport);

        RouteGroupBuilder results = app.MapGroup("/api/exams/results/{resultId:guid}").WithTags("Exams & Certification");
        results.MapGet("/", Get).RequireAuthorization(DriveOsPermissionCodes.Exams.ResultsRead);
        app.MapGet("/api/exams/students/{studentId:guid}/results", ListStudent).WithTags("Exams & Certification").RequireAuthorization(DriveOsPermissionCodes.Exams.ResultsRead);
        results.MapPost("/verify", Verify).RequireAuthorization(DriveOsPermissionCodes.Exams.ResultsVerify);
        results.MapPost("/finalize", async (Guid resultId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct) =>
            await FinalizeResult(resultId, mediator, tenant, user, ct))
            .RequireAuthorization(DriveOsPermissionCodes.Exams.ResultsFinalize);
        results.MapPost("/correct", Correct).RequireAuthorization(DriveOsPermissionCodes.Exams.ResultsCorrect);
        return app;
    }

    private static async Task<IResult> GetByAttempt(Guid attemptId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    { if (tenant.OrganizationId is not { } o) return Results.Unauthorized(); return ToResult(await mediator.Send(new GetExamResultByAttemptQuery(o, new ExamAttemptId(attemptId)), ct)); }
    private static async Task<IResult> Get(Guid resultId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    { if (tenant.OrganizationId is not { } o) return Results.Unauthorized(); return ToResult(await mediator.Send(new GetExamResultQuery(o, new ExamResultId(resultId)), ct)); }


    private static async Task<IResult> ListStudent(Guid studentId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<IReadOnlyList<ExamResultResponse>> result = await mediator.Send(new GetStudentExamResultsQuery(organizationId, new PersonId(studentId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> Record(Guid attemptId, ResultWriteRequest r, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct) =>
        await Write(attemptId, r, "Manual", mediator, tenant, user, ct);
    private static async Task<IResult> Import(Guid attemptId, ResultWriteRequest r, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct) =>
        await Write(attemptId, r, r.SourceKind, mediator, tenant, user, ct);
    private static async Task<IResult> Write(Guid attemptId, ResultWriteRequest r, string sourceKind, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var o, out var actor)) return Results.Unauthorized();
        if (!Enum.TryParse<ExamResultOutcomeInput>(r.Outcome, true, out var outcome)) return Failure(DriveOS.Modules.ExamsCertification.Domain.Results.ExamResultErrors.InvalidOutcome);
        DocumentId? documentId = r.EvidenceDocumentId.HasValue ? new DocumentId(r.EvidenceDocumentId.Value) : null;
        return ToResult(await mediator.Send(new RecordExamResultCommand(o, new ExamAttemptId(attemptId), outcome, r.Score, r.FailureReasonCode, r.Comments, sourceKind, r.ProviderCode, r.ExternalResultId, documentId, r.ReceivedAtUtc, r.OperationId, actor), ct));
    }
    private static async Task<IResult> Verify(Guid resultId, VerifyResultRequest r, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    { if (!TryContext(tenant, user, out var o, out var a)) return Results.Unauthorized(); return ToResult(await mediator.Send(new VerifyExamResultCommand(o, new ExamResultId(resultId), r.VerificationReference, a), ct)); }
    private static async Task<IResult> FinalizeResult(Guid resultId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    { if (!TryContext(tenant, user, out var o, out var a)) return Results.Unauthorized(); return ToResult(await mediator.Send(new FinalizeExamResultCommand(o, new ExamResultId(resultId), a), ct)); }
    private static async Task<IResult> Correct(Guid resultId, CorrectResultRequest r, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var o, out var a)) return Results.Unauthorized();
        if (!Enum.TryParse<ExamResultOutcomeInput>(r.Outcome, true, out var outcome)) return Failure(DriveOS.Modules.ExamsCertification.Domain.Results.ExamResultErrors.InvalidOutcome);
        DocumentId? documentId = r.EvidenceDocumentId.HasValue ? new DocumentId(r.EvidenceDocumentId.Value) : null;
        return ToResult(await mediator.Send(new CorrectExamResultCommand(o, new ExamResultId(resultId), outcome, r.Score, r.FailureReasonCode, r.Comments, r.SourceKind, r.ProviderCode, r.ExternalResultId, documentId, r.ReceivedAtUtc, r.CorrectionReason, r.OperationId, a), ct));
    }

    private static bool TryContext(ICurrentTenant tenant, ICurrentUser user, out OrganizationId o, out UserId a) { o = default; a = default; if (tenant.OrganizationId is not { } org || user.UserId is not { } uid) return false; o = org; a = uid; return true; }
    private static IResult ToResult(Result<ExamResultResponse> r) => r.IsSuccess ? Results.Ok(r.Value) : Failure(r.Error);
    private static IResult Failure(Error e) => Results.Problem(statusCode: e.Type switch { ErrorType.NotFound => 404, ErrorType.Conflict => 409, ErrorType.Validation => 400, _ => 400 }, title: e.Code, extensions: new Dictionary<string, object?> { { "code", e.Code }, { "messageKey", e.MessageKey } });
}

internal sealed record ResultWriteRequest(string Outcome, decimal? Score, string? FailureReasonCode, string? Comments, string SourceKind,
    string ProviderCode, string? ExternalResultId, Guid? EvidenceDocumentId, DateTimeOffset ReceivedAtUtc, Guid OperationId);
internal sealed record VerifyResultRequest(string VerificationReference);
internal sealed record CorrectResultRequest(string Outcome, decimal? Score, string? FailureReasonCode, string? Comments, string SourceKind,
    string ProviderCode, string? ExternalResultId, Guid? EvidenceDocumentId, DateTimeOffset ReceivedAtUtc, string CorrectionReason, Guid OperationId);
