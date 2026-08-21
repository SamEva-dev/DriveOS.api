using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class RequestTrainingSessionReportRevisionCommandHandler(ITrainingSessionRepository repository, ITrainingSessionExecutionLock executionLock, ITrainingDeliveryUnitOfWork unitOfWork, IClock clock) : ICommandHandler<RequestTrainingSessionReportRevisionCommand, TrainingSessionReportRevisionResponse>
{
    public async Task<Result<TrainingSessionReportRevisionResponse>> Handle(RequestTrainingSessionReportRevisionCommand c, CancellationToken ct)
    {
        await unitOfWork.BeginTransactionAsync(ct); try { await executionLock.AcquireAsync(c.OrganizationId, c.SessionId, ct); var s = await repository.GetByIdForUpdateAsync(c.OrganizationId, c.SessionId, ct); if (s is null) return Result.Failure<TrainingSessionReportRevisionResponse>(TrainingSessionErrors.NotFound); var r=s.RequestReportRevision(c.OperationId,c.ExpectedVersion,(SessionReportRevisionScenario)c.Scenario,c.FieldCode,c.CurrentValue,c.ProposedValue,c.Reason,c.HasFinancialImpact,c.ApprovalRequired,c.ActorUserId,clock.UtcNow); if(r.IsFailure){await unitOfWork.RollbackTransactionAsync(ct);return Result.Failure<TrainingSessionReportRevisionResponse>(r.Error);} await unitOfWork.CommitTransactionAsync(ct); return Result.Success(Map(r.Value)); } catch { if(unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(ct); throw; }
    }
    internal static TrainingSessionReportRevisionResponse Map(SessionReportRevision x)=>new(x.Id.Value,(int)x.Scenario,(int)x.Status,x.FieldCode,x.CurrentValue,x.ProposedValue,x.Reason,x.HasFinancialImpact,x.RequestedByUserId.Value,x.RequestedAtUtc,x.DecidedByUserId?.Value,x.DecidedAtUtc,x.DecisionReason,x.AppliedReportVersion);
}
public sealed class DecideTrainingSessionReportRevisionCommandHandler(ITrainingSessionRepository repository, ITrainingSessionExecutionLock executionLock, ITrainingDeliveryUnitOfWork unitOfWork, IClock clock) : ICommandHandler<DecideTrainingSessionReportRevisionCommand, TrainingSessionReportRevisionResponse>
{
    public async Task<Result<TrainingSessionReportRevisionResponse>> Handle(DecideTrainingSessionReportRevisionCommand c, CancellationToken ct){await unitOfWork.BeginTransactionAsync(ct);try{await executionLock.AcquireAsync(c.OrganizationId,c.SessionId,ct);var s=await repository.GetByIdForUpdateAsync(c.OrganizationId,c.SessionId,ct);if(s?.Report is null)return Result.Failure<TrainingSessionReportRevisionResponse>(TrainingSessionErrors.NotFound);var result=s.DecideReportRevision(c.RevisionId,c.Approve,c.DecisionReason,c.ActorUserId,clock.UtcNow);if(result.IsFailure){await unitOfWork.RollbackTransactionAsync(ct);return Result.Failure<TrainingSessionReportRevisionResponse>(result.Error);}var rev=s.Report.Revisions.First(x=>x.Id==c.RevisionId);await unitOfWork.CommitTransactionAsync(ct);return Result.Success(RequestTrainingSessionReportRevisionCommandHandler.Map(rev));}catch{if(unitOfWork.HasActiveTransaction)await unitOfWork.RollbackTransactionAsync(ct);throw;}}
}
public sealed class GetTrainingSessionReportRevisionsQueryHandler(ITrainingSessionRepository repository) : IQueryHandler<GetTrainingSessionReportRevisionsQuery, IReadOnlyCollection<TrainingSessionReportRevisionResponse>>
{
    public async Task<Result<IReadOnlyCollection<TrainingSessionReportRevisionResponse>>> Handle(GetTrainingSessionReportRevisionsQuery q,CancellationToken ct){var s=await repository.GetByIdAsync(q.OrganizationId,q.SessionId,ct);if(s?.Report is null)return Result.Success<IReadOnlyCollection<TrainingSessionReportRevisionResponse>>([]);return Result.Success<IReadOnlyCollection<TrainingSessionReportRevisionResponse>>(s.Report.Revisions.OrderByDescending(x=>x.RequestedAtUtc).Select(RequestTrainingSessionReportRevisionCommandHandler.Map).ToArray());}
}
