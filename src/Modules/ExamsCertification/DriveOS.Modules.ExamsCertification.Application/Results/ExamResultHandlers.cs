using System.Security.Cryptography;
using System.Text;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;
using DriveOS.Modules.ExamsCertification.Domain.Results;
using DriveOS.Modules.ExamsCertification.Domain.Results.Success;
using DriveOS.Modules.ExamsCertification.Domain.Results.Failure;
using DriveOS.Modules.ExamsCertification.Domain.Remediation;
using DriveOS.Modules.ExamsCertification.Domain.Certifications;
using DriveOS.Modules.ExamsCertification.Application.Success;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Results;

public sealed class RecordExamResultCommandHandler(IExamResultRepository resultRepository, IExamAttemptRepository attemptRepository,
    IExamsCertificationUnitOfWork unitOfWork, IClock clock) : ICommandHandler<RecordExamResultCommand, ExamResultResponse>
{
    public async Task<Result<ExamResultResponse>> Handle(RecordExamResultCommand command, CancellationToken cancellationToken)
    {
        string fingerprint = Fingerprint(command);
        ExamResult? replay = await resultRepository.FindByOperationIdAsync(command.OrganizationId, command.OperationId, cancellationToken);
        if (replay is not null)
            return replay.MatchesOperation(command.OperationId, fingerprint) ? Result.Success(Map(replay)) : Result.Failure<ExamResultResponse>(ExamResultErrors.OperationConflict);

        ExamAttempt? attempt = await attemptRepository.GetByIdAsync(command.OrganizationId, command.AttemptId, cancellationToken);
        if (attempt is null) return Result.Failure<ExamResultResponse>(ExamResultErrors.AttemptNotFound);
        if (attempt.Status != ExamAttemptStatus.AwaitingResult) return Result.Failure<ExamResultResponse>(ExamResultErrors.AttemptNotAwaitingResult);
        if (await resultRepository.GetByAttemptAsync(command.OrganizationId, command.AttemptId, cancellationToken) is not null)
            return Result.Failure<ExamResultResponse>(ExamResultErrors.AlreadyExists);

        if (!Enum.TryParse<ExamResultSourceKind>(command.SourceKind, true, out var sourceKind))
            return Result.Failure<ExamResultResponse>(ExamResultErrors.InvalidSource);

        Result<ExamResult> creation = ExamResult.Create(command.OrganizationId, attempt.Id, attempt.RegistrationId, attempt.StudentId,
            attempt.AttemptNumber, (ExamResultOutcome)(int)command.Outcome, command.Score, command.FailureReasonCode, command.Comments,
            sourceKind, command.ProviderCode, command.ExternalResultId, command.EvidenceDocumentId, command.ReceivedAtUtc,
            command.OperationId, fingerprint, command.ActorUserId, clock.UtcNow);
        if (creation.IsFailure) return Result.Failure<ExamResultResponse>(creation.Error);
        resultRepository.Add(creation.Value); await unitOfWork.CommitAsync(cancellationToken); return Result.Success(Map(creation.Value));
    }

    private static string Fingerprint(RecordExamResultCommand x) => Hash($"{x.OrganizationId.Value:N}|{x.AttemptId.Value:N}|{(int)x.Outcome}|{x.Score}|{x.FailureReasonCode}|{x.SourceKind}|{x.ProviderCode}|{x.ExternalResultId}|{x.EvidenceDocumentId?.Value:N}|{x.ReceivedAtUtc:O}");
    internal static string Hash(string canonical) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    internal static ExamResultResponse Map(ExamResult x) => new(x.Id.Value, x.AttemptId.Value, x.RegistrationId.Value, x.StudentId.Value,
        x.AttemptNumber, x.CurrentRevision, x.Outcome.ToString(), x.Score, x.FailureReasonCode, x.Comments, x.SourceKind.ToString(),
        x.ProviderCode, x.ExternalResultId, x.EvidenceDocumentId?.Value, x.ReceivedAtUtc, x.Status.ToString(), x.VerifiedAtUtc,
        x.VerifiedByUserId?.Value, x.VerificationReference, x.FinalizedAtUtc, x.FinalizedByUserId?.Value,
        x.Revisions.OrderBy(y => y.RevisionNumber).Select(y => new ExamResultRevisionResponse(y.RevisionNumber, y.Outcome.ToString(), y.Score,
            y.FailureReasonCode, y.Comments, y.SourceKind.ToString(), y.ProviderCode, y.ExternalResultId, y.EvidenceDocumentId?.Value,
            y.ReceivedAtUtc, y.CorrectionReason, y.OperationId, y.ActorUserId.Value, y.CreatedAtUtc)).ToArray());
}

