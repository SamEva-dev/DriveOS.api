using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record GetTrainingSessionInternalNoteQuery(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId) : IQuery<TrainingSessionInternalNoteResponse>;

internal sealed class GetTrainingSessionInternalNoteQueryHandler(ITrainingSessionRepository repository)
    : IQueryHandler<GetTrainingSessionInternalNoteQuery, TrainingSessionInternalNoteResponse>
{
    public async Task<Result<TrainingSessionInternalNoteResponse>> Handle(GetTrainingSessionInternalNoteQuery query, CancellationToken cancellationToken)
    {
        TrainingSession? session = await repository.GetByIdAsync(query.OrganizationId, query.SessionId, cancellationToken);
        if (session is null) return Result.Failure<TrainingSessionInternalNoteResponse>(TrainingSessionErrors.NotFound);
        if (session.Report is null) return Result.Failure<TrainingSessionInternalNoteResponse>(TrainingSessionErrors.ReportDraftRequiresCompletedSession);

        TrainingSessionInternalNoteResponse response = new(
            session.Id.Value,
            session.Report.Version,
            session.Report.InternalNote,
            session.Report.NarrativeRevisions
                .Where(x => x.Kind == SessionReportNarrativeKind.InternalNote)
                .OrderByDescending(x => x.ReportVersion)
                .Select(x => new TrainingSessionNarrativeRevisionResponse(
                    x.Id,
                    (int)x.Kind,
                    x.ReportVersion,
                    x.Content,
                    x.ChangedByUserId.Value,
                    x.ChangedAtUtc))
                .ToArray());

        return Result.Success(response);
    }
}
