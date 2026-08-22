using System.Security.Cryptography;
using System.Text;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Preparation;

public sealed class RefreshExamPreparationCommandHandler(
    IExamRegistrationRepository registrations,
    IExamPreparationRepository preparations,
    IExamPreparationSnapshotGateway gateway,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RefreshExamPreparationCommand, ExamPreparationResponse>
{
    public async Task<Result<ExamPreparationResponse>> Handle(
        RefreshExamPreparationCommand command,
        CancellationToken cancellationToken)
    {
        ExamRegistration? registration = await registrations.GetByIdAsync(
            command.OrganizationId, command.RegistrationId, cancellationToken);

        if (registration is null)
            return Result.Failure<ExamPreparationResponse>(ExamPreparationErrors.NotFound);

        if (registration.Status != ExamRegistrationStatus.Confirmed)
            return Result.Failure<ExamPreparationResponse>(ExamPreparationErrors.RegistrationNotConfirmed);

        string fingerprint = Fingerprint(command);
        ExamPreparation? preparation = await preparations.GetByRegistrationForUpdateAsync(
            command.OrganizationId, command.RegistrationId, cancellationToken);

        if (preparation?.LastOperationId == command.OperationId)
        {
            return string.Equals(preparation.LastRequestFingerprint, fingerprint, StringComparison.Ordinal)
                ? Result.Success(Map(preparation))
                : Result.Failure<ExamPreparationResponse>(ExamPreparationErrors.OperationConflict);
        }

        Result<ExamPreparationSourceSnapshot> source = await gateway.BuildAsync(
            command.OrganizationId, command.RegistrationId, cancellationToken);

        if (source.IsFailure)
            return Result.Failure<ExamPreparationResponse>(source.Error);

        if (preparation is null)
        {
            Result<ExamPreparation> created = ExamPreparation.Create(
                command.OrganizationId,
                command.RegistrationId,
                registration.StudentId,
                command.ActorUserId,
                clock.UtcNow);

            if (created.IsFailure)
                return Result.Failure<ExamPreparationResponse>(created.Error);

            preparation = created.Value;
            preparations.Add(preparation);
        }

        IReadOnlyCollection<int> reminders = command.ReminderOffsetsDays is { Count: > 0 }
            ? command.ReminderOffsetsDays
            : [7, 2, 1, 0];

        Result refreshed = preparation.Refresh(
            source.Value.ConvocationVersion,
            source.Value.Checks,
            source.Value.InstructorRequired,
            source.Value.VehicleRequired,
            command.MeetingPointConfirmed,
            command.VehicleEnergyConfirmed,
            command.InstructorConfirmed,
            command.InstructionsTransmitted,
            reminders,
            command.OperationId,
            fingerprint,
            command.ActorUserId,
            clock.UtcNow);

        if (refreshed.IsFailure)
            return Result.Failure<ExamPreparationResponse>(refreshed.Error);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(Map(preparation));
    }

    private static string Fingerprint(RefreshExamPreparationCommand command)
    {
        string canonical = string.Join(
            '|',
            command.RegistrationId.Value.ToString("N"),
            command.MeetingPointConfirmed,
            command.VehicleEnergyConfirmed,
            command.InstructorConfirmed,
            command.InstructionsTransmitted,
            string.Join(',', (command.ReminderOffsetsDays ?? [7, 2, 1, 0]).Distinct().OrderByDescending(x => x)));

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    internal static ExamPreparationResponse Map(ExamPreparation preparation) => new(
        preparation.Id.Value,
        preparation.RegistrationId.Value,
        preparation.StudentId.Value,
        preparation.Revision,
        preparation.ConvocationVersion,
        preparation.Status.ToString(),
        preparation.Status == ExamPreparationStatus.Ready,
        preparation.IsConfirmed,
        preparation.ConfirmedRevision,
        preparation.ConfirmedAtUtc,
        preparation.ConfirmedByUserId?.Value,
        preparation.MeetingPointConfirmed,
        preparation.VehicleEnergyConfirmed,
        preparation.InstructorConfirmed,
        preparation.InstructionsTransmitted,
        preparation.ReminderOffsetsDays,
        preparation.LastEvaluatedAtUtc,
        preparation.Checks
            .Select(c => new ExamPreparationCheckResponse(
                c.Code, c.Required, c.Status.ToString(), c.MessageKey, c.Source, c.Evidence))
            .ToArray());
}

public sealed class ConfirmExamPreparationCommandHandler(
    IExamPreparationRepository preparations,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ConfirmExamPreparationCommand, ExamPreparationResponse>
{
    public async Task<Result<ExamPreparationResponse>> Handle(
        ConfirmExamPreparationCommand command,
        CancellationToken cancellationToken)
    {
        ExamPreparation? preparation = await preparations.GetByRegistrationForUpdateAsync(
            command.OrganizationId, command.RegistrationId, cancellationToken);

        if (preparation is null)
            return Result.Failure<ExamPreparationResponse>(ExamPreparationErrors.NotFound);

        Result confirmed = preparation.Confirm(command.ActorUserId, clock.UtcNow);
        if (confirmed.IsFailure)
            return Result.Failure<ExamPreparationResponse>(confirmed.Error);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(RefreshExamPreparationCommandHandler.Map(preparation));
    }
}

public sealed class GetExamPreparationQueryHandler(IExamPreparationRepository preparations)
    : IQueryHandler<GetExamPreparationQuery, ExamPreparationResponse>
{
    public async Task<Result<ExamPreparationResponse>> Handle(
        GetExamPreparationQuery query,
        CancellationToken cancellationToken)
    {
        ExamPreparation? preparation = await preparations.GetByRegistrationAsync(
            query.OrganizationId, query.RegistrationId, cancellationToken);

        return preparation is null
            ? Result.Failure<ExamPreparationResponse>(ExamPreparationErrors.NotFound)
            : Result.Success(RefreshExamPreparationCommandHandler.Map(preparation));
    }
}
