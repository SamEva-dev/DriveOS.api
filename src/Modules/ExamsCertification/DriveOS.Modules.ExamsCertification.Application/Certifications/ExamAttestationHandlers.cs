using System.Security.Cryptography;
using System.Text;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Certifications;
using DriveOS.Modules.ExamsCertification.Domain.Results;
using DriveOS.Modules.ExamsCertification.Domain.Results.Success;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Certifications;

internal static class ExamAttestationMapping
{
    public static ExamAttestationResponse Map(ExamAttestation x) => new(x.Id.Value, x.ExamResultId.Value, x.ResultRevision,
        x.ExamAttemptId.Value, x.ExamRegistrationId.Value, x.StudentId.Value, x.AttemptNumber, x.Type.ToString(), x.Reference,
        x.CurrentVersion, x.SupersedesAttestationId?.Value, x.Status.ToString(), x.IssuedAtUtc, x.IssuedByUserId.Value,
        x.ExpiresAtUtc, x.DeliveredAtUtc, x.DeliveredByUserId?.Value, x.DeliveryChannel?.ToString(), x.RevokedAtUtc,
        x.RevokedByUserId?.Value, x.RevocationReasonCode, x.RevocationNotes, x.SupersededAtUtc,
        x.Revisions.OrderByDescending(r => r.Version).Select(r => new ExamAttestationRevisionResponse(r.Id.Value, r.Version,
            r.TemplateCode, r.TemplateVersion, r.DocumentId.Value, r.DocumentSha256, !string.IsNullOrWhiteSpace(r.PublicVerificationTokenHash),
            r.SignatureProcessReference, r.SignatureEvidenceHash, r.SignedByUserId?.Value, r.SignedAtUtc,
            r.GeneratedByUserId.Value, r.GeneratedAtUtc)).ToArray());
    public static string HashToken(string? token) => string.IsNullOrWhiteSpace(token) ? string.Empty : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim())));
}

public sealed class IssueExamAttestationCommandHandler(IExamAttestationRepository attestations,IExamResultRepository results,IExamSuccessProcessRepository successProcesses,IExamsCertificationUnitOfWork uow,IClock clock) : ICommandHandler<IssueExamAttestationCommand, ExamAttestationResponse>
{
 public async Task<Result<ExamAttestationResponse>> Handle(IssueExamAttestationCommand c,CancellationToken ct)
 {
  if(!Enum.TryParse<ExamAttestationType>(c.Type,true,out var type)) return Result.Failure<ExamAttestationResponse>(ExamAttestationErrors.InvalidType);
  string tokenHash=ExamAttestationMapping.HashToken(c.PublicVerificationToken);
  string fingerprint=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{c.OrganizationId.Value:N}|{c.ResultId.Value:N}|{type}|{c.Reference}|{c.TemplateCode}|{c.TemplateVersion}|{c.DocumentId.Value:N}|{c.DocumentSha256}|{tokenHash}|{c.ExpiresAtUtc:O}|{c.SupersedesAttestationId?.Value:N}")));
  var replay=await attestations.FindByOperationIdAsync(c.OrganizationId,c.OperationId,ct); if(replay is not null) return replay.MatchesOperation(c.OperationId,fingerprint)?Result.Success(ExamAttestationMapping.Map(replay)):Result.Failure<ExamAttestationResponse>(ExamAttestationErrors.OperationConflict);
  var result=await results.GetByIdAsync(c.OrganizationId,c.ResultId,ct); if(result is null)return Result.Failure<ExamAttestationResponse>(ExamResultErrors.NotFound); if(result.Status!=ExamResultStatus.Finalized)return Result.Failure<ExamAttestationResponse>(ExamAttestationErrors.FinalizedResultRequired); if(type==ExamAttestationType.SuccessAttestation&&result.Outcome!=ExamResultOutcome.Passed)return Result.Failure<ExamAttestationResponse>(ExamAttestationErrors.SuccessRequired);
  ExamAttestation? previous=null; if(c.SupersedesAttestationId is {} pid){previous=await attestations.GetForUpdateAsync(c.OrganizationId,pid,ct);if(previous is null||previous.ExamResultId!=result.Id||previous.Type!=type)return Result.Failure<ExamAttestationResponse>(ExamAttestationErrors.NotFound);} else if(await attestations.GetCurrentAsync(c.OrganizationId,result.Id,type,ct) is not null)return Result.Failure<ExamAttestationResponse>(ExamAttestationErrors.OperationConflict);
  var created=ExamAttestation.Issue(c.OrganizationId,result.Id,result.CurrentRevision,result.AttemptId,result.RegistrationId,result.StudentId,result.AttemptNumber,type,c.Reference,previous?.Id,c.TemplateCode,c.TemplateVersion,c.DocumentId,c.DocumentSha256,string.IsNullOrEmpty(tokenHash)?null:tokenHash,c.ExpiresAtUtc,c.OperationId,fingerprint,c.ActorUserId,clock.UtcNow); if(created.IsFailure)return Result.Failure<ExamAttestationResponse>(created.Error);
  await uow.BeginTransactionAsync(ct); try{previous?.Supersede(clock.UtcNow);attestations.Add(created.Value);if(type==ExamAttestationType.SuccessAttestation&&result.Outcome==ExamResultOutcome.Passed){var process=await successProcesses.GetByResultForUpdateAsync(c.OrganizationId,result.Id,result.CurrentRevision,ct);if(process is not null){var rr=process.ApplyConsequence(ExamSuccessActionCode.PrepareCertification,ExamSuccessActionStatus.Completed,$"exam-attestation:{created.Value.Id.Value:N}",null,null,c.ActorUserId,clock.UtcNow);if(rr.IsFailure){await uow.RollbackTransactionAsync(ct);return Result.Failure<ExamAttestationResponse>(rr.Error);}}}await uow.CommitTransactionAsync(ct);return Result.Success(ExamAttestationMapping.Map(created.Value));}catch{await uow.RollbackTransactionAsync(ct);throw;}
 }
}

