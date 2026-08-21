using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class RecordTrainingSessionInterventionCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RecordTrainingSessionInterventionCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(RecordTrainingSessionInterventionCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            TrainingSession? session = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null) return await RollbackNotFound();
            Result result = session.RecordIntervention(command.OperationId, command.Type, command.Severity, command.OccurredAtUtc, command.Context, command.Reason, command.RelatedCompetencyId, command.Outcome, command.InternalComment, command.SharedExplanation, command.ActorUserId, clock.UtcNow);
            return await CommitOrRollback(session, result, cancellationToken);
        }
        catch { if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }
    }
    private async Task<Result<TrainingSessionResponse>> RollbackNotFound() { await unitOfWork.RollbackTransactionAsync(); return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.NotFound); }
    private async Task<Result<TrainingSessionResponse>> CommitOrRollback(TrainingSession session, Result result, CancellationToken ct) { if (result.IsFailure) { await unitOfWork.RollbackTransactionAsync(ct); return Result.Failure<TrainingSessionResponse>(result.Error); } await unitOfWork.CommitTransactionAsync(ct); return Result.Success(TrainingSessionMappings.ToResponse(session)); }
}


public sealed class RecordTrainingSessionMarkerCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RecordTrainingSessionMarkerCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(RecordTrainingSessionMarkerCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            TrainingSession? session = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.NotFound);
            }

            Result result = session.RecordMarker(
                command.OperationId,
                command.Type,
                command.OccurredAtUtc,
                command.CompetencyId,
                command.ShortNote,
                command.Severity,
                command.Latitude,
                command.Longitude,
                command.CreatedOffline,
                command.ActorUserId,
                clock.UtcNow);

            if (result.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(result.Error);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(TrainingSessionMappings.ToResponse(session));
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            }

            throw;
        }
    }
}

public sealed class RecordTrainingSessionObservationCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RecordTrainingSessionObservationCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(RecordTrainingSessionObservationCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            TrainingSession? session = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.NotFound); }
            Result result = session.RecordObservation(command.OperationId, command.Type, command.ObservedAtUtc, command.Content, command.IsInternal, command.ActorUserId, clock.UtcNow);
            if (result.IsFailure) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(result.Error); }
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(TrainingSessionMappings.ToResponse(session));
        }
        catch { if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }
    }
}

public sealed class InterruptTrainingSessionCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<InterruptTrainingSessionCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(InterruptTrainingSessionCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            TrainingSession? session = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.NotFound); }
            Result result = session.Interrupt(command.OperationId, command.Reason, command.Description, command.InterruptedAtUtc, command.ActorUserId, clock.UtcNow);
            if (result.IsFailure) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(result.Error); }
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(TrainingSessionMappings.ToResponse(session));
        }
        catch { if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }
    }
}

public sealed class ResumeTrainingSessionCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ResumeTrainingSessionCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(ResumeTrainingSessionCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            TrainingSession? session = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.NotFound); }
            Result result = session.Resume(command.OperationId, command.ResumedAtUtc, command.ActorUserId, clock.UtcNow);
            if (result.IsFailure) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(result.Error); }
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(TrainingSessionMappings.ToResponse(session));
        }
        catch { if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }
    }
}

public sealed class RecordTrainingSessionOdometerCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RecordTrainingSessionOdometerCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(RecordTrainingSessionOdometerCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            TrainingSession? session = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.NotFound); }
            Result result = session.RecordOdometer(command.OperationId, command.OdometerKilometers, command.Source, command.ObservedAtUtc, command.ActorUserId, clock.UtcNow);
            if (result.IsFailure) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(result.Error); }
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(TrainingSessionMappings.ToResponse(session));
        }
        catch { if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }
    }
}


public sealed class RecordTrainingSessionEnergyCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RecordTrainingSessionEnergyCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(RecordTrainingSessionEnergyCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            TrainingSession? session = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.NotFound); }
            Result result = session.RecordEnergy(command.OperationId, command.Type, command.EnergyLevelPercent, command.Quantity, command.ObservedAtUtc, command.Note, command.CreatedOffline, command.ActorUserId, clock.UtcNow);
            if (result.IsFailure) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<TrainingSessionResponse>(result.Error); }
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(TrainingSessionMappings.ToResponse(session));
        }
        catch { if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }
    }
}
