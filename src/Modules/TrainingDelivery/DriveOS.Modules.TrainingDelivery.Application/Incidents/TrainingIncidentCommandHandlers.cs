using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.Modules.TrainingDelivery.Domain.Incidents;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Incidents;

public sealed class ReportTrainingIncidentCommandHandler(
    ITrainingSessionRepository sessionRepository,
    ITrainingIncidentRepository incidentRepository,
    ITrainingSessionExecutionLock sessionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ReportTrainingIncidentCommand, TrainingIncidentResponse>
{
    public async Task<Result<TrainingIncidentResponse>> Handle(ReportTrainingIncidentCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await sessionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            TrainingIncident? retry = await incidentRepository.GetByReportOperationAsync(command.OrganizationId, command.SessionId, command.OperationId, cancellationToken);
            if (retry is not null)
            {
                var extras = ToParticipants(command.AdditionalParticipants);
                if (!retry.MatchesReportRetry(command.IncidentType, command.Severity, command.OccurredAtUtc, command.Description, command.ImmediateActions, extras))
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<TrainingIncidentResponse>(TrainingIncidentErrors.OperationConflict);
                }
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Success(TrainingIncidentMappings.ToResponse(retry));
            }

            TrainingSession? session = await sessionRepository.GetByIdAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingIncidentResponse>(TrainingSessionErrors.NotFound);
            }
            if (session.Status is not (TrainingSessionStatus.Ready or TrainingSessionStatus.InProgress or TrainingSessionStatus.Interrupted or TrainingSessionStatus.Completed))
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingIncidentResponse>(TrainingIncidentErrors.InvalidSessionStatus);
            }

            UserId instructor = session.ActualInstructorId ?? session.ReadyInstructorId ?? session.InstructorId;
            Guid? vehicle = session.ActualVehicleId ?? session.ReadyVehicleId ?? session.VehicleId;
            BranchId? branch = session.ActualBranchId ?? session.ReadyBranchId ?? session.BranchId;
            Result<TrainingIncident> created = TrainingIncident.Report(
                TrainingIncidentId.New(), command.OrganizationId, session.Id, session.StudentId, instructor, vehicle, branch,
                session.PerformingOrganizationId, command.OperationId, command.IncidentType, command.Severity, command.OccurredAtUtc,
                command.Description, command.ImmediateActions, ToParticipants(command.AdditionalParticipants), command.ActorUserId, clock.UtcNow);
            if (created.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingIncidentResponse>(created.Error);
            }
            incidentRepository.Add(created.Value);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(TrainingIncidentMappings.ToResponse(created.Value));
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static IEnumerable<(TrainingIncidentParticipantType Type, Guid? ReferenceId, string? Label)> ToParticipants(IEnumerable<TrainingIncidentParticipantInput> items) =>
        items.Select(x => ((TrainingIncidentParticipantType)x.Type, x.ReferenceId, x.Label));
}

public abstract class TrainingIncidentMutationHandlerBase(
    ITrainingIncidentRepository repository,
    ITrainingIncidentExecutionLock incidentLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock)
{
    protected async Task<Result<TrainingIncidentResponse>> ExecuteAsync(OrganizationId organizationId, TrainingIncidentId incidentId, Func<TrainingIncident, DateTimeOffset, Result> mutation, CancellationToken ct)
    {
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await incidentLock.AcquireAsync(organizationId, incidentId, ct);
            TrainingIncident? incident = await repository.GetByIdForUpdateAsync(organizationId, incidentId, ct);
            if (incident is null) { await unitOfWork.RollbackTransactionAsync(ct); return Result.Failure<TrainingIncidentResponse>(TrainingIncidentErrors.NotFound); }
            Result result = mutation(incident, clock.UtcNow);
            if (result.IsFailure) { await unitOfWork.RollbackTransactionAsync(ct); return Result.Failure<TrainingIncidentResponse>(result.Error); }
            await unitOfWork.CommitTransactionAsync(ct);
            return Result.Success(TrainingIncidentMappings.ToResponse(incident));
        }
        catch { if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(ct); throw; }
    }
}

public sealed class AddTrainingIncidentEvidenceCommandHandler(ITrainingIncidentRepository r,ITrainingIncidentExecutionLock l,ITrainingDeliveryUnitOfWork u,IClock c) : TrainingIncidentMutationHandlerBase(r,l,u,c), ICommandHandler<AddTrainingIncidentEvidenceCommand,TrainingIncidentResponse>
{ public Task<Result<TrainingIncidentResponse>> Handle(AddTrainingIncidentEvidenceCommand x,CancellationToken ct)=>ExecuteAsync(x.OrganizationId,x.IncidentId,(i,n)=>i.AddEvidence(x.OperationId,x.DocumentId,x.EvidenceType,x.Description,x.ActorUserId,n),ct); }
public sealed class EscalateTrainingIncidentCommandHandler(ITrainingIncidentRepository r,ITrainingIncidentExecutionLock l,ITrainingDeliveryUnitOfWork u,IClock c) : TrainingIncidentMutationHandlerBase(r,l,u,c), ICommandHandler<EscalateTrainingIncidentCommand,TrainingIncidentResponse>
{ public Task<Result<TrainingIncidentResponse>> Handle(EscalateTrainingIncidentCommand x,CancellationToken ct)=>ExecuteAsync(x.OrganizationId,x.IncidentId,(i,n)=>i.Escalate(x.OperationId,x.Reason,x.ActorUserId,n),ct); }
public sealed class StartTrainingIncidentReviewCommandHandler(ITrainingIncidentRepository r,ITrainingIncidentExecutionLock l,ITrainingDeliveryUnitOfWork u,IClock c) : TrainingIncidentMutationHandlerBase(r,l,u,c), ICommandHandler<StartTrainingIncidentReviewCommand,TrainingIncidentResponse>
{ public Task<Result<TrainingIncidentResponse>> Handle(StartTrainingIncidentReviewCommand x,CancellationToken ct)=>ExecuteAsync(x.OrganizationId,x.IncidentId,(i,n)=>i.StartReview(x.OperationId,x.Reason,x.ActorUserId,n),ct); }
public sealed class ResolveTrainingIncidentCommandHandler(ITrainingIncidentRepository r,ITrainingIncidentExecutionLock l,ITrainingDeliveryUnitOfWork u,IClock c) : TrainingIncidentMutationHandlerBase(r,l,u,c), ICommandHandler<ResolveTrainingIncidentCommand,TrainingIncidentResponse>
{ public Task<Result<TrainingIncidentResponse>> Handle(ResolveTrainingIncidentCommand x,CancellationToken ct)=>ExecuteAsync(x.OrganizationId,x.IncidentId,(i,n)=>i.Resolve(x.OperationId,x.Resolution,x.ActorUserId,n),ct); }
public sealed class CloseTrainingIncidentCommandHandler(ITrainingIncidentRepository r,ITrainingIncidentExecutionLock l,ITrainingDeliveryUnitOfWork u,IClock c) : TrainingIncidentMutationHandlerBase(r,l,u,c), ICommandHandler<CloseTrainingIncidentCommand,TrainingIncidentResponse>
{ public Task<Result<TrainingIncidentResponse>> Handle(CloseTrainingIncidentCommand x,CancellationToken ct)=>ExecuteAsync(x.OrganizationId,x.IncidentId,(i,n)=>i.Close(x.OperationId,x.Note,x.ActorUserId,n),ct); }
