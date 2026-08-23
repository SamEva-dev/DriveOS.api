using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Workforce.Domain.Qualifications;

public enum EmployeeQualificationStatus { Declared, Verified, Rejected, Expired, Superseded }
public enum QualificationSource { Manual, Import, ExternalProvider }

/// <summary>Generic professional qualification owned by Workforce. It never grants an application permission.</summary>
public sealed class EmployeeQualification
{
    private EmployeeQualification() { }
    private EmployeeQualification(EmployeeQualificationId id, string countryCode, string qualificationType, string title, string? identifier,
        string? issuingAuthority, DateOnly? issuedOn, DateOnly? expiresOn, QualificationSource source, DateTimeOffset nowUtc, UserId actor)
    { Id=id; CountryCode=NormalizeToken(countryCode); QualificationType=NormalizeToken(qualificationType); Title=title.Trim(); Identifier=NormalizeOptional(identifier,120); IssuingAuthority=NormalizeOptional(issuingAuthority,160); IssuedOn=issuedOn; ExpiresOn=expiresOn; Source=source; Status=EmployeeQualificationStatus.Declared; DeclaredAtUtc=nowUtc.ToUniversalTime(); DeclaredByUserId=actor; }
    public EmployeeQualificationId Id { get; private set; }
    public string CountryCode { get; private set; } = string.Empty;
    public string QualificationType { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Identifier { get; private set; }
    public string? IssuingAuthority { get; private set; }
    public DateOnly? IssuedOn { get; private set; }
    public DateOnly? ExpiresOn { get; private set; }
    public QualificationSource Source { get; private set; }
    public EmployeeQualificationStatus Status { get; private set; }
    public DateTimeOffset DeclaredAtUtc { get; private set; }
    public UserId DeclaredByUserId { get; private set; }
    public DateTimeOffset? VerifiedAtUtc { get; private set; }
    public UserId? VerifiedByUserId { get; private set; }
    public string? VerificationMethod { get; private set; }
    public string? DecisionReason { get; private set; }
    public EmployeeQualificationId? SupersededById { get; private set; }

    public static Result<EmployeeQualification> Declare(EmployeeQualificationId id,string countryCode,string qualificationType,string title,string? identifier,string? issuingAuthority,DateOnly? issuedOn,DateOnly? expiresOn,QualificationSource source,DateTimeOffset nowUtc,UserId actor)
    {
        if(id.IsEmpty || actor.IsEmpty) return Result.Failure<EmployeeQualification>(QualificationErrors.InvalidOwner);
        string country=NormalizeToken(countryCode); string type=NormalizeToken(qualificationType); string name=(title??string.Empty).Trim();
        if(country.Length!=2 || type.Length is <2 or >64 || name.Length is <2 or >160) return Result.Failure<EmployeeQualification>(QualificationErrors.InvalidQualification);
        if(issuedOn.HasValue && expiresOn.HasValue && expiresOn.Value<issuedOn.Value) return Result.Failure<EmployeeQualification>(QualificationErrors.InvalidValidityPeriod);
        return Result.Success(new EmployeeQualification(id,country,type,name,identifier,issuingAuthority,issuedOn,expiresOn,source,nowUtc,actor));
    }
    public Result Verify(string method,string? reason,DateTimeOffset nowUtc,UserId actor){ if(Status is EmployeeQualificationStatus.Superseded or EmployeeQualificationStatus.Rejected)return Result.Failure(QualificationErrors.NotCurrent); if(string.IsNullOrWhiteSpace(method))return Result.Failure(QualificationErrors.VerificationMethodRequired); Status=EmployeeQualificationStatus.Verified; VerificationMethod=method.Trim(); DecisionReason=NormalizeOptional(reason,500); VerifiedAtUtc=nowUtc.ToUniversalTime(); VerifiedByUserId=actor; return Result.Success(); }
    public Result Reject(string reason,DateTimeOffset nowUtc,UserId actor){ if(Status is EmployeeQualificationStatus.Superseded or EmployeeQualificationStatus.Rejected)return Result.Failure(QualificationErrors.NotCurrent); if(string.IsNullOrWhiteSpace(reason))return Result.Failure(QualificationErrors.DecisionReasonRequired); Status=EmployeeQualificationStatus.Rejected; DecisionReason=reason.Trim(); VerifiedAtUtc=nowUtc.ToUniversalTime(); VerifiedByUserId=actor; return Result.Success(); }
    public void Supersede(EmployeeQualificationId byId){ Status=EmployeeQualificationStatus.Superseded; SupersededById=byId; }
    public static string NormalizeToken(string value)=>(value??string.Empty).Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value,int max){ if(string.IsNullOrWhiteSpace(value))return null; string x=value.Trim(); return x.Length<=max?x:x[..max]; }
}

