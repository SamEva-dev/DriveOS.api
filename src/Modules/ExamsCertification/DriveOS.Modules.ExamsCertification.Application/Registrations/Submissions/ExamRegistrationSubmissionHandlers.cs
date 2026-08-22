using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Application.Providers.Connections;
using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.File;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Submissions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Submissions;

public sealed class SubmitExamRegistrationCommandHandler(
    IExamRegistrationRepository registrationRepository,
    IExamRegistrationFileRepository fileRepository,
    IExamRegistrationSubmissionRepository submissionRepository,
    IExamRegistrationSubmissionProviderResolver providerResolver,
    IExamProviderExecutionGuard providerExecutionGuard,
    IExamProviderErrorMapper errorMapper,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SubmitExamRegistrationCommand, ExamRegistrationSubmissionResponse>
{
    public async Task<Result<ExamRegistrationSubmissionResponse>> Handle(SubmitExamRegistrationCommand command, CancellationToken cancellationToken)
    {
        ExamRegistration? registration = await registrationRepository.GetByIdForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (registration is null)
            return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationErrors.NotFound);

        ExamRegistrationFile? file = await fileRepository.GetByRegistrationForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (file?.CurrentRevision is not { } revision || file.Status != ExamRegistrationFileStatus.Ready)
            return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationSubmissionErrors.FileNotReady);

        if (string.IsNullOrWhiteSpace(revision.CandidateReference))
            return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationSubmissionErrors.CandidateReferenceRequired);

        string payload = BuildPayload(registration, file, revision);
        string fingerprint = Fingerprint(command, revision.Id, revision.Version, payload);
        ExamRegistrationSubmission? replay = await submissionRepository.FindByOperationIdAsync(command.OrganizationId, command.OperationId, cancellationToken);
        if (replay is not null)
        {
            return replay.MatchesOperation(command.OperationId, fingerprint)
                ? Result.Success(Map(replay))
                : Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationSubmissionErrors.OperationConflict);
        }

        ExamRegistrationSubmission? existingRevision = await submissionRepository.FindByFileRevisionAsync(
            command.OrganizationId, command.RegistrationId, revision.Id, cancellationToken);
        if (existingRevision is not null)
            return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationSubmissionErrors.FileRevisionAlreadySubmitted);

        IExamRegistrationSubmissionProvider? provider = providerResolver.Resolve(registration.ProviderCode);
        if (provider is null)
            return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationSubmissionErrors.ProviderNotFound);
        if (!provider.Descriptor.Capabilities.HasFlag(ExamPlaceProviderCapability.SubmitRegistration))
            return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationSubmissionErrors.ProviderCapabilityMissing);

        int nextVersion = await submissionRepository.GetNextVersionAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        Result<ExamRegistrationSubmission> creation = ExamRegistrationSubmission.Create(
            command.OrganizationId, registration.Id, file.Id, revision.Id, revision.Version, nextVersion,
            registration.ProviderCode, payload, command.OperationId, fingerprint, command.ActorUserId, clock.UtcNow);
        if (creation.IsFailure)
            return Result.Failure<ExamRegistrationSubmissionResponse>(creation.Error);

        ExamRegistrationSubmission submission = creation.Value;
        submissionRepository.Add(submission);

        Result pending = registration.MarkPendingSubmission(command.ActorUserId, clock.UtcNow);
        if (pending.IsFailure) return Result.Failure<ExamRegistrationSubmissionResponse>(pending.Error);
        Result fileSubmitted = file.MarkSubmitted(revision.Version, command.ActorUserId, clock.UtcNow);
        if (fileSubmitted.IsFailure) return Result.Failure<ExamRegistrationSubmissionResponse>(fileSubmitted.Error);

        await unitOfWork.CommitAsync(cancellationToken);

        var providerRequest = new ExternalExamRegistrationSubmissionRequest(
            command.OrganizationId,
            registration.Id,
            registration.ExternalPlaceId,
            revision.CandidateReference,
            payload,
            command.OperationId.ToString("N"));

        try
        {
            ExternalExamRegistrationSubmissionResult external = await providerExecutionGuard.ExecuteAsync(
                command.OrganizationId,
                registration.ProviderCode,
                ct => provider.SubmitAsync(providerRequest, ct),
                cancellationToken);

            ApplyProviderResult(submission, registration, file, external, errorMapper, command.ActorUserId, clock.UtcNow);
            await unitOfWork.CommitAsync(cancellationToken);
            return Result.Success(Map(submission));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            string raw = JsonSerializer.Serialize(new { exceptionType = ex.GetType().Name });
            submission.MarkFailed(
                ExamRegistrationSubmissionErrors.ProviderUnavailable.Code,
                ExamRegistrationSubmissionErrors.ProviderUnavailable.MessageKey,
                raw,
                command.ActorUserId,
                clock.UtcNow);
            await unitOfWork.CommitAsync(cancellationToken);
            return Result.Success(Map(submission));
        }
    }

    private static void ApplyProviderResult(
        ExamRegistrationSubmission submission,
        ExamRegistration registration,
        ExamRegistrationFile file,
        ExternalExamRegistrationSubmissionResult external,
        IExamProviderErrorMapper errorMapper,
        UserId actor,
        DateTimeOffset now)
    {
        switch (external.Outcome)
        {
            case ExternalExamRegistrationSubmissionOutcome.AwaitingManualSubmission:
                submission.MarkAwaitingManualSubmission(actor, now);
                break;

            case ExternalExamRegistrationSubmissionOutcome.Submitted:
                submission.MarkSubmitted(external.ExternalSubmissionId, external.ProviderResponseCode, external.ProviderResponseJson, actor, now);
                registration.MarkSubmitted(external.ExternalRegistrationId, external.CandidateReference, actor, now);
                break;

            case ExternalExamRegistrationSubmissionOutcome.Accepted:
                submission.MarkAccepted(external.ExternalSubmissionId, external.ExternalRegistrationId, external.CandidateReference,
                    external.ProviderResponseCode, external.ProviderResponseJson, actor, now);
                registration.MarkConfirmed(external.ExternalRegistrationId, external.CandidateReference, actor, now);
                file.MarkOfficiallyAccepted(actor, now);
                break;

            case ExternalExamRegistrationSubmissionOutcome.Rejected:
            {
                ExamProviderMappedError mapped = errorMapper.Map(submission.ProviderCode, external.ProviderErrorCode, false);
                submission.MarkRejected(mapped.Code, mapped.MessageKey, external.ProviderResponseCode, external.ProviderResponseJson, actor, now);
                registration.MarkRejected(actor, now);
                file.MarkOfficiallyRejected(actor, now);
                break;
            }

            case ExternalExamRegistrationSubmissionOutcome.CorrectionRequested:
            {
                ExamProviderMappedError mapped = errorMapper.Map(submission.ProviderCode, external.ProviderErrorCode, true);
                submission.MarkCorrectionRequested(mapped.Code, mapped.MessageKey, external.ProviderResponseCode, external.ProviderResponseJson, actor, now);
                registration.MarkCorrectionRequested(actor, now);
                file.MarkCorrectionRequested(actor, now);
                break;
            }
        }
    }

    private static string BuildPayload(ExamRegistration registration, ExamRegistrationFile file, ExamRegistrationFileRevision revision) =>
        JsonSerializer.Serialize(new
        {
            schemaVersion = "driveos.exam-registration.v1",
            registrationId = registration.Id.Value,
            studentId = registration.StudentId.Value,
            trainingPathId = registration.TrainingPathId.Value,
            readinessDecisionId = registration.ReadinessDecisionId.Value,
            examPlaceId = registration.ExamPlaceId.Value,
            examCenterId = registration.ExamCenterId.Value,
            registration.ExamType,
            registration.LicenseCategory,
            registration.ScheduledStartUtc,
            registration.ScheduledEndUtc,
            registration.ProviderCode,
            registration.ExternalPlaceId,
            fileId = file.Id.Value,
            fileVersion = revision.Version,
            fileRevisionId = revision.Id,
            revision.CandidateReference,
            revision.OfficialDataJson,
            checklist = revision.Checklist.OrderBy(x => x.Code).Select(x => new
            {
                x.Code,
                x.Required,
                status = x.Status.ToString(),
                x.Source,
                x.Evidence
            })
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static string Fingerprint(SubmitExamRegistrationCommand command, Guid revisionId, int version, string payload)
    {
        string canonical = $"{command.OrganizationId.Value:N}|{command.RegistrationId.Value:N}|{revisionId:N}|{version}|{payload}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    internal static ExamRegistrationSubmissionResponse Map(ExamRegistrationSubmission x) => new(
        x.Id.Value, x.RegistrationId.Value, x.RegistrationFileId.Value, x.FileRevisionId, x.FileVersion, x.SubmissionVersion,
        x.ProviderCode, x.Status.ToString(), x.ExternalSubmissionId, x.ExternalRegistrationId, x.CandidateReference,
        x.ProviderResponseCode, x.ErrorCode, x.ErrorMessageKey, x.SubmittedAtUtc, x.RespondedAtUtc, x.CreatedAtUtc);
}

public sealed class RecordExamRegistrationOfficialResponseCommandHandler(
    IExamRegistrationRepository registrationRepository,
    IExamRegistrationFileRepository fileRepository,
    IExamRegistrationSubmissionRepository submissionRepository,
    IExamProviderErrorMapper errorMapper,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RecordExamRegistrationOfficialResponseCommand, ExamRegistrationSubmissionResponse>
{
    public async Task<Result<ExamRegistrationSubmissionResponse>> Handle(RecordExamRegistrationOfficialResponseCommand command, CancellationToken cancellationToken)
    {
        ExamRegistrationSubmission? submission = await submissionRepository.GetByIdForUpdateAsync(command.OrganizationId, command.SubmissionId, cancellationToken);
        if (submission is null || submission.RegistrationId != command.RegistrationId)
            return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationSubmissionErrors.NotFound);
        if (submission.IsFinal)
            return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationSubmissionErrors.AlreadyFinalized);

        ExamRegistration? registration = await registrationRepository.GetByIdForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        ExamRegistrationFile? file = await fileRepository.GetByRegistrationForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (registration is null || file is null)
            return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationErrors.NotFound);

        switch (command.Outcome)
        {
            case OfficialExamRegistrationOutcome.Submitted:
                submission.MarkSubmitted(command.ExternalSubmissionId, command.ProviderResponseCode, command.ProviderResponseJson, command.ActorUserId, clock.UtcNow);
                registration.MarkSubmitted(command.ExternalRegistrationId, command.CandidateReference, command.ActorUserId, clock.UtcNow);
                break;
            case OfficialExamRegistrationOutcome.Accepted:
                submission.MarkAccepted(command.ExternalSubmissionId, command.ExternalRegistrationId, command.CandidateReference,
                    command.ProviderResponseCode, command.ProviderResponseJson, command.ActorUserId, clock.UtcNow);
                registration.MarkConfirmed(command.ExternalRegistrationId, command.CandidateReference, command.ActorUserId, clock.UtcNow);
                file.MarkOfficiallyAccepted(command.ActorUserId, clock.UtcNow);
                break;
            case OfficialExamRegistrationOutcome.Rejected:
            {
                ExamProviderMappedError mapped = errorMapper.Map(submission.ProviderCode, command.ProviderErrorCode, false);
                submission.MarkRejected(mapped.Code, mapped.MessageKey, command.ProviderResponseCode, command.ProviderResponseJson, command.ActorUserId, clock.UtcNow);
                registration.MarkRejected(command.ActorUserId, clock.UtcNow);
                file.MarkOfficiallyRejected(command.ActorUserId, clock.UtcNow);
                break;
            }
            case OfficialExamRegistrationOutcome.CorrectionRequested:
            {
                ExamProviderMappedError mapped = errorMapper.Map(submission.ProviderCode, command.ProviderErrorCode, true);
                submission.MarkCorrectionRequested(mapped.Code, mapped.MessageKey, command.ProviderResponseCode, command.ProviderResponseJson, command.ActorUserId, clock.UtcNow);
                registration.MarkCorrectionRequested(command.ActorUserId, clock.UtcNow);
                file.MarkCorrectionRequested(command.ActorUserId, clock.UtcNow);
                break;
            }
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(SubmitExamRegistrationCommandHandler.Map(submission));
    }
}

