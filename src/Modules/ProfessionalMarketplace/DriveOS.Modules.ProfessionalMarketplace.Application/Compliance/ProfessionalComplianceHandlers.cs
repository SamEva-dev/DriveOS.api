using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Compliance;

public sealed class RegisterProfessionalDocumentCommandHandler(IProfessionalDocumentRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<RegisterProfessionalDocumentCommand,ProfessionalDocumentId>
{
    public async Task<Result<ProfessionalDocumentId>> Handle(RegisterProfessionalDocumentCommand c,CancellationToken ct){if(await repo.DocumentReferenceExistsAsync(c.ProfileId,c.DocumentReferenceId,ct))return Result.Failure<ProfessionalDocumentId>(Error.Conflict("ProfessionalMarketplace.Compliance.DuplicateDocumentReference","errors.professionalMarketplace.compliance.duplicateDocumentReference"));var x=ProfessionalDocument.Create(c.Id,c.ProfileId,c.DocumentReferenceId,c.DocumentTypeCode,c.CountryCode,c.Mandatory,c.IssueDate,c.ExpirationDate,clock.UtcNow,c.ActorUserId);if(x.IsFailure)return Result.Failure<ProfessionalDocumentId>(x.Error);repo.Add(x.Value);await uow.CommitAsync(ct);return Result.Success(x.Value.Id);}
}
public sealed class SubmitProfessionalDocumentCommandHandler(IProfessionalDocumentRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<SubmitProfessionalDocumentCommand>{public async Task<Result> Handle(SubmitProfessionalDocumentCommand c,CancellationToken ct){var x=await repo.GetAsync(c.Id,true,ct);if(x is null)return Result.Failure(ProfessionalComplianceErrors.DocumentNotFound);var r=x.SubmitForReview(clock.UtcNow,c.ActorUserId);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}}
public sealed class ApproveProfessionalDocumentCommandHandler(IProfessionalDocumentRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<ApproveProfessionalDocumentCommand>{public async Task<Result> Handle(ApproveProfessionalDocumentCommand c,CancellationToken ct){var x=await repo.GetAsync(c.Id,true,ct);if(x is null)return Result.Failure(ProfessionalComplianceErrors.DocumentNotFound);var r=x.Approve(c.Method,DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),clock.UtcNow,c.ActorUserId);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}}
public sealed class RejectProfessionalDocumentCommandHandler(IProfessionalDocumentRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<RejectProfessionalDocumentCommand>{public async Task<Result> Handle(RejectProfessionalDocumentCommand c,CancellationToken ct){var x=await repo.GetAsync(c.Id,true,ct);if(x is null)return Result.Failure(ProfessionalComplianceErrors.DocumentNotFound);var r=x.Reject(c.Reason,clock.UtcNow,c.ActorUserId);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}}
public sealed class RegisterProfessionalCredentialCommandHandler(IProfessionalCredentialRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<RegisterProfessionalCredentialCommand,ProfessionalCredentialId>{public async Task<Result<ProfessionalCredentialId>> Handle(RegisterProfessionalCredentialCommand c,CancellationToken ct){if(await repo.DuplicateExistsAsync(c.ProfileId,c.CredentialTypeCode,c.CountryCode,c.ReferenceNumber,ct))return Result.Failure<ProfessionalCredentialId>(Error.Conflict("ProfessionalMarketplace.Compliance.DuplicateCredential","errors.professionalMarketplace.compliance.duplicateCredential"));var x=ProfessionalCredential.Create(c.Id,c.ProfileId,c.CredentialTypeCode,c.CountryCode,c.IssuingAuthority,c.ReferenceNumber,c.ValidFrom,c.ValidUntil,c.CategoryCodes,c.EvidenceDocumentId,clock.UtcNow,c.ActorUserId);if(x.IsFailure)return Result.Failure<ProfessionalCredentialId>(x.Error);repo.Add(x.Value);await uow.CommitAsync(ct);return Result.Success(x.Value.Id);}}
public sealed class VerifyProfessionalCredentialCommandHandler(IProfessionalCredentialRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<VerifyProfessionalCredentialCommand>{public async Task<Result> Handle(VerifyProfessionalCredentialCommand c,CancellationToken ct){var x=await repo.GetAsync(c.Id,true,ct);if(x is null)return Result.Failure(ProfessionalComplianceErrors.CredentialNotFound);var r=x.Verify(c.Method,DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),clock.UtcNow,c.ActorUserId);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}}
public sealed class RejectProfessionalCredentialCommandHandler(IProfessionalCredentialRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<RejectProfessionalCredentialCommand>{public async Task<Result> Handle(RejectProfessionalCredentialCommand c,CancellationToken ct){var x=await repo.GetAsync(c.Id,true,ct);if(x is null)return Result.Failure(ProfessionalComplianceErrors.CredentialNotFound);var r=x.Reject(c.Reason,clock.UtcNow,c.ActorUserId);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}}

public sealed class CreateProfessionalComplianceRequirementCommandHandler(
    IProfessionalComplianceRequirementRepository requirements,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<CreateProfessionalComplianceRequirementCommand,ProfessionalComplianceRequirementId>
{
    public async Task<Result<ProfessionalComplianceRequirementId>> Handle(CreateProfessionalComplianceRequirementCommand c,CancellationToken ct)
    {
        if(await requirements.ActiveVersionExistsAsync(c.RequirementCode,c.CountryCode,c.ProfessionalType,c.Version,ct))
            return Result.Failure<ProfessionalComplianceRequirementId>(ProfessionalComplianceErrors.DuplicateRequirementVersion);
        var created=ProfessionalComplianceRequirement.Create(c.Id,c.RequirementCode,c.CountryCode,c.ProfessionalType,c.EvidenceKind,c.EvidenceTypeCode,c.Mandatory,c.Blocking,c.ApplicableCategoryCodes,c.EffectiveFrom,c.EffectiveTo,c.Version,clock.UtcNow,c.ActorUserId);
        if(created.IsFailure)return Result.Failure<ProfessionalComplianceRequirementId>(created.Error);
        requirements.Add(created.Value); await uow.CommitAsync(ct); return Result.Success(created.Value.Id);
    }
}

public sealed class ReevaluateProfessionalComplianceCommandHandler(
    IProfessionalProfileRepository profiles,
    IProfessionalComplianceRequirementRepository requirements,
    IProfessionalDocumentRepository documents,
    IProfessionalCredentialRepository credentials,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<ReevaluateProfessionalComplianceCommand,ProfessionalComplianceEvaluation>
{
    public async Task<Result<ProfessionalComplianceEvaluation>> Handle(ReevaluateProfessionalComplianceCommand c,CancellationToken ct)
    {
        var profile=await profiles.GetByIdForUpdateAsync(c.ProfileId,ct);
        if(profile is null)return Result.Failure<ProfessionalComplianceEvaluation>(ProfessionalComplianceErrors.ProfileNotFound);
        if(string.IsNullOrWhiteSpace(profile.BillingCountryCode))
            return Result.Failure<ProfessionalComplianceEvaluation>(ProfessionalComplianceErrors.InvalidRequirement);

        var today=DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var applicable=await requirements.ListApplicableAsync(profile.BillingCountryCode,profile.ProfessionalType,profile.TeachingCategoryCodes,today,ct);
        var docs=await documents.ListByProfileAsync(profile.Id,ct);
        var creds=await credentials.ListByProfileAsync(profile.Id,ct);

        var missing=new List<string>();
        var invalid=new List<string>();
        var pending=new List<string>();

        foreach(var requirement in applicable.Where(x=>x.Mandatory))
        {
            if(requirement.EvidenceKind==ProfessionalEvidenceKind.Document)
            {
                var matches=docs.Where(x=>x.DocumentTypeCode==requirement.EvidenceTypeCode&&x.CountryCode==requirement.CountryCode).ToArray();
                if(matches.Length==0){missing.Add(requirement.RequirementCode);continue;}
                if(matches.Any(x=>x.IsValidOn(today)))continue;
                if(matches.Any(x=>x.Status is ProfessionalDocumentStatus.Uploaded or ProfessionalDocumentStatus.PendingReview or ProfessionalDocumentStatus.ExpiringSoon)){pending.Add(requirement.RequirementCode);continue;}
                invalid.Add(requirement.RequirementCode);
                continue;
            }

            var credentialMatches=creds.Where(x=>x.CredentialTypeCode==requirement.EvidenceTypeCode&&x.CountryCode==requirement.CountryCode).ToArray();
            if(requirement.ApplicableCategoryCodes.Length>0)
                credentialMatches=credentialMatches.Where(x=>requirement.ApplicableCategoryCodes.All(cat=>x.CategoryCodes.Contains(cat,StringComparer.Ordinal))).ToArray();
            if(credentialMatches.Length==0){missing.Add(requirement.RequirementCode);continue;}
            if(credentialMatches.Any(x=>x.IsValidOn(today)))continue;
            if(credentialMatches.Any(x=>x.Status==ProfessionalCredentialStatus.PendingVerification)){pending.Add(requirement.RequirementCode);continue;}
            invalid.Add(requirement.RequirementCode);
        }

        ProfessionalComplianceStatus status;
        if(!profile.IsProfileComplete) status=ProfessionalComplianceStatus.Incomplete;
        else if(invalid.Count>0 && applicable.Where(x=>x.Blocking).Any(x=>invalid.Contains(x.RequirementCode,StringComparer.Ordinal))) status=ProfessionalComplianceStatus.NonCompliant;
        else if(missing.Count>0) status=ProfessionalComplianceStatus.Incomplete;
        else if(pending.Count>0) status=ProfessionalComplianceStatus.PendingReview;
        else if(invalid.Count>0) status=ProfessionalComplianceStatus.PartiallyCompliant;
        else status=ProfessionalComplianceStatus.Compliant;

        var update=profile.MarkCompliance(status,clock.UtcNow,c.ActorUserId);
        if(update.IsFailure)return Result.Failure<ProfessionalComplianceEvaluation>(update.Error);
        await uow.CommitAsync(ct);
        return Result.Success(new ProfessionalComplianceEvaluation(status,missing.ToArray(),invalid.ToArray(),pending.ToArray(),clock.UtcNow));
    }
}


public sealed class GetProfessionalComplianceQueryHandler(
    IProfessionalProfileRepository profiles,
    IProfessionalDocumentRepository documents,
    IProfessionalCredentialRepository credentials) : IQueryHandler<GetProfessionalComplianceQuery,ProfessionalComplianceResponse>
{
    public async Task<Result<ProfessionalComplianceResponse>> Handle(GetProfessionalComplianceQuery q,CancellationToken ct)
    {
        var profile=await profiles.GetByIdAsync(q.ProfileId,ct);
        if(profile is null)return Result.Failure<ProfessionalComplianceResponse>(ProfessionalComplianceErrors.ProfileNotFound);
        var docs=await documents.ListByProfileAsync(q.ProfileId,ct);
        var creds=await credentials.ListByProfileAsync(q.ProfileId,ct);
        return Result.Success(new ProfessionalComplianceResponse(
            q.ProfileId.Value,profile.ComplianceStatus,
            docs.OrderByDescending(x=>x.CreatedAtUtc).Select(x=>new ProfessionalComplianceDocumentResponse(
                x.Id.Value,x.DocumentReferenceId,x.DocumentTypeCode,x.CountryCode,x.Mandatory,x.IssueDate,x.ExpirationDate,x.Status,
                x.VerificationMethod,x.VerifiedAtUtc,x.VerifiedByUserId?.Value,x.RejectionReason,x.SupersededById?.Value)).ToArray(),
            creds.OrderByDescending(x=>x.CreatedAtUtc).Select(x=>new ProfessionalComplianceCredentialResponse(
                x.Id.Value,x.CredentialTypeCode,x.CountryCode,x.IssuingAuthority,x.ReferenceNumber,x.ValidFrom,x.ValidUntil,x.CategoryCodes,
                x.EvidenceDocumentId?.Value,x.Status,x.VerificationMethod,x.VerifiedAtUtc,x.VerifiedByUserId?.Value,x.RejectionReason)).ToArray()));
    }
}
