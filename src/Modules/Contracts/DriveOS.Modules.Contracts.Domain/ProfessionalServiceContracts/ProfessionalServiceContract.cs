using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Domain.ProfessionalServiceContracts;

/// <summary>
/// Contractual proof for a Marketplace professional engagement.
/// Marketplace owns the commercial relationship; Contracts owns document/version/signature truth.
/// Any document change after the first signature must create a new contract version.
/// </summary>
public sealed class ProfessionalServiceContract
    :AggregateRoot<ProfessionalServiceContractId>,IAuditableEntity
{
    private ProfessionalServiceContract(){}

    private ProfessionalServiceContract(
        ProfessionalServiceContractId id,
        OrganizationId organizationId,
        ProfessionalEngagementId engagementId,
        ProfessionalProfileId professionalProfileId,
        Guid providerOrganizationId,
        string contractNumber,
        string contractType,
        ProfessionalServiceContractSignatureOrder signatureOrder,
        string termsSnapshotJson,
        ProfessionalServiceContractSignatory[] signatories):base(id)
    {
        OrganizationId=organizationId;
        EngagementId=engagementId;
        ProfessionalProfileId=professionalProfileId;
        ProviderOrganizationId=providerOrganizationId;
        ContractNumber=contractNumber.Trim().ToUpperInvariant();
        ContractType=Token(contractType,80);
        SignatureOrder=signatureOrder;
        TermsSnapshotJson=termsSnapshotJson;
        Signatories=signatories.OrderBy(x=>x.SigningOrder).ToArray();
        Version=1;
        Status=ProfessionalServiceContractStatus.Draft;
    }

    public OrganizationId OrganizationId{get;private set;}
    public ProfessionalEngagementId EngagementId{get;private set;}
    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public Guid ProviderOrganizationId{get;private set;}
    public string ContractNumber{get;private set;}=string.Empty;
    public string ContractType{get;private set;}=string.Empty;
    public ProfessionalServiceContractSignatureOrder SignatureOrder{get;private set;}
    public string TermsSnapshotJson{get;private set;}="{}";
    public int Version{get;private set;}
    public ProfessionalServiceContractStatus Status{get;private set;}
    public ProfessionalServiceContractSignatory[] Signatories{get;private set;}=[];
    public ProfessionalServiceContractVersionSnapshot[] PreviousVersions{get;private set;}=[];

    public string? DocumentReference{get;private set;}
    public string? DocumentSha256{get;private set;}
    public DateTimeOffset? GeneratedAtUtc{get;private set;}
    public UserId? GeneratedByUserId{get;private set;}
    public DateTimeOffset? SentForSignatureAtUtc{get;private set;}
    public UserId? SentForSignatureByUserId{get;private set;}
    public DateTimeOffset? SignedAtUtc{get;private set;}
    public DateTimeOffset? TerminatedAtUtc{get;private set;}
    public UserId? TerminatedByUserId{get;private set;}
    public string? TerminationReason{get;private set;}

    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<ProfessionalServiceContract> Create(
        ProfessionalServiceContractId id,
        OrganizationId organizationId,
        ProfessionalEngagementId engagementId,
        ProfessionalProfileId professionalProfileId,
        Guid providerOrganizationId,
        string contractNumber,
        string contractType,
        ProfessionalServiceContractSignatureOrder signatureOrder,
        string termsSnapshotJson,
        IEnumerable<ProfessionalServiceContractSignatory> signatories,
        DateTimeOffset now,
        UserId actor)
    {
        ProfessionalServiceContractSignatory[] parties=(signatories??[]).ToArray();
        if(id.IsEmpty||organizationId.IsEmpty||engagementId.IsEmpty||professionalProfileId.IsEmpty||
           providerOrganizationId==Guid.Empty||string.IsNullOrWhiteSpace(contractNumber)||
           string.IsNullOrWhiteSpace(contractType)||string.IsNullOrWhiteSpace(termsSnapshotJson)||
           !Enum.IsDefined(signatureOrder)||parties.Length<2||
           parties.Any(x=>x.PersonId.IsEmpty||x.SigningOrder<1||string.IsNullOrWhiteSpace(x.Role)||x.Role.Trim().Length>80)||
           parties.Count(x=>x.IsRequired)<2||parties.Select(x=>x.PersonId).Distinct().Count()!=parties.Length)
            return Result.Failure<ProfessionalServiceContract>(ProfessionalServiceContractErrors.Invalid);

        var x=new ProfessionalServiceContract(
            id,organizationId,engagementId,professionalProfileId,providerOrganizationId,
            contractNumber,contractType,signatureOrder,termsSnapshotJson,parties);
        x.SetCreatedAudit(now,actor);
        return Result.Success(x);
    }

    public Result Generate(string documentReference,string documentSha256,DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalServiceContractStatus.Draft)
            return Result.Failure(ProfessionalServiceContractErrors.InvalidTransition);

        documentReference=(documentReference??string.Empty).Trim();
        documentSha256=(documentSha256??string.Empty).Trim().ToUpperInvariant();
        if(documentReference.Length<2||documentReference.Length>500||documentSha256.Length!=64||
           documentSha256.Any(c=>!Uri.IsHexDigit(c)))
            return Result.Failure(ProfessionalServiceContractErrors.InvalidDocument);

        DocumentReference=documentReference;
        DocumentSha256=documentSha256;
        Status=ProfessionalServiceContractStatus.Generated;
        GeneratedAtUtc=now.ToUniversalTime();
        GeneratedByUserId=actor;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result SendForSignature(DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalServiceContractStatus.Generated||string.IsNullOrWhiteSpace(DocumentSha256))
            return Result.Failure(ProfessionalServiceContractErrors.InvalidTransition);

        Status=ProfessionalServiceContractStatus.SentForSignature;
        SentForSignatureAtUtc=now.ToUniversalTime();
        SentForSignatureByUserId=actor;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result RecordSignature(
        PersonId personId,
        string documentSha256,
        string signatureMethod,
        string authenticationMethod,
        string provider,
        string providerReference,
        string? certificateReference,
        string? ipAddress,
        DateTimeOffset signedAtUtc,
        DateTimeOffset receivedAtUtc,
        UserId actor)
    {
        if(Status is not ProfessionalServiceContractStatus.SentForSignature and
           not ProfessionalServiceContractStatus.PartiallySigned)
            return Result.Failure(ProfessionalServiceContractErrors.InvalidTransition);

        if(!string.Equals(DocumentSha256,documentSha256?.Trim(),StringComparison.OrdinalIgnoreCase))
            return Result.Failure(ProfessionalServiceContractErrors.DocumentHashMismatch);

        int index=Array.FindIndex(Signatories,x=>x.PersonId==personId);
        if(index<0)return Result.Failure(ProfessionalServiceContractErrors.SignatoryNotFound);
        if(Signatories[index].SignedAtUtc is not null)return Result.Failure(ProfessionalServiceContractErrors.AlreadySigned);

        int nextOrder=Signatories
            .Where(x=>x.IsRequired&&x.SignedAtUtc is null)
            .Select(x=>x.SigningOrder)
            .DefaultIfEmpty(Signatories[index].SigningOrder)
            .Min();

        if(SignatureOrder==ProfessionalServiceContractSignatureOrder.Sequential&&
           Signatories[index].SigningOrder>nextOrder)
            return Result.Failure(ProfessionalServiceContractErrors.SigningOrderViolation);

        string method=Token(signatureMethod,80);
        string auth=Token(authenticationMethod,80);
        string providerToken=Token(provider,120);
        string providerRef=(providerReference??string.Empty).Trim();
        if(method.Length<1||auth.Length<1||providerToken.Length<1||providerRef.Length<1||providerRef.Length>250)
            return Result.Failure(ProfessionalServiceContractErrors.InvalidSignatureEvidence);

        Signatories[index]=Signatories[index] with
        {
            SignedAtUtc=signedAtUtc.ToUniversalTime(),
            ReceivedAtUtc=receivedAtUtc.ToUniversalTime(),
            SignatureMethod=method,
            AuthenticationMethod=auth,
            Provider=providerToken,
            ProviderReference=providerRef,
            CertificateReference=Optional(certificateReference,250),
            IpAddress=Optional(ipAddress,64)
        };

        bool allRequired=Signatories.Where(x=>x.IsRequired).All(x=>x.SignedAtUtc is not null);
        Status=allRequired?ProfessionalServiceContractStatus.Signed:ProfessionalServiceContractStatus.PartiallySigned;
        if(allRequired)SignedAtUtc=receivedAtUtc.ToUniversalTime();
        SetModifiedAudit(receivedAtUtc,actor);
        return Result.Success();
    }

    public Result CreateRevision(
        string documentReference,
        string documentSha256,
        string reason,
        DateTimeOffset now,
        UserId actor)
    {
        if(Status is not ProfessionalServiceContractStatus.PartiallySigned and
           not ProfessionalServiceContractStatus.Signed)
            return Result.Failure(ProfessionalServiceContractErrors.RevisionRequiresSignature);

        reason=(reason??string.Empty).Trim();
        if(reason.Length is <2 or >512)
            return Result.Failure(ProfessionalServiceContractErrors.ReasonRequired);

        PreviousVersions=
        [
            ..PreviousVersions,
            new ProfessionalServiceContractVersionSnapshot(
                Version,
                DocumentReference,
                DocumentSha256,
                Signatories,
                Status,
                GeneratedAtUtc,
                SentForSignatureAtUtc,
                SignedAtUtc,
                reason,
                now.ToUniversalTime(),
                actor)
        ];

        Version++;
        Signatories=Signatories.Select(x=>x with
        {
            SignedAtUtc=null,
            ReceivedAtUtc=null,
            SignatureMethod=null,
            AuthenticationMethod=null,
            Provider=null,
            ProviderReference=null,
            CertificateReference=null,
            IpAddress=null
        }).ToArray();

        DocumentReference=null;
        DocumentSha256=null;
        GeneratedAtUtc=null;
        GeneratedByUserId=null;
        SentForSignatureAtUtc=null;
        SentForSignatureByUserId=null;
        SignedAtUtc=null;
        Status=ProfessionalServiceContractStatus.Draft;

        Result generated=Generate(documentReference,documentSha256,now,actor);
        if(generated.IsFailure)return generated;

        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result Terminate(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status is ProfessionalServiceContractStatus.Terminated or ProfessionalServiceContractStatus.Cancelled)
            return Result.Failure(ProfessionalServiceContractErrors.InvalidTransition);
        reason=(reason??string.Empty).Trim();
        if(reason.Length is <2 or >512)return Result.Failure(ProfessionalServiceContractErrors.ReasonRequired);
        Status=ProfessionalServiceContractStatus.Terminated;
        TerminatedAtUtc=now.ToUniversalTime();
        TerminatedByUserId=actor;
        TerminationReason=reason;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string Token(string? value,int max){string x=(value??string.Empty).Trim().ToUpperInvariant();return x.Length<=max?x:x[..max];}
    private static string? Optional(string? value,int max){if(string.IsNullOrWhiteSpace(value))return null;string x=value.Trim();return x.Length<=max?x:x[..max];}
}

public sealed record ProfessionalServiceContractSignatory(
    PersonId PersonId,
    string Role,
    int SigningOrder,
    bool IsRequired,
    DateTimeOffset? SignedAtUtc=null,
    DateTimeOffset? ReceivedAtUtc=null,
    string? SignatureMethod=null,
    string? AuthenticationMethod=null,
    string? Provider=null,
    string? ProviderReference=null,
    string? CertificateReference=null,
    string? IpAddress=null);


public sealed record ProfessionalServiceContractVersionSnapshot(
    int Version,
    string? DocumentReference,
    string? DocumentSha256,
    ProfessionalServiceContractSignatory[] Signatories,
    ProfessionalServiceContractStatus Status,
    DateTimeOffset? GeneratedAtUtc,
    DateTimeOffset? SentForSignatureAtUtc,
    DateTimeOffset? SignedAtUtc,
    string RevisionReason,
    DateTimeOffset SupersededAtUtc,
    UserId SupersededByUserId);

public enum ProfessionalServiceContractStatus
{
    Draft=1,
    Generated=2,
    SentForSignature=3,
    PartiallySigned=4,
    Signed=5,
    Rejected=6,
    Expired=7,
    Cancelled=8,
    Terminated=9
}

public enum ProfessionalServiceContractSignatureOrder{Sequential=1,Parallel=2}
