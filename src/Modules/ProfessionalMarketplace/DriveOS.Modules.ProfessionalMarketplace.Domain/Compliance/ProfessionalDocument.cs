using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;

/// <summary>
/// BC-13 reference to professional evidence owned physically by BC-06.
/// Marketplace owns classification, jurisdiction, review status and validity only.
/// </summary>
public sealed class ProfessionalDocument : AggregateRoot<ProfessionalDocumentId>, IAuditableEntity
{
    private ProfessionalDocument() { }

    private ProfessionalDocument(ProfessionalDocumentId id,ProfessionalProfileId profileId,Guid documentReferenceId,string documentTypeCode,string countryCode,bool mandatory,DateOnly? issueDate,DateOnly? expirationDate):base(id)
    {
        ProfessionalProfileId=profileId; DocumentReferenceId=documentReferenceId; DocumentTypeCode=Token(documentTypeCode);
        CountryCode=Token(countryCode); Mandatory=mandatory; IssueDate=issueDate; ExpirationDate=expirationDate; Status=ProfessionalDocumentStatus.Uploaded;
    }

    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public Guid DocumentReferenceId{get;private set;}
    public string DocumentTypeCode{get;private set;}=string.Empty;
    public string CountryCode{get;private set;}=string.Empty;
    public bool Mandatory{get;private set;}
    public DateOnly? IssueDate{get;private set;}
    public DateOnly? ExpirationDate{get;private set;}
    public ProfessionalDocumentStatus Status{get;private set;}
    public ProfessionalVerificationMethod? VerificationMethod{get;private set;}
    public DateTimeOffset? VerifiedAtUtc{get;private set;}
    public UserId? VerifiedByUserId{get;private set;}
    public string? RejectionReason{get;private set;}
    public ProfessionalDocumentId? SupersededById{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public bool IsExpired(DateOnly date)=>ExpirationDate is DateOnly e&&e<date;
    public bool IsValidOn(DateOnly date)=>(Status is ProfessionalDocumentStatus.Valid or ProfessionalDocumentStatus.ExpiringSoon)&&!IsExpired(date);

    public static Result<ProfessionalDocument> Create(ProfessionalDocumentId id,ProfessionalProfileId profileId,Guid documentReferenceId,string documentTypeCode,string countryCode,bool mandatory,DateOnly? issueDate,DateOnly? expirationDate,DateTimeOffset now,UserId actor)
    {
        if(id.IsEmpty||profileId.IsEmpty||documentReferenceId==Guid.Empty)return Result.Failure<ProfessionalDocument>(ProfessionalComplianceErrors.InvalidIdentifier);
        var type=Token(documentTypeCode);var country=Token(countryCode);
        if(type.Length is <1 or >80||country.Length!=2)return Result.Failure<ProfessionalDocument>(ProfessionalComplianceErrors.InvalidDocumentMetadata);
        if(expirationDate is DateOnly exp&&issueDate is DateOnly issue&&exp<issue)return Result.Failure<ProfessionalDocument>(ProfessionalComplianceErrors.InvalidValidityPeriod);
        var x=new ProfessionalDocument(id,profileId,documentReferenceId,type,country,mandatory,issueDate,expirationDate);x.SetCreatedAudit(now,actor);return Result.Success(x);
    }

    public Result SubmitForReview(DateTimeOffset now,UserId actor){if(Status is not ProfessionalDocumentStatus.Uploaded and not ProfessionalDocumentStatus.Rejected)return Result.Failure(ProfessionalComplianceErrors.InvalidDocumentTransition);Status=ProfessionalDocumentStatus.PendingReview;RejectionReason=null;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Approve(ProfessionalVerificationMethod method,DateOnly today,DateTimeOffset now,UserId actor){if(Status!=ProfessionalDocumentStatus.PendingReview)return Result.Failure(ProfessionalComplianceErrors.InvalidDocumentTransition);if(IsExpired(today))return Result.Failure(ProfessionalComplianceErrors.DocumentExpired);Status=ProfessionalDocumentStatus.Valid;VerificationMethod=method;VerifiedAtUtc=now.ToUniversalTime();VerifiedByUserId=actor;RejectionReason=null;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Reject(string reason,DateTimeOffset now,UserId actor){if(Status!=ProfessionalDocumentStatus.PendingReview)return Result.Failure(ProfessionalComplianceErrors.InvalidDocumentTransition);reason=(reason??"").Trim();if(reason.Length is <2 or >512)return Result.Failure(ProfessionalComplianceErrors.RejectionReasonRequired);Status=ProfessionalDocumentStatus.Rejected;RejectionReason=reason;VerificationMethod=ProfessionalVerificationMethod.Manual;VerifiedAtUtc=now.ToUniversalTime();VerifiedByUserId=actor;SetModifiedAudit(now,actor);return Result.Success();}
    public Result MarkExpiringSoon(DateTimeOffset now,UserId? actor=null)
    {
        if(Status!=ProfessionalDocumentStatus.Valid)
            return Result.Failure(ProfessionalComplianceErrors.InvalidDocumentTransition);
        Status=ProfessionalDocumentStatus.ExpiringSoon;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result MarkExpired(DateTimeOffset now,UserId? actor=null){if(Status is ProfessionalDocumentStatus.Expired or ProfessionalDocumentStatus.Superseded)return Result.Failure(ProfessionalComplianceErrors.InvalidDocumentTransition);Status=ProfessionalDocumentStatus.Expired;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Supersede(ProfessionalDocumentId replacement,DateTimeOffset now,UserId actor){if(replacement.IsEmpty||replacement==Id)return Result.Failure(ProfessionalComplianceErrors.InvalidIdentifier);Status=ProfessionalDocumentStatus.Superseded;SupersededById=replacement;SetModifiedAudit(now,actor);return Result.Success();}
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;} public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string Token(string? value)=>(value??"").Trim().ToUpperInvariant();
}
public enum ProfessionalDocumentStatus{Missing=1,Uploaded=2,PendingReview=3,Valid=4,Rejected=5,Expired=6,ExpiringSoon=7,Superseded=8}
public enum ProfessionalVerificationMethod{Manual=1,Automated=2,ImportedVerifiedEvidence=3}