public sealed class VerifyExamResultCommandHandler(IExamResultRepository repository, IExamsCertificationUnitOfWork uow, IClock clock)
    : ICommandHandler<VerifyExamResultCommand, ExamResultResponse>
{
    public async Task<Result<ExamResultResponse>> Handle(VerifyExamResultCommand c, CancellationToken ct) { var x = await repository.GetByIdForUpdateAsync(c.OrganizationId,c.ResultId,ct); if(x is null)return Result.Failure<ExamResultResponse>(ExamResultErrors.NotFound); var r=x.Verify(c.VerificationReference,c.ActorUserId,clock.UtcNow); if(r.IsFailure)return Result.Failure<ExamResultResponse>(r.Error); await uow.CommitAsync(ct); return Result.Success(RecordExamResultCommandHandler.Map(x)); }
}

public sealed class FinalizeExamResultCommandHandler(
    IExamResultRepository repository,
    IExamAttemptRepository attempts,
    IExamSuccessConsequenceStore successConsequences,
    IExamSuccessProcessRepository successProcesses,
    IExamFailureAnalysisRepository failureAnalyses,
    IExamsCertificationUnitOfWork uow,
    IClock clock) : ICommandHandler<FinalizeExamResultCommand, ExamResultResponse>
{
    public async Task<Result<ExamResultResponse>> Handle(FinalizeExamResultCommand c, CancellationToken ct)
    {
        ExamResult? x = await repository.GetByIdForUpdateAsync(c.OrganizationId, c.ResultId, ct);
        if (x is null) return Result.Failure<ExamResultResponse>(ExamResultErrors.NotFound);

        ExamAttempt? attempt = await attempts.GetByIdAsync(c.OrganizationId, x.AttemptId, ct);
        if (attempt is null) return Result.Failure<ExamResultResponse>(ExamResultErrors.AttemptNotFound);

        DateTimeOffset now = clock.UtcNow;
        Result finalized = x.Finalize(c.ActorUserId, now);
        if (finalized.IsFailure) return Result.Failure<ExamResultResponse>(finalized.Error);

        if (x.Outcome == ExamResultOutcome.Passed)
        {
            ExamSuccessProcess? existingProcess = await successProcesses.GetByResultForUpdateAsync(c.OrganizationId, x.Id, x.CurrentRevision, ct);
            if (existingProcess is null)
                successProcesses.Add(ExamSuccessProcess.Create(c.OrganizationId, x.Id, x.CurrentRevision, x.AttemptId, x.RegistrationId, x.StudentId, x.AttemptNumber, c.ActorUserId, now));

            await successConsequences.EnqueueAsync(new ExamSuccessSnapshot(
                c.OrganizationId, x.Id, x.AttemptId, x.RegistrationId, x.StudentId, x.AttemptNumber, x.CurrentRevision,
                attempt.ExamType, attempt.LicenseCategory, attempt.CompletedAtUtc ?? now, x.FinalizedAtUtc ?? now, c.ActorUserId), ct);
        }
        else if (x.Outcome == ExamResultOutcome.Failed)
        {
            ExamFailureAnalysis? existingAnalysis = await failureAnalyses.GetByResultForUpdateAsync(c.OrganizationId, x.Id, x.CurrentRevision, ct);
            if (existingAnalysis is null)
                failureAnalyses.Add(ExamFailureAnalysis.Create(c.OrganizationId, x.Id, x.CurrentRevision, x.AttemptId, x.RegistrationId,
                    x.StudentId, x.AttemptNumber, x.FailureReasonCode, c.ActorUserId, now));
        }

        await uow.CommitAsync(ct);
        return Result.Success(RecordExamResultCommandHandler.Map(x));
    }
}