/// <summary>Regulatory authorization to teach a scoped driving category in one country/jurisdiction.</summary>
public sealed class InstructorAuthorization
{
    private InstructorAuthorization() { }
    private InstructorAuthorization(InstructorAuthorizationId id,string countryCode,string authorizationType,string identifier,string issuingAuthority,string? jurisdictionCode,string licenseCategoryCode,DateOnly? issuedOn,DateOnly? expiresOn,QualificationSource source,DateTimeOffset nowUtc,UserId actor)
    { Id=id; CountryCode=EmployeeQualification.NormalizeToken(countryCode); AuthorizationType=EmployeeQualification.NormalizeToken(authorizationType); Identifier=identifier.Trim().ToUpperInvariant(); IssuingAuthority=issuingAuthority.Trim(); JurisdictionCode=string.IsNullOrWhiteSpace(jurisdictionCode)?null:jurisdictionCode.Trim().ToUpperInvariant(); LicenseCategoryCode=EmployeeQualification.NormalizeToken(licenseCategoryCode); IssuedOn=issuedOn; ExpiresOn=expiresOn; Source=source; Status=EmployeeQualificationStatus.Declared; DeclaredAtUtc=nowUtc.ToUniversalTime(); DeclaredByUserId=actor; }
    public InstructorAuthorizationId Id { get; private set; }
    public string CountryCode { get; private set; }=string.Empty;
    public string AuthorizationType { get; private set; }=string.Empty;
    public string Identifier { get; private set; }=string.Empty;
    public string IssuingAuthority { get; private set; }=string.Empty;
    public string? JurisdictionCode { get; private set; }
    public string LicenseCategoryCode { get; private set; }=string.Empty;
    public DateOnly? IssuedOn { get; private set; }
    public DateOnly? ExpiresOn { get; private set; }
    public QualificationSource Source { get; private set; }
    public EmployeeQualificationStatus Status { get; private set; }
    public DateTimeOffset DeclaredAtUtc { get; private set; }
    public UserId DeclaredByUserId { get; private set; }
    public DateTimeOffset? VerifiedAtUtc { get; private set; }
    public UserId? VerifiedByUserId { get; private set; }
    public string? VerificationMethod { get; private set; }
    public string? DecisionReason { get; private set; }
    public InstructorAuthorizationId? SupersededById { get; private set; }
    public bool IsVerifiedAt(DateOnly date)=>Status==EmployeeQualificationStatus.Verified && (!IssuedOn.HasValue||IssuedOn.Value<=date) && (!ExpiresOn.HasValue||ExpiresOn.Value>=date);
    public static Result<InstructorAuthorization> Declare(InstructorAuthorizationId id,string countryCode,string authorizationType,string identifier,string issuingAuthority,string? jurisdictionCode,string licenseCategoryCode,DateOnly? issuedOn,DateOnly? expiresOn,QualificationSource source,DateTimeOffset nowUtc,UserId actor)
    { string country=EmployeeQualification.NormalizeToken(countryCode), type=EmployeeQualification.NormalizeToken(authorizationType), category=EmployeeQualification.NormalizeToken(licenseCategoryCode); if(id.IsEmpty||actor.IsEmpty)return Result.Failure<InstructorAuthorization>(QualificationErrors.InvalidOwner); if(country.Length!=2||type.Length is <2 or >64||category.Length is <1 or >32||string.IsNullOrWhiteSpace(identifier)||identifier.Trim().Length>120||string.IsNullOrWhiteSpace(issuingAuthority)||issuingAuthority.Trim().Length>160)return Result.Failure<InstructorAuthorization>(QualificationErrors.InvalidAuthorization); if(issuedOn.HasValue&&expiresOn.HasValue&&expiresOn.Value<issuedOn.Value)return Result.Failure<InstructorAuthorization>(QualificationErrors.InvalidValidityPeriod); return Result.Success(new InstructorAuthorization(id,country,type,identifier,issuingAuthority,jurisdictionCode,category,issuedOn,expiresOn,source,nowUtc,actor)); }
    public Result Verify(string method,string? reason,DateTimeOffset nowUtc,UserId actor){if(Status is EmployeeQualificationStatus.Superseded or EmployeeQualificationStatus.Rejected)return Result.Failure(QualificationErrors.NotCurrent);if(string.IsNullOrWhiteSpace(method))return Result.Failure(QualificationErrors.VerificationMethodRequired);Status=EmployeeQualificationStatus.Verified;VerificationMethod=method.Trim();DecisionReason=string.IsNullOrWhiteSpace(reason)?null:reason.Trim();VerifiedAtUtc=nowUtc.ToUniversalTime();VerifiedByUserId=actor;return Result.Success();}
    public Result Reject(string reason,DateTimeOffset nowUtc,UserId actor){if(Status is EmployeeQualificationStatus.Superseded or EmployeeQualificationStatus.Rejected)return Result.Failure(QualificationErrors.NotCurrent);if(string.IsNullOrWhiteSpace(reason))return Result.Failure(QualificationErrors.DecisionReasonRequired);Status=EmployeeQualificationStatus.Rejected;DecisionReason=reason.Trim();VerifiedAtUtc=nowUtc.ToUniversalTime();VerifiedByUserId=actor;return Result.Success();}
    public void Supersede(InstructorAuthorizationId byId){Status=EmployeeQualificationStatus.Superseded;SupersededById=byId;}
}