public abstract class AttestationMutationHandlerBase
{
 protected static async Task<Result<ExamAttestationResponse>> Mutate(OrganizationId org,ExamAttestationId id,Func<ExamAttestation,Result> action,IExamAttestationRepository repo,IExamsCertificationUnitOfWork uow,CancellationToken ct){var x=await repo.GetForUpdateAsync(org,id,ct);if(x is null)return Result.Failure<ExamAttestationResponse>(ExamAttestationErrors.NotFound);var r=action(x);if(r.IsFailure)return Result.Failure<ExamAttestationResponse>(r.Error);await uow.CommitAsync(ct);return Result.Success(ExamAttestationMapping.Map(x));}
}
public sealed class CorrectExamAttestationDocumentCommandHandler(IExamAttestationRepository repo,IExamsCertificationUnitOfWork uow,IClock clock):AttestationMutationHandlerBase,ICommandHandler<CorrectExamAttestationDocumentCommand,ExamAttestationResponse>{public Task<Result<ExamAttestationResponse>> Handle(CorrectExamAttestationDocumentCommand c,CancellationToken ct)=>Mutate(c.OrganizationId,c.AttestationId,x=>x.CorrectDocument(c.TemplateCode,c.TemplateVersion,c.DocumentId,c.DocumentSha256,string.IsNullOrWhiteSpace(c.PublicVerificationToken)?null:ExamAttestationMapping.HashToken(c.PublicVerificationToken),c.ActorUserId,clock.UtcNow),repo,uow,ct);}
public sealed class SignExamAttestationCommandHandler(IExamAttestationRepository repo,IExamsCertificationUnitOfWork uow,IClock clock):AttestationMutationHandlerBase,ICommandHandler<SignExamAttestationCommand,ExamAttestationResponse>{public Task<Result<ExamAttestationResponse>> Handle(SignExamAttestationCommand c,CancellationToken ct)=>Mutate(c.OrganizationId,c.AttestationId,x=>x.RecordSignature(c.SignatureProcessReference,c.SignatureEvidenceHash,c.ActorUserId,clock.UtcNow),repo,uow,ct);}
public sealed class DeliverExamAttestationCommandHandler(IExamAttestationRepository repo,IExamsCertificationUnitOfWork uow,IClock clock):AttestationMutationHandlerBase,ICommandHandler<DeliverExamAttestationCommand,ExamAttestationResponse>{public Task<Result<ExamAttestationResponse>> Handle(DeliverExamAttestationCommand c,CancellationToken ct)=>Enum.TryParse<ExamAttestationDeliveryChannel>(c.DeliveryChannel,true,out var channel)?Mutate(c.OrganizationId,c.AttestationId,x=>x.MarkDelivered(channel,c.ActorUserId,clock.UtcNow),repo,uow,ct):Task.FromResult(Result.Failure<ExamAttestationResponse>(ExamAttestationErrors.InvalidDelivery));}
public sealed class RevokeExamAttestationCommandHandler(IExamAttestationRepository repo,IExamsCertificationUnitOfWork uow,IClock clock):AttestationMutationHandlerBase,ICommandHandler<RevokeExamAttestationCommand,ExamAttestationResponse>{public Task<Result<ExamAttestationResponse>> Handle(RevokeExamAttestationCommand c,CancellationToken ct)=>Mutate(c.OrganizationId,c.AttestationId,x=>x.Revoke(c.ReasonCode,c.Notes,c.ActorUserId,clock.UtcNow),repo,uow,ct);}
public sealed class GetExamAttestationQueryHandler(IExamAttestationRepository repo):IQueryHandler<GetExamAttestationQuery,ExamAttestationResponse>{public async Task<Result<ExamAttestationResponse>> Handle(GetExamAttestationQuery q,CancellationToken ct){var x=await repo.GetAsync(q.OrganizationId,q.AttestationId,ct);return x is null?Result.Failure<ExamAttestationResponse>(ExamAttestationErrors.NotFound):Result.Success(ExamAttestationMapping.Map(x));}}
public sealed class GetExamResultAttestationsQueryHandler(IExamAttestationRepository repo):IQueryHandler<GetExamResultAttestationsQuery,IReadOnlyList<ExamAttestationResponse>>{public async Task<Result<IReadOnlyList<ExamAttestationResponse>>> Handle(GetExamResultAttestationsQuery q,CancellationToken ct)=>Result.Success<IReadOnlyList<ExamAttestationResponse>>((await repo.ListByResultAsync(q.OrganizationId,q.ResultId,ct)).Select(ExamAttestationMapping.Map).ToArray());}
public sealed class GetStudentExamAttestationsQueryHandler(IExamAttestationRepository repo):IQueryHandler<GetStudentExamAttestationsQuery,IReadOnlyList<ExamAttestationResponse>>{public async Task<Result<IReadOnlyList<ExamAttestationResponse>>> Handle(GetStudentExamAttestationsQuery q,CancellationToken ct)=>Result.Success<IReadOnlyList<ExamAttestationResponse>>((await repo.ListByStudentAsync(q.OrganizationId,q.StudentId,ct)).Select(ExamAttestationMapping.Map).ToArray());}
public sealed class VerifyExamAttestationQueryHandler(IExamAttestationRepository repo,IClock clock):IQueryHandler<VerifyExamAttestationQuery,PublicExamAttestationVerificationResponse>{public async Task<Result<PublicExamAttestationVerificationResponse>> Handle(VerifyExamAttestationQuery q,CancellationToken ct){if(string.IsNullOrWhiteSpace(q.PublicToken))return Result.Failure<PublicExamAttestationVerificationResponse>(ExamAttestationErrors.VerificationTokenRequired);var hash=ExamAttestationMapping.HashToken(q.PublicToken);var x=await repo.FindByPublicVerificationTokenHashAsync(hash,ct);if(x is null)return Result.Failure<PublicExamAttestationVerificationResponse>(ExamAttestationErrors.NotFound);var revision=x.Revisions.Single(r=>r.PublicVerificationTokenHash==hash);return Result.Success(new PublicExamAttestationVerificationResponse(x.Id.Value,x.Type.ToString(),x.Reference,x.Status.ToString(),revision.Version,x.IssuedAtUtc,x.ExpiresAtUtc,x.IsPubliclyValid(clock.UtcNow),revision.SignedAtUtc.HasValue));}}
