using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Application.Incidents;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class GetTrainingSessionReportReviewQueryHandler(ITrainingSessionReadService readService, ITrainingIncidentReadService incidentReadService)
    : IQueryHandler<GetTrainingSessionReportReviewQuery, TrainingSessionReportReviewResponse>
{
    public async Task<Result<TrainingSessionReportReviewResponse>> Handle(GetTrainingSessionReportReviewQuery query, CancellationToken cancellationToken)
    {
        TrainingSessionResponse? session = await readService.GetAsync(query.OrganizationId, query.SessionId, cancellationToken);
        if (session is null) return Result.Failure<TrainingSessionReportReviewResponse>(TrainingSessionErrors.NotFound);

        TrainingSessionReportResponse? report = session.Report;
        IReadOnlyCollection<TrainingIncidentResponse> incidents = await incidentReadService.GetBySessionAsync(query.OrganizationId, query.SessionId, cancellationToken);
        var checks = new List<TrainingSessionReportReviewCheck>
        {
            Check("report", report is not null, true, "training.reportReview.checks.report"),
            Check("attendance", session.CurrentAttendance is not null, true, "training.reportReview.checks.attendance"),
            Check("duration", session.DeliveredDurationMinutes is > 0, true, "training.reportReview.checks.duration"),
            Check("competencies", session.CompetencyAssessments.Count > 0, true, "training.reportReview.checks.competencies"),
            Check("markers", session.Markers.All(m => !string.IsNullOrWhiteSpace(m.ShortNote)), true, "training.reportReview.checks.markers"),
            Check("incidents", incidents.All(i => !string.IsNullOrWhiteSpace(i.Description) && !string.IsNullOrWhiteSpace(i.ImmediateActions)), true, "training.reportReview.checks.incidents"),
            Check("comment", !string.IsNullOrWhiteSpace(report?.SharedComment), false, "training.reportReview.checks.comment"),
            Check("kilometers", !session.ActualVehicleId.HasValue || session.DistanceKilometers.HasValue, false, "training.reportReview.checks.kilometers"),
            Check("summary", !string.IsNullOrWhiteSpace(report?.Summary), true, "training.reportReview.checks.summary"),
            Check("nextObjective", !string.IsNullOrWhiteSpace(report?.NextObjective), true, "training.reportReview.checks.nextObjective")
        };

        bool canSubmit = report is not null && checks.Where(x => x.Blocking).All(x => x.Passed)
            && (report.Status == (int)SessionReportStatus.Draft || report.Status == (int)SessionReportStatus.ReadyToSubmit || report.Status == (int)SessionReportStatus.RejectedForCorrection);
        return Result.Success(new TrainingSessionReportReviewResponse(session.Id, report?.Status ?? (int)SessionReportStatus.Draft, report?.Version ?? 0, canSubmit, checks));
    }

    private static TrainingSessionReportReviewCheck Check(string code, bool passed, bool blocking, string key) => new(code, passed, blocking, key);
}

public sealed class MarkTrainingSessionReportReadyCommandHandler(ITrainingSessionRepository repository, ITrainingSessionExecutionLock executionLock, ITrainingDeliveryUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<MarkTrainingSessionReportReadyCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(MarkTrainingSessionReportReadyCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            TrainingSession? session = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.NotFound); }
            Result result = session.MarkReportReadyToSubmit(command.OperationId, command.ExpectedVersion, command.ActorUserId, clock.UtcNow);
            if (result.IsFailure) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(result.Error); }
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(TrainingSessionMappings.ToResponse(session));
        }
        catch { if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }
    }
}

public sealed class SubmitTrainingSessionReportCommandHandler(ITrainingSessionRepository repository, ITrainingSessionExecutionLock executionLock, ITrainingDeliveryUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<SubmitTrainingSessionReportCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(SubmitTrainingSessionReportCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            TrainingSession? session = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.NotFound); }
            Result result = session.SubmitReport(command.OperationId, command.ExpectedVersion, command.RequestSupervisorReview, command.ActorUserId, clock.UtcNow);
            if (result.IsFailure) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(result.Error); }
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(TrainingSessionMappings.ToResponse(session));
        }
        catch { if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }
    }
}
