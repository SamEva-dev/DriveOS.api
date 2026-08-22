using System.Security.Cryptography;
using System.Text;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Places;
using DriveOS.Modules.ExamsCertification.Domain.Readiness;
using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations;

public sealed class HoldExamPlaceCommandHandler(
    IExamPlaceRepository placeRepository,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<HoldExamPlaceCommand, ExamPlaceHoldResponse>
{
    public async Task<Result<ExamPlaceHoldResponse>> Handle(HoldExamPlaceCommand command, CancellationToken cancellationToken)
    {
        ExamPlace? place = await placeRepository.GetByIdForUpdateAsync(command.OrganizationId, command.ExamPlaceId, cancellationToken);
        if (place is null) return Result.Failure<ExamPlaceHoldResponse>(ExamRegistrationErrors.PlaceNotFound);

        int holdMinutes = Math.Clamp(command.HoldMinutes, 1, 15);
        Guid token = Guid.NewGuid();
        DateTimeOffset expiresAtUtc = clock.UtcNow.AddMinutes(holdMinutes);
        Result hold = place.Hold(token, expiresAtUtc, command.ActorUserId, clock.UtcNow);
        if (hold.IsFailure) return Result.Failure<ExamPlaceHoldResponse>(hold.Error);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(new ExamPlaceHoldResponse(place.Id.Value, token, expiresAtUtc));
    }
}

public sealed class ReleaseExamPlaceHoldCommandHandler(
    IExamPlaceRepository placeRepository,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ReleaseExamPlaceHoldCommand>
{
    public async Task<Result> Handle(ReleaseExamPlaceHoldCommand command, CancellationToken cancellationToken)
    {
        ExamPlace? place = await placeRepository.GetByIdForUpdateAsync(command.OrganizationId, command.ExamPlaceId, cancellationToken);
        if (place is null) return Result.Failure(ExamRegistrationErrors.PlaceNotFound);

        Result release = place.ReleaseHold(command.HoldToken, command.ActorUserId, clock.UtcNow);
        if (release.IsFailure) return release;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class CreateExamRegistrationCommandHandler(
    IExamRegistrationRepository registrationRepository,
    IExamReadinessDecisionRepository readinessRepository,
    IExamPlaceRepository placeRepository,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateExamRegistrationCommand, ExamRegistrationResponse>
{
    public async Task<Result<ExamRegistrationResponse>> Handle(CreateExamRegistrationCommand command, CancellationToken cancellationToken)
    {
        string fingerprint = Fingerprint(command);
        ExamRegistration? replay = await registrationRepository.FindByOperationIdAsync(command.OrganizationId, command.OperationId, cancellationToken);
        if (replay is not null)
        {
            return replay.MatchesOperation(command.OperationId, fingerprint)
                ? Result.Success(Map(replay))
                : Result.Failure<ExamRegistrationResponse>(ExamRegistrationErrors.OperationConflict);
        }

        ExamReadinessDecision? readiness = await readinessRepository.GetCurrentAsync(
            command.OrganizationId, command.StudentId, command.TrainingPathId, cancellationToken);
        if (readiness is null || readiness.Outcome is not (ExamReadinessOutcome.Ready or ExamReadinessOutcome.ReadyWithConditions))
            return Result.Failure<ExamRegistrationResponse>(ExamRegistrationErrors.ReadinessNotEligible);

        ExamPlace? place = await placeRepository.GetByIdForUpdateAsync(command.OrganizationId, command.ExamPlaceId, cancellationToken);
        if (place is null) return Result.Failure<ExamRegistrationResponse>(ExamRegistrationErrors.PlaceNotFound);
        if (place.HeldByUserId != command.ActorUserId || place.HoldToken != command.HoldToken)
            return Result.Failure<ExamRegistrationResponse>(ExamRegistrationErrors.PlaceNotHeldByRequester);

        ExamRegistration? active = await registrationRepository.FindActiveForStudentAsync(
            command.OrganizationId, command.StudentId, place.ExamType, place.LicenseCategory, cancellationToken);
        if (active is not null)
            return Result.Failure<ExamRegistrationResponse>(ExamRegistrationErrors.ActiveRegistrationAlreadyExists);

        ExamRegistrationId id = ExamRegistrationId.New();
        Result<ExamRegistration> creation = ExamRegistration.Create(
            id,
            command.OrganizationId,
            command.StudentId,
            command.TrainingPathId,
            readiness.Id,
            place.Id,
            place.ExamCenterId,
            place.ExamType,
            place.LicenseCategory,
            place.StartsAtUtc,
            place.EndsAtUtc,
            place.ProviderCode,
            place.ExternalPlaceId,
            command.OperationId,
            fingerprint,
            command.ActorUserId,
            clock.UtcNow);
        if (creation.IsFailure) return Result.Failure<ExamRegistrationResponse>(creation.Error);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            Result assign = place.Assign(command.HoldToken, command.StudentId, id, command.ActorUserId, clock.UtcNow);
            if (assign.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<ExamRegistrationResponse>(assign.Error);
            }

            registrationRepository.Add(creation.Value);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(Map(creation.Value));
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static string Fingerprint(CreateExamRegistrationCommand command)
    {
        string canonical = $"{command.OrganizationId.Value:N}|{command.StudentId.Value:N}|{command.TrainingPathId.Value:N}|{command.ExamPlaceId.Value:N}|{command.HoldToken:N}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static ExamRegistrationResponse Map(ExamRegistration x) => new(
        x.Id.Value, x.StudentId.Value, x.TrainingPathId.Value, x.ReadinessDecisionId.Value, x.ExamPlaceId.Value,
        x.ExamCenterId.Value, x.ExamType, x.LicenseCategory, x.ScheduledStartUtc, x.ScheduledEndUtc,
        x.ProviderCode, x.ExternalPlaceId, x.ExternalRegistrationId, x.CandidateReference, x.Status.ToString(), x.CreatedAtUtc);
}

public sealed class GetExamRegistrationQueryHandler(IExamRegistrationRepository repository)
    : IQueryHandler<GetExamRegistrationQuery, ExamRegistrationResponse>
{
    public async Task<Result<ExamRegistrationResponse>> Handle(GetExamRegistrationQuery query, CancellationToken cancellationToken)
    {
        ExamRegistration? x = await repository.GetByIdAsync(query.OrganizationId, query.RegistrationId, cancellationToken);
        return x is null
            ? Result.Failure<ExamRegistrationResponse>(ExamRegistrationErrors.NotFound)
            : Result.Success(Map(x));
    }

    private static ExamRegistrationResponse Map(ExamRegistration x) => new(
        x.Id.Value, x.StudentId.Value, x.TrainingPathId.Value, x.ReadinessDecisionId.Value, x.ExamPlaceId.Value,
        x.ExamCenterId.Value, x.ExamType, x.LicenseCategory, x.ScheduledStartUtc, x.ScheduledEndUtc,
        x.ProviderCode, x.ExternalPlaceId, x.ExternalRegistrationId, x.CandidateReference, x.Status.ToString(), x.CreatedAtUtc);
}

public sealed class GetStudentExamRegistrationsQueryHandler(IExamRegistrationRepository repository)
    : IQueryHandler<GetStudentExamRegistrationsQuery, IReadOnlyList<ExamRegistrationResponse>>
{
    public async Task<Result<IReadOnlyList<ExamRegistrationResponse>>> Handle(GetStudentExamRegistrationsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<ExamRegistration> items = await repository.ListForStudentAsync(query.OrganizationId, query.StudentId, cancellationToken);
        return Result.Success<IReadOnlyList<ExamRegistrationResponse>>(items.Select(x => new ExamRegistrationResponse(
            x.Id.Value, x.StudentId.Value, x.TrainingPathId.Value, x.ReadinessDecisionId.Value, x.ExamPlaceId.Value,
            x.ExamCenterId.Value, x.ExamType, x.LicenseCategory, x.ScheduledStartUtc, x.ScheduledEndUtc,
            x.ProviderCode, x.ExternalPlaceId, x.ExternalRegistrationId, x.CandidateReference, x.Status.ToString(), x.CreatedAtUtc)).ToArray());
    }
}
