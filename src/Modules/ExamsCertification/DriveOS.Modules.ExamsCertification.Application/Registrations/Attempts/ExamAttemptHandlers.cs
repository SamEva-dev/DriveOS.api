using System.Security.Cryptography;
using System.Text;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation;
using DriveOS.SharedKernel.Results;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.Application.Abstractions.Time;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Attempts;

public sealed class CreateExamAttemptCommandHandler(
    IExamRegistrationRepository registrations,
    IExamPreparationRepository preparations,
    IExamOperationalPlanRepository operationalPlans,
    IExamResourceAssignmentRepository assignments,
    IExamAttemptRepository attempts,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateExamAttemptCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(CreateExamAttemptCommand command, CancellationToken cancellationToken)
    {
        ExamRegistration? registration = await registrations.GetByIdAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (registration is null)
            return Result.Failure<ExamAttemptResponse>(ExamAttemptErrors.NotFound);
        if (registration.Status != ExamRegistrationStatus.Confirmed)
            return Result.Failure<ExamAttemptResponse>(ExamAttemptErrors.RegistrationNotConfirmed);

        ExamPreparation? preparation = await preparations.GetByRegistrationAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (preparation is null || !preparation.IsConfirmed || preparation.ConfirmedRevision != preparation.Revision)
            return Result.Failure<ExamAttemptResponse>(ExamAttemptErrors.PreparationNotConfirmed);

        string fingerprint = Fingerprint("create", command.RegistrationId.Value, preparation.Id.Value, preparation.Revision, command.OperationId);
        ExamAttempt? existing = await attempts.GetByRegistrationForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (existing is not null)
        {
            return existing.MatchesOperation(command.OperationId, fingerprint)
                ? Result.Success(Map(existing))
                : Result.Failure<ExamAttemptResponse>(ExamAttemptErrors.AlreadyExists);
        }

        ExamOperationalPlan? plan = await operationalPlans.GetByRegistrationAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (plan is null)
            return Result.Failure<ExamAttemptResponse>(ExamAttemptErrors.OperationalPlanMissing);

        if (plan.Status != ExamOperationalPlanStatus.ReadyForAssignment || plan.HasSchedulingConflicts)
            return Result.Failure<ExamAttemptResponse>(ExamAttemptErrors.OperationalPlanNotReady);

        ExamResourceAssignment? assignment = await assignments.GetByRegistrationAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (assignment is null)
            return Result.Failure<ExamAttemptResponse>(ExamAttemptErrors.ResourceAssignmentMissing);
        if (plan.ConvocationVersion != preparation.ConvocationVersion || assignment.ConvocationVersion != preparation.ConvocationVersion)
            return Result.Failure<ExamAttemptResponse>(ExamAttemptErrors.ConvocationVersionMismatch);
        if (assignment.Status != ExamResourceAssignmentStatus.Assigned || assignment.SchedulingBookingId is null)
            return Result.Failure<ExamAttemptResponse>(ExamAttemptErrors.BookingNotConfirmed);

        int attemptNumber = await attempts.GetNextAttemptNumberAsync(
            command.OrganizationId, registration.StudentId, registration.ExamType, registration.LicenseCategory, cancellationToken);

        Result<ExamAttempt> created = ExamAttempt.Create(
            command.OrganizationId,
            command.RegistrationId,
            preparation.Id,
            registration.StudentId,
            attemptNumber,
            preparation.Revision,
            preparation.ConvocationVersion,
            registration.ExamType,
            registration.LicenseCategory,
            registration.ExamCenterId,
            registration.ExamPlaceId,
            plan.OfficialStartUtc,
            plan.OfficialEndUtc,
            plan.MeetingAtUtc,
            assignment.InstructorId,
            assignment.VehicleId,
            assignment.SchedulingBookingId.Value,
            command.OperationId,
            fingerprint,
            command.ActorUserId,
            clock.UtcNow);

        if (created.IsFailure)
            return Result.Failure<ExamAttemptResponse>(created.Error);

        attempts.Add(created.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(Map(created.Value));
    }

    internal static ExamAttemptResponse Map(ExamAttempt x) => new(
        x.Id.Value, x.RegistrationId.Value, x.PreparationId.Value, x.StudentId.Value,
        x.AttemptNumber, x.PreparationRevision, x.ConvocationVersion, x.ExamType, x.LicenseCategory,
        x.ExamCenterId.Value, x.ExamPlaceId.Value, x.ScheduledStartUtc, x.ScheduledEndUtc, x.MeetingAtUtc,
        x.InstructorId?.Value, x.VehicleId?.Value, x.SchedulingBookingId.Value, x.Status.ToString(), x.AttendanceStatus.ToString(),
        x.CheckedInAtUtc, x.DepartedAtUtc, x.ArrivedAtCenterAtUtc, x.StartedAtUtc, x.CompletedAtUtc, x.ReturnedAtUtc,
        x.OperationalReasonCode, x.OperationalNotes, x.Timeline.OrderBy(t => t.OccurredAtUtc).Select(t => new ExamAttemptTimelineResponse(
            t.Id.Value, t.OperationId, t.Type.ToString(), t.Status.ToString(), t.Note, t.OccurredAtUtc, t.ActorUserId.Value,
            t.Latitude, t.Longitude, t.AccuracyMeters, t.LocationPurpose?.ToString(), t.InstructorId?.Value, t.VehicleId?.Value)).ToArray(),
        x.CreatedAtUtc, x.LastModifiedAtUtc);

    internal static string Fingerprint(string action, Guid registrationId, params object?[] values)
    {
        string canonical = string.Join('|', new[] { action, registrationId.ToString("N") }.Concat(values.Select(x => x?.ToString() ?? "")));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}

public sealed class GetExamAttemptQueryHandler(IExamAttemptRepository attempts) : IQueryHandler<GetExamAttemptQuery, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(GetExamAttemptQuery query, CancellationToken cancellationToken)
    {
        ExamAttempt? attempt = await attempts.GetByRegistrationAsync(query.OrganizationId, query.RegistrationId, cancellationToken);
        return attempt is null
            ? Result.Failure<ExamAttemptResponse>(ExamAttemptErrors.NotFound)
            : Result.Success(CreateExamAttemptCommandHandler.Map(attempt));
    }
}

public abstract class ExamAttemptMutationHandlerBase(
    IExamAttemptRepository attempts,
    IExamPreparationRepository preparations,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock)
{
    protected readonly IExamAttemptRepository Attempts = attempts;
    protected readonly IExamPreparationRepository Preparations = preparations;
    protected readonly IExamsCertificationUnitOfWork UnitOfWork = unitOfWork;
    protected readonly IClock Clock = clock;

    protected async Task<Result<ExamAttempt>> LoadAsync(OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken)
    {
        ExamAttempt? attempt = await Attempts.GetByRegistrationForUpdateAsync(organizationId, registrationId, cancellationToken);
        return attempt is null ? Result.Failure<ExamAttempt>(ExamAttemptErrors.NotFound) : Result.Success(attempt);
    }

    protected async Task<Result> EnsurePreparationStillCurrentAsync(ExamAttempt attempt, CancellationToken cancellationToken)
    {
        ExamPreparation? preparation = await Preparations.GetByRegistrationAsync(attempt.OrganizationId, attempt.RegistrationId, cancellationToken);
        if (preparation is null || !preparation.IsConfirmed)
            return Result.Failure(ExamAttemptErrors.PreparationNotConfirmed);
        if (preparation.Id != attempt.PreparationId
            || preparation.Revision != attempt.PreparationRevision
            || preparation.ConvocationVersion != attempt.ConvocationVersion)
            return Result.Failure(ExamAttemptErrors.PreparationChanged);
        return Result.Success();
    }

    protected async Task<Result<ExamAttemptResponse>> SaveAsync(ExamAttempt attempt, Result mutation, CancellationToken cancellationToken)
    {
        if (mutation.IsFailure) return Result.Failure<ExamAttemptResponse>(mutation.Error);
        await UnitOfWork.CommitAsync(cancellationToken);
        return Result.Success(CreateExamAttemptCommandHandler.Map(attempt));
    }
}

public sealed class CheckInExamAttemptCommandHandler(
    IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<CheckInExamAttemptCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(CheckInExamAttemptCommand command, CancellationToken ct)
    {
        Result<ExamAttempt> loaded = await LoadAsync(command.OrganizationId, command.RegistrationId, ct);
        if (loaded.IsFailure) return Result.Failure<ExamAttemptResponse>(loaded.Error);
        Result guard = await EnsurePreparationStillCurrentAsync(loaded.Value, ct);
        if (guard.IsFailure) return Result.Failure<ExamAttemptResponse>(guard.Error);
        string fp = CreateExamAttemptCommandHandler.Fingerprint("check-in", command.RegistrationId.Value, command.OperationId);
        return await SaveAsync(loaded.Value, loaded.Value.CheckIn(command.OperationId, fp, command.ActorUserId, command.OccurredAtUtc ?? Clock.UtcNow), ct);
    }
}

public sealed class StartExamAttemptCommandHandler(
    IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<StartExamAttemptCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(StartExamAttemptCommand command, CancellationToken ct)
    {
        Result<ExamAttempt> loaded = await LoadAsync(command.OrganizationId, command.RegistrationId, ct);
        if (loaded.IsFailure) return Result.Failure<ExamAttemptResponse>(loaded.Error);
        Result guard = await EnsurePreparationStillCurrentAsync(loaded.Value, ct);
        if (guard.IsFailure) return Result.Failure<ExamAttemptResponse>(guard.Error);
        string fp = CreateExamAttemptCommandHandler.Fingerprint("start", command.RegistrationId.Value, command.OperationId);
        return await SaveAsync(loaded.Value, loaded.Value.Start(command.OperationId, fp, command.ActorUserId, command.OccurredAtUtc ?? Clock.UtcNow), ct);
    }
}

public sealed class CompleteExamAttemptCommandHandler(
    IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<CompleteExamAttemptCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(CompleteExamAttemptCommand command, CancellationToken ct)
    {
        Result<ExamAttempt> loaded = await LoadAsync(command.OrganizationId, command.RegistrationId, ct);
        if (loaded.IsFailure) return Result.Failure<ExamAttemptResponse>(loaded.Error);
        string fp = CreateExamAttemptCommandHandler.Fingerprint("complete", command.RegistrationId.Value, command.OperationId);
        return await SaveAsync(loaded.Value, loaded.Value.Complete(command.OperationId, fp, command.ActorUserId, command.OccurredAtUtc ?? Clock.UtcNow), ct);
    }
}


public sealed class RecordExamDepartureCommandHandler(IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<RecordExamDepartureCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(RecordExamDepartureCommand c, CancellationToken ct) { var l=await LoadAsync(c.OrganizationId,c.RegistrationId,ct); if(l.IsFailure)return Result.Failure<ExamAttemptResponse>(l.Error); var g=await EnsurePreparationStillCurrentAsync(l.Value,ct); if(g.IsFailure)return Result.Failure<ExamAttemptResponse>(g.Error); var fp=CreateExamAttemptCommandHandler.Fingerprint("departure",c.RegistrationId.Value,c.OperationId); return await SaveAsync(l.Value,l.Value.RecordDeparture(c.OperationId,fp,c.ActorUserId,c.OccurredAtUtc??Clock.UtcNow),ct); }
}
public sealed class RecordExamArrivalCommandHandler(IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<RecordExamArrivalCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(RecordExamArrivalCommand c, CancellationToken ct) { var l=await LoadAsync(c.OrganizationId,c.RegistrationId,ct); if(l.IsFailure)return Result.Failure<ExamAttemptResponse>(l.Error); var g=await EnsurePreparationStillCurrentAsync(l.Value,ct); if(g.IsFailure)return Result.Failure<ExamAttemptResponse>(g.Error); var fp=CreateExamAttemptCommandHandler.Fingerprint("arrival",c.RegistrationId.Value,c.OperationId); return await SaveAsync(l.Value,l.Value.RecordArrival(c.OperationId,fp,c.ActorUserId,c.OccurredAtUtc??Clock.UtcNow),ct); }
}
public sealed class RecordExamReturnCommandHandler(IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<RecordExamReturnCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(RecordExamReturnCommand c, CancellationToken ct) { var l=await LoadAsync(c.OrganizationId,c.RegistrationId,ct); if(l.IsFailure)return Result.Failure<ExamAttemptResponse>(l.Error); var fp=CreateExamAttemptCommandHandler.Fingerprint("return",c.RegistrationId.Value,c.OperationId); return await SaveAsync(l.Value,l.Value.RecordReturn(c.OperationId,fp,c.ActorUserId,c.OccurredAtUtc??Clock.UtcNow),ct); }
}
public sealed class ReportExamAttemptIncidentCommandHandler(IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<ReportExamAttemptIncidentCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(ReportExamAttemptIncidentCommand c, CancellationToken ct) { var l=await LoadAsync(c.OrganizationId,c.RegistrationId,ct); if(l.IsFailure)return Result.Failure<ExamAttemptResponse>(l.Error); var fp=CreateExamAttemptCommandHandler.Fingerprint("incident",c.RegistrationId.Value,c.IncidentCode,c.Description,c.OperationId); return await SaveAsync(l.Value,l.Value.ReportIncident(c.IncidentCode,c.Description,c.OperationId,fp,c.ActorUserId,c.OccurredAtUtc??Clock.UtcNow),ct); }
}
public sealed class AddExamAttemptNoteCommandHandler(IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<AddExamAttemptNoteCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(AddExamAttemptNoteCommand c, CancellationToken ct) { var l=await LoadAsync(c.OrganizationId,c.RegistrationId,ct); if(l.IsFailure)return Result.Failure<ExamAttemptResponse>(l.Error); var fp=CreateExamAttemptCommandHandler.Fingerprint("note",c.RegistrationId.Value,c.Note,c.OperationId); return await SaveAsync(l.Value,l.Value.AddOperationalNote(c.Note,c.OperationId,fp,c.ActorUserId,c.OccurredAtUtc??Clock.UtcNow),ct); }
}
public sealed class RecordExamAttemptLocationCommandHandler(IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<RecordExamAttemptLocationCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(RecordExamAttemptLocationCommand c, CancellationToken ct) { if(!Enum.TryParse<ExamAttemptLocationPurpose>(c.Purpose,true,out var purpose)) return Result.Failure<ExamAttemptResponse>(ExamAttemptErrors.InvalidLocation); var l=await LoadAsync(c.OrganizationId,c.RegistrationId,ct); if(l.IsFailure)return Result.Failure<ExamAttemptResponse>(l.Error); var fp=CreateExamAttemptCommandHandler.Fingerprint("location",c.RegistrationId.Value,c.Latitude,c.Longitude,c.AccuracyMeters,c.Purpose,c.OperationId); return await SaveAsync(l.Value,l.Value.RecordLocation(c.Latitude,c.Longitude,c.AccuracyMeters,purpose,c.OperationId,fp,c.ActorUserId,c.OccurredAtUtc??Clock.UtcNow),ct); }
}
public sealed class RecordExamAttemptResourceChangeCommandHandler(IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamResourceAssignmentRepository assignments, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<RecordExamAttemptResourceChangeCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(RecordExamAttemptResourceChangeCommand c, CancellationToken ct) { var l=await LoadAsync(c.OrganizationId,c.RegistrationId,ct); if(l.IsFailure)return Result.Failure<ExamAttemptResponse>(l.Error); var g=await EnsurePreparationStillCurrentAsync(l.Value,ct); if(g.IsFailure)return Result.Failure<ExamAttemptResponse>(g.Error); var a=await assignments.GetByRegistrationAsync(c.OrganizationId,c.RegistrationId,ct); if(a is null || a.Status!=ExamResourceAssignmentStatus.Assigned || a.SchedulingBookingId is null) return Result.Failure<ExamAttemptResponse>(ExamAttemptErrors.ResourceAssignmentChanged); var fp=CreateExamAttemptCommandHandler.Fingerprint("resource-change",c.RegistrationId.Value,a.InstructorId?.Value,a.VehicleId?.Value,c.Reason,c.OperationId); return await SaveAsync(l.Value,l.Value.RecordValidatedResourceChange(a.InstructorId,a.VehicleId,c.Reason,c.OperationId,fp,c.ActorUserId,c.OccurredAtUtc??Clock.UtcNow),ct); }
}

public sealed class MarkExamAttemptAbsentCommandHandler(
    IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<MarkExamAttemptAbsentCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(MarkExamAttemptAbsentCommand command, CancellationToken ct)
    {
        Result<ExamAttempt> loaded = await LoadAsync(command.OrganizationId, command.RegistrationId, ct);
        if (loaded.IsFailure) return Result.Failure<ExamAttemptResponse>(loaded.Error);
        string fp = CreateExamAttemptCommandHandler.Fingerprint("absent", command.RegistrationId.Value, command.Excused, command.ReasonCode, command.Notes, command.OperationId);
        return await SaveAsync(loaded.Value, loaded.Value.MarkAbsent(command.Excused, command.ReasonCode, command.Notes, command.OperationId, fp, command.ActorUserId, Clock.UtcNow), ct);
    }
}

public sealed class PostponeExamAttemptCommandHandler(
    IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<PostponeExamAttemptCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(PostponeExamAttemptCommand command, CancellationToken ct)
    {
        Result<ExamAttempt> loaded = await LoadAsync(command.OrganizationId, command.RegistrationId, ct);
        if (loaded.IsFailure) return Result.Failure<ExamAttemptResponse>(loaded.Error);
        string fp = CreateExamAttemptCommandHandler.Fingerprint("postpone", command.RegistrationId.Value, command.ReasonCode, command.Notes, command.OperationId);
        return await SaveAsync(loaded.Value, loaded.Value.Postpone(command.ReasonCode, command.Notes, command.OperationId, fp, command.ActorUserId, Clock.UtcNow), ct);
    }
}

public sealed class CancelExamAttemptCommandHandler(
    IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<CancelExamAttemptCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(CancelExamAttemptCommand command, CancellationToken ct)
    {
        Result<ExamAttempt> loaded = await LoadAsync(command.OrganizationId, command.RegistrationId, ct);
        if (loaded.IsFailure) return Result.Failure<ExamAttemptResponse>(loaded.Error);
        string fp = CreateExamAttemptCommandHandler.Fingerprint("cancel", command.RegistrationId.Value, command.ReasonCode, command.Notes, command.OperationId);
        return await SaveAsync(loaded.Value, loaded.Value.Cancel(command.ReasonCode, command.Notes, command.OperationId, fp, command.ActorUserId, Clock.UtcNow), ct);
    }
}

public sealed class InterruptExamAttemptCommandHandler(
    IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<InterruptExamAttemptCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(InterruptExamAttemptCommand command, CancellationToken ct)
    {
        Result<ExamAttempt> loaded = await LoadAsync(command.OrganizationId, command.RegistrationId, ct);
        if (loaded.IsFailure) return Result.Failure<ExamAttemptResponse>(loaded.Error);
        string fp = CreateExamAttemptCommandHandler.Fingerprint("interrupt", command.RegistrationId.Value, command.ReasonCode, command.Notes, command.OperationId);
        return await SaveAsync(loaded.Value, loaded.Value.Interrupt(command.ReasonCode, command.Notes, command.OperationId, fp, command.ActorUserId, Clock.UtcNow), ct);
    }
}

public sealed class MarkExamAttemptUnableToStartCommandHandler(
    IExamAttemptRepository attempts, IExamPreparationRepository preparations, IExamsCertificationUnitOfWork uow, IClock clock)
    : ExamAttemptMutationHandlerBase(attempts, preparations, uow, clock), ICommandHandler<MarkExamAttemptUnableToStartCommand, ExamAttemptResponse>
{
    public async Task<Result<ExamAttemptResponse>> Handle(MarkExamAttemptUnableToStartCommand command, CancellationToken ct)
    {
        Result<ExamAttempt> loaded = await LoadAsync(command.OrganizationId, command.RegistrationId, ct);
        if (loaded.IsFailure) return Result.Failure<ExamAttemptResponse>(loaded.Error);
        string fp = CreateExamAttemptCommandHandler.Fingerprint("unable-to-start", command.RegistrationId.Value, command.ReasonCode, command.Notes, command.OperationId);
        return await SaveAsync(loaded.Value, loaded.Value.UnableToStart(command.ReasonCode, command.Notes, command.OperationId, fp, command.ActorUserId, Clock.UtcNow), ct);
    }
}