public sealed class RetryExamRegistrationSubmissionCommandHandler(
    IExamRegistrationSubmissionRepository submissionRepository,
    IExamRegistrationRepository registrationRepository,
    IExamRegistrationFileRepository fileRepository,
    IExamRegistrationSubmissionProviderResolver providerResolver,
    IExamProviderExecutionGuard providerExecutionGuard,
    IExamProviderErrorMapper errorMapper,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RetryExamRegistrationSubmissionCommand, ExamRegistrationSubmissionResponse>
{
    public async Task<Result<ExamRegistrationSubmissionResponse>> Handle(RetryExamRegistrationSubmissionCommand command, CancellationToken cancellationToken)
    {
        ExamRegistrationSubmission? submission = await submissionRepository.GetByIdForUpdateAsync(command.OrganizationId, command.SubmissionId, cancellationToken);
        if (submission is null || submission.RegistrationId != command.RegistrationId)
            return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationSubmissionErrors.NotFound);
        if (submission.Status is not (ExamRegistrationSubmissionStatus.Failed or ExamRegistrationSubmissionStatus.Pending))
            return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationSubmissionErrors.AlreadyFinalized);

        ExamRegistration? registration = await registrationRepository.GetByIdForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        ExamRegistrationFile? file = await fileRepository.GetByRegistrationForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (registration is null || file is null) return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationErrors.NotFound);

        IExamRegistrationSubmissionProvider? provider = providerResolver.Resolve(submission.ProviderCode);
        if (provider is null) return Result.Failure<ExamRegistrationSubmissionResponse>(ExamRegistrationSubmissionErrors.ProviderNotFound);

        try
        {
            ExternalExamRegistrationSubmissionResult external = await providerExecutionGuard.ExecuteAsync(
                command.OrganizationId,
                submission.ProviderCode,
                ct => provider.SubmitAsync(new ExternalExamRegistrationSubmissionRequest(
                    command.OrganizationId, registration.Id, registration.ExternalPlaceId, registration.CandidateReference,
                    submission.PayloadJson, submission.OperationId.ToString("N")), ct),
                cancellationToken);

            if (external.Outcome == ExternalExamRegistrationSubmissionOutcome.Submitted)
            {
                submission.MarkSubmitted(external.ExternalSubmissionId, external.ProviderResponseCode, external.ProviderResponseJson, command.ActorUserId, clock.UtcNow);
                registration.MarkSubmitted(external.ExternalRegistrationId, external.CandidateReference, command.ActorUserId, clock.UtcNow);
            }
            else if (external.Outcome == ExternalExamRegistrationSubmissionOutcome.AwaitingManualSubmission)
            {
                submission.MarkAwaitingManualSubmission(command.ActorUserId, clock.UtcNow);
            }
            else if (external.Outcome == ExternalExamRegistrationSubmissionOutcome.Accepted)
            {
                submission.MarkAccepted(external.ExternalSubmissionId, external.ExternalRegistrationId, external.CandidateReference,
                    external.ProviderResponseCode, external.ProviderResponseJson, command.ActorUserId, clock.UtcNow);
                registration.MarkConfirmed(external.ExternalRegistrationId, external.CandidateReference, command.ActorUserId, clock.UtcNow);
                file.MarkOfficiallyAccepted(command.ActorUserId, clock.UtcNow);
            }
            else
            {
                ExamProviderMappedError mapped = errorMapper.Map(submission.ProviderCode, external.ProviderErrorCode,
                    external.Outcome == ExternalExamRegistrationSubmissionOutcome.CorrectionRequested);
                if (external.Outcome == ExternalExamRegistrationSubmissionOutcome.CorrectionRequested)
                {
                    submission.MarkCorrectionRequested(mapped.Code, mapped.MessageKey, external.ProviderResponseCode, external.ProviderResponseJson, command.ActorUserId, clock.UtcNow);
                    registration.MarkCorrectionRequested(command.ActorUserId, clock.UtcNow);
                    file.MarkCorrectionRequested(command.ActorUserId, clock.UtcNow);
                }
                else
                {
                    submission.MarkRejected(mapped.Code, mapped.MessageKey, external.ProviderResponseCode, external.ProviderResponseJson, command.ActorUserId, clock.UtcNow);
                    registration.MarkRejected(command.ActorUserId, clock.UtcNow);
                    file.MarkOfficiallyRejected(command.ActorUserId, clock.UtcNow);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            submission.MarkFailed(ExamRegistrationSubmissionErrors.ProviderUnavailable.Code,
                ExamRegistrationSubmissionErrors.ProviderUnavailable.MessageKey,
                JsonSerializer.Serialize(new { exceptionType = ex.GetType().Name }), command.ActorUserId, clock.UtcNow);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(SubmitExamRegistrationCommandHandler.Map(submission));
    }
}

public sealed class GetExamRegistrationSubmissionsQueryHandler(IExamRegistrationSubmissionRepository repository)
    : IQueryHandler<GetExamRegistrationSubmissionsQuery, IReadOnlyList<ExamRegistrationSubmissionResponse>>
{
    public async Task<Result<IReadOnlyList<ExamRegistrationSubmissionResponse>>> Handle(GetExamRegistrationSubmissionsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<ExamRegistrationSubmission> items = await repository.ListByRegistrationAsync(query.OrganizationId, query.RegistrationId, cancellationToken);
        return Result.Success<IReadOnlyList<ExamRegistrationSubmissionResponse>>(items.Select(SubmitExamRegistrationCommandHandler.Map).ToArray());
    }
}
