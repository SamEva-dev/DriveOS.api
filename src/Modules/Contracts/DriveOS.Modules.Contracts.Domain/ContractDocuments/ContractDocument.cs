using DriveOS.Modules.Contracts.Domain.ContractDocuments.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Contracts.Domain.ContractDocuments;
public sealed class ContractDocument : AggregateRoot<ContractDocumentId>, IAuditableEntity
{
    private readonly List<ContractDocumentVersion> _versions=[];
    private ContractDocument() { }
    private ContractDocument(ContractDocumentId id, OrganizationId org, TrainingContractId contractId, int contractVersion, ContractDocumentType type, string title, ContractDocumentVisibility visibility, DateOnly? retainUntil, string? legalBasis):base(id)
    { OrganizationId=org; ContractId=contractId; ContractVersionNumber=contractVersion; DocumentType=type; Title=title; Visibility=visibility; RetainUntil=retainUntil; RetentionLegalBasis=legalBasis; Status=ContractDocumentStatus.Active; }
    public OrganizationId OrganizationId {get;private set;}
    public TrainingContractId ContractId {get;private set;}
    public int ContractVersionNumber {get;private set;}
    public ContractDocumentType DocumentType {get;private set;}
    public string Title {get;private set;}=string.Empty;
    public ContractDocumentVisibility Visibility {get;private set;}
    public DateOnly? RetainUntil {get;private set;}
    public string? RetentionLegalBasis {get;private set;}
    public ContractDocumentStatus Status {get;private set;}
    public int CurrentVersionNumber {get;private set;}
    public DateTimeOffset? ArchivedAtUtc {get;private set;}
    public UserId? ArchivedByUserId {get;private set;}
    public IReadOnlyCollection<ContractDocumentVersion> Versions=>_versions.AsReadOnly();
    public DateTimeOffset CreatedAtUtc {get;private set;}
    public UserId? CreatedByUserId {get;private set;}
    public DateTimeOffset? LastModifiedAtUtc {get;private set;}
    public UserId? LastModifiedByUserId {get;private set;}
    public static Result<ContractDocument> Create(ContractDocumentId id, OrganizationId org, TrainingContractId contractId, int contractVersion, ContractDocumentType type, string title, ContractDocumentVisibility visibility, DateOnly? retainUntil, string? legalBasis, ContractDocumentVersion version)
    {
        var t=title?.Trim()??string.Empty; var basis=string.IsNullOrWhiteSpace(legalBasis)?null:legalBasis.Trim();
        if(id.IsEmpty||org.IsEmpty||contractId.IsEmpty||contractVersion<1||!Enum.IsDefined(type)||!Enum.IsDefined(visibility)||t.Length is <2 or >200||basis?.Length>300||version.DocumentId!=id||version.VersionNumber!=1) return Result.Failure<ContractDocument>(ContractDocumentErrors.Invalid);
        var d=new ContractDocument(id,org,contractId,contractVersion,type,t,visibility,retainUntil,basis); d._versions.Add(version); d.CurrentVersionNumber=1;
        d.RaiseDomainEvent(new ContractDocumentCreatedDomainEvent(id,contractId,org,type,1)); return Result.Success(d);
    }
    public Result AddVersion(ContractDocumentVersion version)
    { if(Status==ContractDocumentStatus.Archived)return Result.Failure(ContractDocumentErrors.Archived); if(version.DocumentId!=Id||version.VersionNumber!=CurrentVersionNumber+1)return Result.Failure(ContractDocumentErrors.InvalidFile); _versions.Add(version); CurrentVersionNumber=version.VersionNumber; RaiseDomainEvent(new ContractDocumentVersionAddedDomainEvent(Id,ContractId,CurrentVersionNumber)); return Result.Success(); }
    public Result Archive(UserId actor, DateTimeOffset at)
    { if(Status==ContractDocumentStatus.Archived)return Result.Failure(ContractDocumentErrors.AlreadyArchived); if(actor.IsEmpty||at==default)return Result.Failure(ContractDocumentErrors.Invalid); Status=ContractDocumentStatus.Archived; ArchivedByUserId=actor; ArchivedAtUtc=at.ToUniversalTime(); RaiseDomainEvent(new ContractDocumentArchivedDomainEvent(Id,ContractId,actor,ArchivedAtUtc.Value)); return Result.Success(); }
    public void SetCreatedAudit(DateTimeOffset at, UserId? by){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=by;}
    public void SetModifiedAudit(DateTimeOffset at, UserId? by){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=by;}
}
