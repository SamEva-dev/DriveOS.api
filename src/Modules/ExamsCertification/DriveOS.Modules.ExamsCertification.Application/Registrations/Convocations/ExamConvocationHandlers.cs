using System.Security.Cryptography;
using System.Text;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Places;
using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Convocations;

public sealed class ReceiveExamConvocationCommandHandler(
    IExamRegistrationRepository registrationRepository,
    IExamCenterRepository centerRepository,
    IExamConvocationRepository convocationRepository,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ReceiveExamConvocationCommand, ExamConvocationResponse>
{
    public async Task<Result<ExamConvocationResponse>> Handle(ReceiveExamConvocationCommand command, CancellationToken cancellationToken)
    {
        ExamRegistration? registration = await registrationRepository.GetByIdAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (registration is null) return Result.Failure<ExamConvocationResponse>(ExamRegistrationErrors.NotFound);
        if (registration.Status != ExamRegistrationStatus.Confirmed)
            return Result.Failure<ExamConvocationResponse>(ExamConvocationErrors.RegistrationNotConfirmed);

        ExamCenter? center = await centerRepository.GetByIdAsync(command.OrganizationId, command.ExamCenterId, cancellationToken);
        if (center is null) return Result.Failure<ExamConvocationResponse>(ExamConvocationErrors.CenterRequired);

        string fingerprint = Fingerprint(command);
        ExamConvocation? convocation = await convocationRepository.GetByRegistrationForUpdateAsync(
            command.OrganizationId, command.RegistrationId, cancellationToken);

        bool created = false;
        if (convocation is null)
        {
            Result<ExamConvocation> creation = ExamConvocation.Create(
                ExamConvocationId.New(), command.OrganizationId, command.RegistrationId,
                registration.StudentId, command.ActorUserId, clock.UtcNow);
            if (creation.IsFailure) return Result.Failure<ExamConvocationResponse>(creation.Error);
            convocation = creation.Value;
            convocationRepository.Add(convocation);
            created = true;
        }

        Result<ExamConvocationRevision> revision = convocation.ReceiveOfficialRevision(
            ExamConvocationRevisionId.New(), center.Id, center.Name, center.Address, center.TimeZoneId,
            command.ScheduledStartUtc, command.ScheduledEndUtc, command.ProviderCode,
            command.OfficialReference, command.CandidateReference, command.Instructions,
            command.RequiredDocuments, command.ProviderPayloadReference, command.OperationId,
            fingerprint, command.ActorUserId, clock.UtcNow);
        if (revision.IsFailure) return Result.Failure<ExamConvocationResponse>(revision.Error);

        if (created || revision.Value.Version == convocation.CurrentVersion)
            await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(Map(convocation));
    }

    private static string Fingerprint(ReceiveExamConvocationCommand command)
    {
        string canonical = string.Join('|',
            command.OrganizationId.Value.ToString("N"),
            command.RegistrationId.Value.ToString("N"),
            command.ExamCenterId.Value.ToString("N"),
            command.ScheduledStartUtc.ToUniversalTime().ToString("O"),
            command.ScheduledEndUtc.ToUniversalTime().ToString("O"),
            command.ProviderCode.Trim(),
            command.OfficialReference?.Trim() ?? string.Empty,
            command.CandidateReference?.Trim() ?? string.Empty,
            command.Instructions?.Trim() ?? string.Empty,
            command.RequiredDocuments?.Trim() ?? string.Empty,
            command.ProviderPayloadReference?.Trim() ?? string.Empty);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    internal static ExamConvocationResponse Map(ExamConvocation x) => new(
        x.Id.Value,
        x.RegistrationId.Value,
        x.StudentId.Value,
        x.CurrentVersion,
        x.DeliveryStatus.ToString(),
        x.DeliveryChannel?.ToString(),
        x.DeliveredAtUtc,
        x.AcknowledgedAtUtc,
        x.InternalMeetingAtUtc,
        x.InternalMeetingInstructions,
        x.Revisions.OrderByDescending(r => r.Version).Select(r => new ExamConvocationRevisionResponse(
            r.Id.Value, r.Version, r.ExamCenterId.Value, r.CenterName, r.CenterAddress, r.TimeZoneId,
            r.ScheduledStartUtc, r.ScheduledEndUtc, r.ProviderCode, r.OfficialReference, r.CandidateReference,
            r.Instructions, r.RequiredDocuments, r.ProviderPayloadReference, r.ReceivedAtUtc)).ToArray(),
        x.CreatedAtUtc,
        x.LastModifiedAtUtc);
}

public sealed class SetExamConvocationMeetingCommandHandler(
    IExamConvocationRepository repository,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SetExamConvocationMeetingCommand, ExamConvocationResponse>
{
    public async Task<Result<ExamConvocationResponse>> Handle(SetExamConvocationMeetingCommand command, CancellationToken cancellationToken)
    {
        ExamConvocation? convocation = await repository.GetByRegistrationForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (convocation is null) return Result.Failure<ExamConvocationResponse>(ExamConvocationErrors.NotFound);
        Result result = convocation.SetInternalMeeting(command.MeetingAtUtc, command.Instructions, command.ActorUserId, clock.UtcNow);
        if (result.IsFailure) return Result.Failure<ExamConvocationResponse>(result.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(ReceiveExamConvocationCommandHandler.Map(convocation));
    }
}

public sealed class MarkExamConvocationDeliveredCommandHandler(
    IExamConvocationRepository repository,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<MarkExamConvocationDeliveredCommand, ExamConvocationResponse>
{
    public async Task<Result<ExamConvocationResponse>> Handle(MarkExamConvocationDeliveredCommand command, CancellationToken cancellationToken)
    {
        ExamConvocation? convocation = await repository.GetByRegistrationForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (convocation is null) return Result.Failure<ExamConvocationResponse>(ExamConvocationErrors.NotFound);
        Result result = convocation.MarkDelivered(command.Channel, command.ActorUserId, clock.UtcNow);
        if (result.IsFailure) return Result.Failure<ExamConvocationResponse>(result.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(ReceiveExamConvocationCommandHandler.Map(convocation));
    }
}

public sealed class MarkExamConvocationAcknowledgedCommandHandler(
    IExamConvocationRepository repository,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<MarkExamConvocationAcknowledgedCommand, ExamConvocationResponse>
{
    public async Task<Result<ExamConvocationResponse>> Handle(MarkExamConvocationAcknowledgedCommand command, CancellationToken cancellationToken)
    {
        ExamConvocation? convocation = await repository.GetByRegistrationForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (convocation is null) return Result.Failure<ExamConvocationResponse>(ExamConvocationErrors.NotFound);
        Result result = convocation.MarkAcknowledged(command.ActorUserId, clock.UtcNow);
        if (result.IsFailure) return Result.Failure<ExamConvocationResponse>(result.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(ReceiveExamConvocationCommandHandler.Map(convocation));
    }
}

public sealed class GetExamConvocationQueryHandler(IExamConvocationRepository repository)
    : IQueryHandler<GetExamConvocationQuery, ExamConvocationResponse>
{
    public async Task<Result<ExamConvocationResponse>> Handle(GetExamConvocationQuery query, CancellationToken cancellationToken)
    {
        ExamConvocation? convocation = await repository.GetByRegistrationAsync(query.OrganizationId, query.RegistrationId, cancellationToken);
        return convocation is null
            ? Result.Failure<ExamConvocationResponse>(ExamConvocationErrors.NotFound)
            : Result.Success(ReceiveExamConvocationCommandHandler.Map(convocation));
    }
}
