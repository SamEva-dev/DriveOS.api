using DriveOS.Modules.Workforce.Domain.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.LeavePolicies;

public enum LeaveCategory { PaidLeave, UnpaidLeave, SickLeave, TrainingLeave, ParentalLeave, CompensatoryLeave, Other }
public enum LeavePolicyStatus { Active, Inactive }

/// <summary>Organization/country leave type policy. It configures workflow defaults but does not encode national law.</summary>
public sealed class LeavePolicy : AggregateRoot<LeavePolicyId>, IAuditableEntity
{
    private LeavePolicy() { }
    private LeavePolicy(LeavePolicyId id, OrganizationId organizationId, string countryCode, string code, string name, LeaveCategory category, bool isPaid, bool requiresApproval, bool requiresEvidence, bool allowHalfDay, int? minimumNoticeDays, int? maximumConsecutiveDays, DateTimeOffset nowUtc) : base(id)
    {
        OrganizationId=organizationId; CountryCode=NormalizeCountry(countryCode); Code=NormalizeCode(code); Name=name.Trim(); Category=category;
        IsPaid=isPaid; RequiresApproval=requiresApproval; RequiresEvidence=requiresEvidence; AllowHalfDay=allowHalfDay; MinimumNoticeDays=minimumNoticeDays; MaximumConsecutiveDays=maximumConsecutiveDays; Status=LeavePolicyStatus.Active;
        RaiseDomainEvent(new LeavePolicyCreatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, OrganizationId, CountryCode, Code, Category));
    }
    public OrganizationId OrganizationId { get; private set; }
    public string CountryCode { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public LeaveCategory Category { get; private set; }
    public bool IsPaid { get; private set; }
    public bool RequiresApproval { get; private set; }
    public bool RequiresEvidence { get; private set; }
    public bool AllowHalfDay { get; private set; }
    public int? MinimumNoticeDays { get; private set; }
    public int? MaximumConsecutiveDays { get; private set; }
    public LeavePolicyStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<LeavePolicy> Create(LeavePolicyId id, OrganizationId organizationId, string countryCode, string code, string name, LeaveCategory category, bool isPaid, bool requiresApproval, bool requiresEvidence, bool allowHalfDay, int? minimumNoticeDays, int? maximumConsecutiveDays, DateTimeOffset nowUtc)
    { var e=Validate(id,organizationId,countryCode,code,name,minimumNoticeDays,maximumConsecutiveDays); return e is null?Result.Success(new LeavePolicy(id,organizationId,countryCode,code,name,category,isPaid,requiresApproval,requiresEvidence,allowHalfDay,minimumNoticeDays,maximumConsecutiveDays,nowUtc)):Result.Failure<LeavePolicy>(e); }
    public Result Update(string countryCode,string code,string name,LeaveCategory category,bool isPaid,bool requiresApproval,bool requiresEvidence,bool allowHalfDay,int? minimumNoticeDays,int? maximumConsecutiveDays,DateTimeOffset nowUtc,UserId actor)
    { var e=Validate(Id,OrganizationId,countryCode,code,name,minimumNoticeDays,maximumConsecutiveDays); if(e is not null)return Result.Failure(e); CountryCode=NormalizeCountry(countryCode);Code=NormalizeCode(code);Name=name.Trim();Category=category;IsPaid=isPaid;RequiresApproval=requiresApproval;RequiresEvidence=requiresEvidence;AllowHalfDay=allowHalfDay;MinimumNoticeDays=minimumNoticeDays;MaximumConsecutiveDays=maximumConsecutiveDays;SetModifiedAudit(nowUtc,actor);RaiseDomainEvent(new LeavePolicyUpdatedDomainEvent(Guid.NewGuid(),nowUtc.ToUniversalTime(),Id,OrganizationId,CountryCode,Code,Category,actor));return Result.Success(); }
    public Result Deactivate(DateTimeOffset nowUtc,UserId actor){if(Status==LeavePolicyStatus.Inactive)return Result.Failure(LeavePolicyErrors.AlreadyInactive);Status=LeavePolicyStatus.Inactive;SetModifiedAudit(nowUtc,actor);RaiseDomainEvent(new LeavePolicyDeactivatedDomainEvent(Guid.NewGuid(),nowUtc.ToUniversalTime(),Id,OrganizationId,actor));return Result.Success();}
    public Result Reactivate(DateTimeOffset nowUtc,UserId actor){if(Status==LeavePolicyStatus.Active)return Result.Failure(LeavePolicyErrors.AlreadyActive);Status=LeavePolicyStatus.Active;SetModifiedAudit(nowUtc,actor);RaiseDomainEvent(new LeavePolicyReactivatedDomainEvent(Guid.NewGuid(),nowUtc.ToUniversalTime(),Id,OrganizationId,actor));return Result.Success();}
    private static Error? Validate(LeavePolicyId id,OrganizationId org,string country,string code,string name,int? min,int? max){if(id.IsEmpty)return LeavePolicyErrors.InvalidIdentifier;if(org.IsEmpty)return LeavePolicyErrors.InvalidOrganization;if(string.IsNullOrWhiteSpace(country)||country.Trim().Length!=2)return LeavePolicyErrors.InvalidCountryCode;if(string.IsNullOrWhiteSpace(code))return LeavePolicyErrors.CodeRequired;if(code.Trim().Length>64)return LeavePolicyErrors.CodeTooLong;if(string.IsNullOrWhiteSpace(name))return LeavePolicyErrors.NameRequired;if(name.Trim().Length>160)return LeavePolicyErrors.NameTooLong;if(min<0||max<=0)return LeavePolicyErrors.InvalidRuleValue;return null;}
    public void SetCreatedAudit(DateTimeOffset at,UserId? by){if(CreatedAtUtc!=default)return;CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=by;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? by){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=by;}
    private static string NormalizeCountry(string value)=>value.Trim().ToUpperInvariant();
    private static string NormalizeCode(string value)=>value.Trim().ToUpperInvariant();
}
