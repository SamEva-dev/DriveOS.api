using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Workforce.Domain.EmployeeDocuments;

public enum EmployeeDocumentCategory { Identity=1, Employment=2, Qualification=3, RegulatoryAuthorization=4, LeaveEvidence=5, OccupationalHealth=6, Payroll=7, Administrative=8, Other=99 }
public enum EmployeeDocumentConfidentiality { Internal=1, Confidential=2, Restricted=3 }
public enum EmployeeDocumentStatus { Registered=1, Verified=2, Superseded=3, Revoked=4, Archived=5 }

/// <summary>
/// HR registry entry referencing a document owned by the document bounded context. Workforce owns the HR
/// classification, validity, confidentiality and verification lifecycle only; binary content, versions,
/// signatures, hashes and signature evidence remain owned by BC-06.
/// </summary>
public sealed class EmployeeDocument : AggregateRoot<EmployeeDocumentId>, IAuditableEntity
{
    private EmployeeDocument() { }
    private EmployeeDocument(EmployeeDocumentId id, OrganizationId organizationId, EmployeeId employeeId, Guid documentReferenceId,
        EmployeeDocumentCategory category, string typeCode, string title, EmployeeDocumentConfidentiality confidentiality,
        DateOnly? issuedOn, DateOnly? validFrom, DateOnly? expiresOn, string? issuer, string? referenceNumber) : base(id)
    { OrganizationId=organizationId; EmployeeId=employeeId; DocumentReferenceId=documentReferenceId; Category=category; DocumentTypeCode=typeCode; Title=title; Confidentiality=confidentiality; IssuedOn=issuedOn; ValidFrom=validFrom; ExpiresOn=expiresOn; Issuer=issuer; ReferenceNumber=referenceNumber; Status=EmployeeDocumentStatus.Registered; }
    public OrganizationId OrganizationId{get;private set;} public EmployeeId EmployeeId{get;private set;} public Guid DocumentReferenceId{get;private set;}
    public EmployeeDocumentCategory Category{get;private set;} public string DocumentTypeCode{get;private set;}=string.Empty; public string Title{get;private set;}=string.Empty;
    public EmployeeDocumentConfidentiality Confidentiality{get;private set;} public DateOnly? IssuedOn{get;private set;} public DateOnly? ValidFrom{get;private set;} public DateOnly? ExpiresOn{get;private set;}
    public string? Issuer{get;private set;} public string? ReferenceNumber{get;private set;} public EmployeeDocumentStatus Status{get;private set;}
    public DateTimeOffset? VerifiedAtUtc{get;private set;} public UserId? VerifiedByUserId{get;private set;} public string? RevocationReason{get;private set;} public EmployeeDocumentId? SupersededByEmployeeDocumentId{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;} public UserId? CreatedByUserId{get;private set;} public DateTimeOffset? LastModifiedAtUtc{get;private set;} public UserId? LastModifiedByUserId{get;private set;}
    public bool IsExpired(DateOnly date)=>ExpiresOn is DateOnly e&&e<date;
    public static Result<EmployeeDocument> Create(EmployeeDocumentId id,OrganizationId org,EmployeeId employee,Guid reference,EmployeeDocumentCategory category,string typeCode,string title,EmployeeDocumentConfidentiality confidentiality,DateOnly? issuedOn,DateOnly? validFrom,DateOnly? expiresOn,string? issuer,string? referenceNumber,DateTimeOffset now,UserId actor)
    { if(id.IsEmpty||org.IsEmpty||employee.IsEmpty||reference==Guid.Empty)return Result.Failure<EmployeeDocument>(EmployeeDocumentErrors.InvalidIdentifier); var v=Validate(typeCode,title,issuedOn,validFrom,expiresOn,issuer,referenceNumber); if(v.IsFailure)return Result.Failure<EmployeeDocument>(v.Error); var x=new EmployeeDocument(id,org,employee,reference,category,typeCode.Trim().ToUpperInvariant(),title.Trim(),confidentiality,issuedOn,validFrom,expiresOn,N(issuer),N(referenceNumber)); x.SetCreatedAudit(now,actor); return Result.Success(x); }
    public Result UpdateMetadata(EmployeeDocumentCategory category,string typeCode,string title,EmployeeDocumentConfidentiality confidentiality,DateOnly? issuedOn,DateOnly? validFrom,DateOnly? expiresOn,string? issuer,string? referenceNumber,DateTimeOffset now,UserId actor)
    { if(Status is EmployeeDocumentStatus.Superseded or EmployeeDocumentStatus.Revoked or EmployeeDocumentStatus.Archived)return Result.Failure(EmployeeDocumentErrors.NotEditable); var v=Validate(typeCode,title,issuedOn,validFrom,expiresOn,issuer,referenceNumber); if(v.IsFailure)return v; Category=category;DocumentTypeCode=typeCode.Trim().ToUpperInvariant();Title=title.Trim();Confidentiality=confidentiality;IssuedOn=issuedOn;ValidFrom=validFrom;ExpiresOn=expiresOn;Issuer=N(issuer);ReferenceNumber=N(referenceNumber);SetModifiedAudit(now,actor);return Result.Success(); }
    public Result Verify(DateTimeOffset now,UserId actor){if(Status!=EmployeeDocumentStatus.Registered)return Result.Failure(EmployeeDocumentErrors.InvalidTransition);Status=EmployeeDocumentStatus.Verified;VerifiedAtUtc=now.ToUniversalTime();VerifiedByUserId=actor;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Supersede(EmployeeDocumentId replacement,DateTimeOffset now,UserId actor){if(replacement.IsEmpty||replacement==Id)return Result.Failure(EmployeeDocumentErrors.InvalidIdentifier);if(Status is EmployeeDocumentStatus.Superseded or EmployeeDocumentStatus.Revoked or EmployeeDocumentStatus.Archived)return Result.Failure(EmployeeDocumentErrors.InvalidTransition);Status=EmployeeDocumentStatus.Superseded;SupersededByEmployeeDocumentId=replacement;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Revoke(string reason,DateTimeOffset now,UserId actor){if(Status is EmployeeDocumentStatus.Superseded or EmployeeDocumentStatus.Revoked or EmployeeDocumentStatus.Archived)return Result.Failure(EmployeeDocumentErrors.InvalidTransition);reason=(reason??string.Empty).Trim();if(reason.Length is <1 or >512)return Result.Failure(EmployeeDocumentErrors.RevocationReasonRequired);Status=EmployeeDocumentStatus.Revoked;RevocationReason=reason;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Archive(DateTimeOffset now,UserId actor){if(Status==EmployeeDocumentStatus.Archived)return Result.Failure(EmployeeDocumentErrors.InvalidTransition);Status=EmployeeDocumentStatus.Archived;SetModifiedAudit(now,actor);return Result.Success();}
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;} public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static Result Validate(string typeCode,string title,DateOnly? issued,DateOnly? validFrom,DateOnly? expires,string? issuer,string? reference){typeCode=(typeCode??string.Empty).Trim();title=(title??string.Empty).Trim();if(typeCode.Length is <1 or >80||title.Length is <1 or >256||(issuer?.Trim().Length??0)>256||(reference?.Trim().Length??0)>128)return Result.Failure(EmployeeDocumentErrors.InvalidMetadata);var baseline=validFrom??issued??DateOnly.MinValue;if(expires is DateOnly e&&e<baseline)return Result.Failure(EmployeeDocumentErrors.InvalidValidityPeriod);return Result.Success();}
    private static string? N(string? v)=>string.IsNullOrWhiteSpace(v)?null:v.Trim();
}
public static class EmployeeDocumentErrors
{
 public static readonly Error InvalidIdentifier=Error.Validation("Workforce.EmployeeDocument.InvalidIdentifier","errors.workforce.employeeDocument.invalidIdentifier");
 public static readonly Error InvalidMetadata=Error.Validation("Workforce.EmployeeDocument.InvalidMetadata","errors.workforce.employeeDocument.invalidMetadata");
 public static readonly Error InvalidValidityPeriod=Error.Validation("Workforce.EmployeeDocument.InvalidValidityPeriod","errors.workforce.employeeDocument.invalidValidityPeriod");
 public static readonly Error InvalidTransition=Error.Conflict("Workforce.EmployeeDocument.InvalidTransition","errors.workforce.employeeDocument.invalidTransition");
 public static readonly Error NotEditable=Error.Conflict("Workforce.EmployeeDocument.NotEditable","errors.workforce.employeeDocument.notEditable");
 public static readonly Error DuplicateDocumentReference=Error.Conflict("Workforce.EmployeeDocument.DuplicateDocumentReference","errors.workforce.employeeDocument.duplicateDocumentReference");
 public static readonly Error RevocationReasonRequired=Error.Validation("Workforce.EmployeeDocument.RevocationReasonRequired","errors.workforce.employeeDocument.revocationReasonRequired");
 public static readonly Error NotFound=Error.NotFound("Workforce.EmployeeDocument.NotFound","errors.workforce.employeeDocument.notFound");
}
public interface IEmployeeDocumentRepository
{
 Task<EmployeeDocument?> GetAsync(OrganizationId organizationId,EmployeeDocumentId id,bool tracking,CancellationToken ct=default);
 Task<IReadOnlyList<EmployeeDocument>> ListAsync(OrganizationId organizationId,EmployeeId? employeeId,EmployeeDocumentCategory? category,EmployeeDocumentStatus? status,CancellationToken ct=default);
 Task<bool> DocumentReferenceExistsAsync(OrganizationId organizationId,Guid documentReferenceId,EmployeeDocumentId? excluding,CancellationToken ct=default);
 void Add(EmployeeDocument document);
}
