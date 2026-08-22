using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Remediation;
using DriveOS.Modules.ExamsCertification.Domain.Results.Failure;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Remediation;

internal static class ExamRemediationMapper
{
    public static ExamRemediationRequestResponse Map(ExamRemediationRequest x) => new(x.Id.Value, x.FailureAnalysisId.Value,
        x.ExamResultId.Value, x.ResultRevision, x.FailedAttemptId.Value, x.RegistrationId.Value, x.StudentId.Value,
        x.FailedAttemptNumber, x.TrainingPathId?.Value, x.AnalysisSummary, x.RecommendationSummary, x.AffectedCompetencyIds,
        x.RecommendationCodes, x.RecommendedHours, x.ResponsibleUserId?.Value, x.ReviewDate, x.TargetDate, x.MockExamRequired,
        x.FundingReviewRequired, x.PedagogicalRemediationPlanId?.Value, x.Status.ToString(), x.DeferredReasonCode, x.FailureCode,
        x.ProvisionedAtUtc, x.CompletedAtUtc, x.ValidatedForRePresentationAtUtc, x.ValidatedByUserId?.Value, x.SupersededAtUtc);
}

public sealed class CreateExamRemediationRequestCommandHandler(IExamFailureAnalysisRepository analyses,
    IExamRemediationRequestRepository requests, IExamsCertificationUnitOfWork uow, IClock clock)
    : ICommandHandler<CreateExamRemediationRequestCommand, ExamRemediationRequestResponse>
{
    public async Task<Result<ExamRemediationRequestResponse>> Handle(CreateExamRemediationRequestCommand c, CancellationToken ct)
    {
        ExamFailureAnalysis? analysis = await analyses.GetByResultForUpdateAsync(c.OrganizationId, c.ResultId, c.ResultRevision, ct);
        if (analysis is null) return Result.Failure<ExamRemediationRequestResponse>(ExamFailureAnalysisErrors.NotFound);
        if (analysis.Status != ExamFailureAnalysisStatus.Approved) return Result.Failure<ExamRemediationRequestResponse>(ExamRemediationRequestErrors.AnalysisNotApproved);
        ExamRemediationRequest? existing = await requests.GetByAnalysisAsync(c.OrganizationId, analysis.Id, ct);
        if (existing is not null) return Result.Success(ExamRemediationMapper.Map(existing));

        Result<ExamRemediationRequest> created = ExamRemediationRequest.Create(c.OrganizationId, analysis.Id, analysis.ExamResultId,
            analysis.ResultRevision, analysis.AttemptId, analysis.RegistrationId, analysis.StudentId, analysis.AttemptNumber,
            analysis.TrainingPathId, analysis.Summary ?? analysis.InstructorAnalysis ?? "Approved failed-exam analysis",
            analysis.Recommendation, ParseGuids(analysis.AffectedCompetencyIdsSerialized), Split(analysis.RecommendationCodesSerialized),
            analysis.RecommendedHours, c.ActorUserId, clock.UtcNow);
        if (created.IsFailure) return Result.Failure<ExamRemediationRequestResponse>(created.Error);
        requests.Add(created.Value);
        await uow.CommitAsync(ct);
        return Result.Success(ExamRemediationMapper.Map(created.Value));
    }

    private static IReadOnlyCollection<Guid> ParseGuids(string value) => string.IsNullOrWhiteSpace(value) ? [] : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(Guid.Parse).ToArray();
    private static IReadOnlyCollection<string> Split(string value) => string.IsNullOrWhiteSpace(value) ? [] : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

public sealed class ConfigureExamRemediationRequestCommandHandler(IExamRemediationRequestRepository requests,
    IExamsCertificationUnitOfWork uow, IClock clock)
    : ICommandHandler<ConfigureExamRemediationRequestCommand, ExamRemediationRequestResponse>
{
    public async Task<Result<ExamRemediationRequestResponse>> Handle(ConfigureExamRemediationRequestCommand c, CancellationToken ct)
    {
        ExamRemediationRequest? x = await requests.GetByIdForUpdateAsync(c.OrganizationId, c.RequestId, ct);
        if (x is null) return Result.Failure<ExamRemediationRequestResponse>(ExamRemediationRequestErrors.NotFound);
        Result r = x.Configure(c.TrainingPathId, c.ResponsibleUserId, c.ReviewDate, c.TargetDate, c.MockExamRequired, c.FundingReviewRequired,
            c.RecommendedHours, c.ActorUserId, clock.UtcNow);
        if (r.IsFailure) return Result.Failure<ExamRemediationRequestResponse>(r.Error);
        await uow.CommitAsync(ct); return Result.Success(ExamRemediationMapper.Map(x));
    }
}

public sealed class ProvisionExamRemediationPlanCommandHandler(IExamRemediationRequestRepository requests, IExamRemediationGateway gateway,
    IExamsCertificationUnitOfWork uow, IClock clock)
    : ICommandHandler<ProvisionExamRemediationPlanCommand, ExamRemediationRequestResponse>
{
    public async Task<Result<ExamRemediationRequestResponse>> Handle(ProvisionExamRemediationPlanCommand c, CancellationToken ct)
    {
        ExamRemediationRequest? x = await requests.GetByIdForUpdateAsync(c.OrganizationId, c.RequestId, ct);
        if (x is null) return Result.Failure<ExamRemediationRequestResponse>(ExamRemediationRequestErrors.NotFound);
        if (x.PedagogicalRemediationPlanId.HasValue) return Result.Success(ExamRemediationMapper.Map(x));
        Result start = x.MarkProvisioning(c.ActorUserId, clock.UtcNow);
        if (start.IsFailure) return Result.Failure<ExamRemediationRequestResponse>(start.Error);
        await uow.CommitAsync(ct);

        string recommendation = x.RecommendationSummary ?? x.AnalysisSummary;
        var provision = await gateway.ProvisionAsync(new ExamRemediationProvisionRequest(x.OrganizationId, x.TrainingPathId!.Value,
            x.ResponsibleUserId!.Value, recommendation, x.RecommendedHours, null, x.ReviewDate!.Value,
            x.AffectedCompetencyIds, x.FailureAnalysisId, x.ExamResultId, x.ResultRevision), ct);

        x = await requests.GetByIdForUpdateAsync(c.OrganizationId, c.RequestId, ct);
        if (x is null) return Result.Failure<ExamRemediationRequestResponse>(ExamRemediationRequestErrors.NotFound);
        if (provision.Success && provision.PlanId.HasValue) x.MarkPlanned(provision.PlanId.Value, c.ActorUserId, clock.UtcNow);
        else if (provision.Deferred) x.MarkDeferred(provision.Code ?? "exams.remediation.provision-deferred", c.ActorUserId, clock.UtcNow);
        else x.MarkFailed(provision.Code ?? "exams.remediation.provision-failed", c.ActorUserId, clock.UtcNow);
        await uow.CommitAsync(ct);
        return Result.Success(ExamRemediationMapper.Map(x));
    }
}

public sealed class RefreshExamRemediationRequestCommandHandler(IExamRemediationRequestRepository requests, IExamRemediationGateway gateway,
    IExamsCertificationUnitOfWork uow, IClock clock)
    : ICommandHandler<RefreshExamRemediationRequestCommand, ExamRemediationRequestResponse>
{
    public async Task<Result<ExamRemediationRequestResponse>> Handle(RefreshExamRemediationRequestCommand c, CancellationToken ct)
    {
        ExamRemediationRequest? x = await requests.GetByIdForUpdateAsync(c.OrganizationId, c.RequestId, ct);
        if (x is null) return Result.Failure<ExamRemediationRequestResponse>(ExamRemediationRequestErrors.NotFound);
        if (!x.PedagogicalRemediationPlanId.HasValue) return Result.Success(ExamRemediationMapper.Map(x));
        ExamRemediationPedagogicalStatus? status = await gateway.GetStatusAsync(c.OrganizationId, x.PedagogicalRemediationPlanId.Value, ct);
        if (status is null) { x.MarkDeferred("exams.remediation.plan-not-resolved", c.ActorUserId, clock.UtcNow); }
        else x.SynchronizePedagogicalStatus(status.Status, c.ActorUserId, clock.UtcNow);
        await uow.CommitAsync(ct); return Result.Success(ExamRemediationMapper.Map(x));
    }
}

public sealed class ValidateExamRemediationForRePresentationCommandHandler(IExamRemediationRequestRepository requests,
    IExamsCertificationUnitOfWork uow, IClock clock)
    : ICommandHandler<ValidateExamRemediationForRePresentationCommand, ExamRemediationRequestResponse>
{
    public async Task<Result<ExamRemediationRequestResponse>> Handle(ValidateExamRemediationForRePresentationCommand c, CancellationToken ct)
    {
        ExamRemediationRequest? x = await requests.GetByIdForUpdateAsync(c.OrganizationId, c.RequestId, ct);
        if (x is null) return Result.Failure<ExamRemediationRequestResponse>(ExamRemediationRequestErrors.NotFound);
        Result r = x.ValidateForRePresentation(c.ActorUserId, clock.UtcNow);
        if (r.IsFailure) return Result.Failure<ExamRemediationRequestResponse>(r.Error);
        await uow.CommitAsync(ct); return Result.Success(ExamRemediationMapper.Map(x));
    }
}

public sealed class CancelExamRemediationRequestCommandHandler(IExamRemediationRequestRepository requests,
    IExamsCertificationUnitOfWork uow, IClock clock)
    : ICommandHandler<CancelExamRemediationRequestCommand, ExamRemediationRequestResponse>
{
    public async Task<Result<ExamRemediationRequestResponse>> Handle(CancelExamRemediationRequestCommand c, CancellationToken ct)
    {
        ExamRemediationRequest? x = await requests.GetByIdForUpdateAsync(c.OrganizationId, c.RequestId, ct);
        if (x is null) return Result.Failure<ExamRemediationRequestResponse>(ExamRemediationRequestErrors.NotFound);
        Result r = x.Cancel(c.Reason, c.ActorUserId, clock.UtcNow);
        if (r.IsFailure) return Result.Failure<ExamRemediationRequestResponse>(r.Error);
        await uow.CommitAsync(ct); return Result.Success(ExamRemediationMapper.Map(x));
    }
}

public sealed class GetExamRemediationRequestQueryHandler(IExamRemediationRequestRepository requests)
    : IQueryHandler<GetExamRemediationRequestQuery, ExamRemediationRequestResponse>
{
    public async Task<Result<ExamRemediationRequestResponse>> Handle(GetExamRemediationRequestQuery q, CancellationToken ct)
    { var x=await requests.GetByIdAsync(q.OrganizationId,q.RequestId,ct); return x is null?Result.Failure<ExamRemediationRequestResponse>(ExamRemediationRequestErrors.NotFound):Result.Success(ExamRemediationMapper.Map(x)); }
}
public sealed class GetExamRemediationByResultQueryHandler(IExamRemediationRequestRepository requests)
    : IQueryHandler<GetExamRemediationByResultQuery, ExamRemediationRequestResponse>
{
    public async Task<Result<ExamRemediationRequestResponse>> Handle(GetExamRemediationByResultQuery q, CancellationToken ct)
    { var x=await requests.GetByResultRevisionForUpdateAsync(q.OrganizationId,q.ResultId,q.ResultRevision,ct); return x is null?Result.Failure<ExamRemediationRequestResponse>(ExamRemediationRequestErrors.NotFound):Result.Success(ExamRemediationMapper.Map(x)); }
}
public sealed class GetStudentExamRemediationsQueryHandler(IExamRemediationRequestRepository requests)
    : IQueryHandler<GetStudentExamRemediationsQuery, IReadOnlyList<ExamRemediationRequestResponse>>
{
    public async Task<Result<IReadOnlyList<ExamRemediationRequestResponse>>> Handle(GetStudentExamRemediationsQuery q, CancellationToken ct)
    { var xs=await requests.ListByStudentAsync(q.OrganizationId,q.StudentId,ct); return Result.Success<IReadOnlyList<ExamRemediationRequestResponse>>(xs.Select(ExamRemediationMapper.Map).ToArray()); }
}
