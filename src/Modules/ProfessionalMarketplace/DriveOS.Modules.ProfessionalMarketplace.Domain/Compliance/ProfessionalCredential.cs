using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;

/// <summary>
/// Verified professional entitlement/fact. A credential is distinct from its evidence document.
/// </summary>
public sealed class ProfessionalCredential : AggregateRoot<ProfessionalCredentialId>, IAuditableEntity
{
    private ProfessionalCredential() { }
    private ProfessionalCredential(ProfessionalCredentialId id,ProfessionalProfileId profileId,string type,string country,string issuer,string? reference,DateOnly validFrom,DateOnly? validUntil,string[] categoryCodes,ProfessionalDocumentId? evidence):base(id)
    {ProfessionalProfileId=profileId;CredentialTypeCode=Token(type);CountryCode=Token(country);IssuingAuthority=issuer.Trim();ReferenceNumber=Norm(reference);ValidFrom=validFrom;ValidUntil=validUntil;CategoryCodes=Normalize(categoryCodes);EvidenceDocumentId=evidence;Status=ProfessionalCredentialStatus.PendingVerification;}
    public ProfessionalProfileId ProfessionalProfileId{get;private set;} public string CredentialTypeCode{get;private set;}=string.Empty; public string CountryCode{get;private set;}=string.Empty; public string IssuingAuthority{get;private set;}=string.Empty; public string? ReferenceNumber{get;private set;}
    public DateOnly ValidFrom{get;private set;} public DateOnly? ValidUntil{get;private set;} public string[] CategoryCodes{get;private set;}=[]; public ProfessionalDocumentId? EvidenceDocumentId{get;private set;} public ProfessionalCredentialStatus Status{get;private set;}
    public ProfessionalVerificationMethod? VerificationMethod{get;private set;} public DateTimeOffset? VerifiedAtUtc{get;private set;} public UserId? VerifiedByUserId{get;private set;} public string? RejectionReason{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;} public UserId? CreatedByUserId{get;private set;} public DateTimeOffset? LastModifiedAtUtc{get;private set;} public UserId? LastModifiedByUserId{get;private set;}
    public bool IsValidOn(DateOnly date)=>Status==ProfessionalCredentialStatus.Verified&&ValidFrom<=date&&(ValidUntil is null||date<=ValidUntil.Value);

    public static Result<ProfessionalCredential> Create(ProfessionalCredentialId id,ProfessionalProfileId profileId,string type,string country,string issuer,string? reference,DateOnly validFrom,DateOnly? validUntil,IEnumerable<string>? categories,ProfessionalDocumentId? evidence,DateTimeOffset now,UserId actor)
    {
        if(id.IsEmpty||profileId.IsEmpty)return Result.Failure<ProfessionalCredential>(ProfessionalComplianceErrors.InvalidIdentifier);
        var t=Token(type);var c=Token(country);issuer=(issuer??"").Trim();
        if(t.Length is <1 or >80||c.Length!=2||issuer.Length is <2 or >200)return Result.Failure<ProfessionalCredential>(ProfessionalComplianceErrors.InvalidCredentialMetadata);
        if(validUntil is DateOnly until&&until<validFrom)return Result.Failure<ProfessionalCredential>(ProfessionalComplianceErrors.InvalidValidityPeriod);
        var x=new ProfessionalCredential(id,profileId,t,c,issuer,reference,validFrom,validUntil,(categories??[]).ToArray(),evidence);x.SetCreatedAudit(now,actor);return Result.Success(x);
    }
    public Result Verify(ProfessionalVerificationMethod method,DateOnly today,DateTimeOffset now,UserId actor){if(Status!=ProfessionalCredentialStatus.PendingVerification)return Result.Failure(ProfessionalComplianceErrors.InvalidCredentialTransition);if(!IsDateValid(today))return Result.Failure(ProfessionalComplianceErrors.CredentialNotCurrentlyValid);Status=ProfessionalCredentialStatus.Verified;VerificationMethod=method;VerifiedAtUtc=now.ToUniversalTime();VerifiedByUserId=actor;RejectionReason=null;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Reject(string reason,DateTimeOffset now,UserId actor){if(Status!=ProfessionalCredentialStatus.PendingVerification)return Result.Failure(ProfessionalComplianceErrors.InvalidCredentialTransition);reason=(reason??"").Trim();if(reason.Length is <2 or >512)return Result.Failure(ProfessionalComplianceErrors.RejectionReasonRequired);Status=ProfessionalCredentialStatus.Rejected;RejectionReason=reason;VerificationMethod=ProfessionalVerificationMethod.Manual;VerifiedAtUtc=now.ToUniversalTime();VerifiedByUserId=actor;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Revoke(string reason,DateTimeOffset now,UserId actor){if(Status!=ProfessionalCredentialStatus.Verified)return Result.Failure(ProfessionalComplianceErrors.InvalidCredentialTransition);reason=(reason??"").Trim();if(reason.Length is <2 or >512)return Result.Failure(ProfessionalComplianceErrors.RejectionReasonRequired);Status=ProfessionalCredentialStatus.Revoked;RejectionReason=reason;SetModifiedAudit(now,actor);return Result.Success();}
    public Result MarkExpired(DateTimeOffset now,UserId? actor=null){if(Status is ProfessionalCredentialStatus.Expired or ProfessionalCredentialStatus.Revoked)return Result.Failure(ProfessionalComplianceErrors.InvalidCredentialTransition);Status=ProfessionalCredentialStatus.Expired;SetModifiedAudit(now,actor);return Result.Success();}
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;} public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private bool IsDateValid(DateOnly date)=>ValidFrom<=date&&(ValidUntil is null||date<=ValidUntil.Value); private static string Token(string? v)=>(v??"").Trim().ToUpperInvariant(); private static string? Norm(string? v)=>string.IsNullOrWhiteSpace(v)?null:v.Trim(); private static string[] Normalize(IEnumerable<string> values)=>values.Where(x=>!string.IsNullOrWhiteSpace(x)).Select(Token).Distinct(StringComparer.Ordinal).OrderBy(x=>x,StringComparer.Ordinal).ToArray();
}
public enum ProfessionalCredentialStatus{PendingVerification=1,Verified=2,Rejected=3,Expired=4,Revoked=5}