public sealed class CorrectExamResultCommandHandler(
    IExamResultRepository repository,
    IExamSuccessConsequenceStore successConsequences,
    IExamSuccessProcessRepository successProcesses,
    IExamFailureAnalysisRepository failureAnalyses,
    IExamRemediationRequestRepository remediationRequests,
    IExamAttestationRepository attestations,
    IExamsCertificationUnitOfWork uow,
    IClock clock) : ICommandHandler<CorrectExamResultCommand, ExamResultResponse>
{
    public async Task<Result<ExamResultResponse>> Handle(CorrectExamResultCommand c, CancellationToken ct)
    {
        ExamResult? x = await repository.GetByIdForUpdateAsync(c.OrganizationId, c.ResultId, ct);
        if (x is null) return Result.Failure<ExamResultResponse>(ExamResultErrors.NotFound);
        if (!Enum.TryParse<ExamResultSourceKind>(c.SourceKind, true, out var sourceKind))
            return Result.Failure<ExamResultResponse>(ExamResultErrors.InvalidSource);

        bool supersedesAnyFinalization = x.Status == ExamResultStatus.Finalized;
        bool supersedesFinalizedPass = x.Status == ExamResultStatus.Finalized && x.Outcome == ExamResultOutcome.Passed;
        bool supersedesFinalizedFailure = x.Status == ExamResultStatus.Finalized && x.Outcome == ExamResultOutcome.Failed;
        int supersededRevision = x.CurrentRevision;
        DateTimeOffset now = clock.UtcNow;
        string fp = RecordExamResultCommandHandler.Hash($"{c.OrganizationId.Value:N}|{c.ResultId.Value:N}|{(int)c.Outcome}|{c.Score}|{c.FailureReasonCode}|{c.SourceKind}|{c.ProviderCode}|{c.ExternalResultId}|{c.EvidenceDocumentId?.Value:N}|{c.ReceivedAtUtc:O}|{c.CorrectionReason}");

        await uow.BeginTransactionAsync(ct);
        try
        {
            Result corrected = x.Correct((ExamResultOutcome)(int)c.Outcome, c.Score, c.FailureReasonCode, c.Comments, sourceKind,
                c.ProviderCode, c.ExternalResultId, c.EvidenceDocumentId, c.ReceivedAtUtc, c.CorrectionReason, c.OperationId, fp, c.ActorUserId, now);
            if (corrected.IsFailure)
            {
                await uow.RollbackTransactionAsync(ct);
                return Result.Failure<ExamResultResponse>(corrected.Error);
            }

            if (supersedesFinalizedPass)
            {
                await successConsequences.SupersedeAsync(c.OrganizationId, x.Id, supersededRevision, now, ct);
                ExamSuccessProcess? successProcess = await successProcesses.GetByResultForUpdateAsync(c.OrganizationId, x.Id, supersededRevision, ct);
                successProcess?.Supersede(now);
            }

            if (supersedesFinalizedFailure)
            {
                ExamFailureAnalysis? failureAnalysis = await failureAnalyses.GetByResultForUpdateAsync(c.OrganizationId, x.Id, supersededRevision, ct);
                failureAnalysis?.Supersede(now);
                ExamRemediationRequest? remediation = await remediationRequests.GetByResultRevisionForUpdateAsync(c.OrganizationId, x.Id, supersededRevision, ct);
                remediation?.Supersede(now);
            }

            if (supersedesAnyFinalization)
            {
                IReadOnlyList<ExamAttestation> issuedAttestations = await attestations.ListByResultRevisionForUpdateAsync(c.OrganizationId, x.Id, supersededRevision, ct);
                foreach (ExamAttestation attestation in issuedAttestations)
                    attestation.Supersede(now);
            }

            await uow.CommitTransactionAsync(ct);
            return Result.Success(RecordExamResultCommandHandler.Map(x));
        }
        catch
        {
            await uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}

public sealed class GetExamResultByAttemptQueryHandler(IExamResultRepository repository) : IQueryHandler<GetExamResultByAttemptQuery, ExamResultResponse>
{
    public async Task<Result<ExamResultResponse>> Handle(GetExamResultByAttemptQuery q,CancellationToken ct){var x=await repository.GetByAttemptAsync(q.OrganizationId,q.AttemptId,ct);return x is null?Result.Failure<ExamResultResponse>(ExamResultErrors.NotFound):Result.Success(RecordExamResultCommandHandler.Map(x));}
}
public sealed class GetExamResultQueryHandler(IExamResultRepository repository) : IQueryHandler<GetExamResultQuery, ExamResultResponse>
{
    public async Task<Result<ExamResultResponse>> Handle(GetExamResultQuery q,CancellationToken ct){var x=await repository.GetByIdAsync(q.OrganizationId,q.ResultId,ct);return x is null?Result.Failure<ExamResultResponse>(ExamResultErrors.NotFound):Result.Success(RecordExamResultCommandHandler.Map(x));}
}


public sealed class GetStudentExamResultsQueryHandler(IExamResultRepository repository)
    : IQueryHandler<GetStudentExamResultsQuery, IReadOnlyList<ExamResultResponse>>
{
    public async Task<Result<IReadOnlyList<ExamResultResponse>>> Handle(GetStudentExamResultsQuery q, CancellationToken ct)
    {
        IReadOnlyList<ExamResult> results = await repository.ListByStudentAsync(q.OrganizationId, q.StudentId, ct);
        return Result.Success<IReadOnlyList<ExamResultResponse>>(results.Select(RecordExamResultCommandHandler.Map).ToArray());
    }
}
