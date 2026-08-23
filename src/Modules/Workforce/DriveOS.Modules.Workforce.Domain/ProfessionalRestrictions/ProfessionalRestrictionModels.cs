using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Workforce.Domain.ProfessionalRestrictions;

public enum ProfessionalRestrictionActivity { AllProfessionalDuties=1, Teaching=2, ExamDuties=3, VehicleOperation=4 }
public enum ProfessionalRestrictionStatus { Planned=1, Active=2, Lifted=3, Cancelled=4 }
public enum ProfessionalRestrictionSource { InternalDecision=1, RegulatoryAuthority=2, OccupationalHealth=3, QualificationIssue=4, Other=99 }

/// <summary>
/// Time-bounded professional restriction attached to an employee. It limits professional eligibility without
/// changing the employment relationship or authentication account. Optional country/category/branch scopes
/// allow a targeted restriction such as "teaching category B in France" while preserving unrelated duties.
/// </summary>
public sealed class ProfessionalRestriction : AggregateRoot<ProfessionalRestrictionId>, IAuditableEntity
{
    private ProfessionalRestriction() { }
    private ProfessionalRestriction(ProfessionalRestrictionId id, OrganizationId organizationId, EmployeeId employeeId,
        ProfessionalRestrictionActivity activity, ProfessionalRestrictionSource source, DateOnly startDate, DateOnly? endDate,
        string reason, string? countryCode, string? licenseCategoryCode, BranchId? branchId, Guid? supportingDocumentReferenceId)
        : base(id)
    {
        OrganizationId=organizationId; EmployeeId=employeeId; Activity=activity; Source=source; StartDate=startDate; EndDate=endDate;
        Reason=reason; CountryCode=N(countryCode)?.ToUpperInvariant(); LicenseCategoryCode=N(licenseCategoryCode)?.ToUpperInvariant();
        BranchId=branchId; SupportingDocumentReferenceId=supportingDocumentReferenceId; Status=ProfessionalRestrictionStatus.Planned;
    }
    public OrganizationId OrganizationId{get;private set;} public EmployeeId EmployeeId{get;private set;}
    public ProfessionalRestrictionActivity Activity{get;private set;} public ProfessionalRestrictionSource Source{get;private set;}
    public DateOnly StartDate{get;private set;} public DateOnly? EndDate{get;private set;} public string Reason{get;private set;}=string.Empty;
    public string? CountryCode{get;private set;} public string? LicenseCategoryCode{get;private set;} public BranchId? BranchId{get;private set;}
    public Guid? SupportingDocumentReferenceId{get;private set;} public ProfessionalRestrictionStatus Status{get;private set;}
    public DateTimeOffset? ActivatedAtUtc{get;private set;} public UserId? ActivatedByUserId{get;private set;}
    public DateTimeOffset? LiftedAtUtc{get;private set;} public UserId? LiftedByUserId{get;private set;} public string? LiftReason{get;private set;}
    public DateTimeOffset? CancelledAtUtc{get;private set;} public UserId? CancelledByUserId{get;private set;} public string? CancellationReason{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;} public UserId? CreatedByUserId{get;private set;} public DateTimeOffset? LastModifiedAtUtc{get;private set;} public UserId? LastModifiedByUserId{get;private set;}

    public static Result<ProfessionalRestriction> Create(ProfessionalRestrictionId id,OrganizationId org,EmployeeId employee,
        ProfessionalRestrictionActivity activity,ProfessionalRestrictionSource source,DateOnly startDate,DateOnly? endDate,string reason,
        string? countryCode,string? licenseCategoryCode,BranchId? branchId,Guid? supportingDocumentReferenceId,DateTimeOffset now,UserId actor)
    {
        if(id.IsEmpty||org.IsEmpty||employee.IsEmpty)return Result.Failure<ProfessionalRestriction>(ProfessionalRestrictionErrors.InvalidIdentifier);
        if(endDate is DateOnly e&&e<startDate)return Result.Failure<ProfessionalRestriction>(ProfessionalRestrictionErrors.InvalidPeriod);
        reason=(reason??string.Empty).Trim(); if(reason.Length is <1 or >1000)return Result.Failure<ProfessionalRestriction>(ProfessionalRestrictionErrors.ReasonRequired);
        if((countryCode?.Trim().Length??0)>8||(licenseCategoryCode?.Trim().Length??0)>32)return Result.Failure<ProfessionalRestriction>(ProfessionalRestrictionErrors.InvalidScope);
        var x=new ProfessionalRestriction(id,org,employee,activity,source,startDate,endDate,reason,countryCode,licenseCategoryCode,branchId,supportingDocumentReferenceId);
        x.SetCreatedAudit(now,actor); return Result.Success(x);
    }
    public Result UpdatePlan(DateOnly startDate,DateOnly? endDate,string reason,string? countryCode,string? licenseCategoryCode,BranchId? branchId,Guid? documentReferenceId,DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalRestrictionStatus.Planned)return Result.Failure(ProfessionalRestrictionErrors.NotEditable);
        if(endDate is DateOnly e&&e<startDate)return Result.Failure(ProfessionalRestrictionErrors.InvalidPeriod);
        reason=(reason??string.Empty).Trim(); if(reason.Length is <1 or >1000)return Result.Failure(ProfessionalRestrictionErrors.ReasonRequired);
        StartDate=startDate;EndDate=endDate;Reason=reason;CountryCode=N(countryCode)?.ToUpperInvariant();LicenseCategoryCode=N(licenseCategoryCode)?.ToUpperInvariant();BranchId=branchId;SupportingDocumentReferenceId=documentReferenceId;SetModifiedAudit(now,actor);return Result.Success();
    }
    public Result Activate(DateTimeOffset now,UserId actor){if(Status!=ProfessionalRestrictionStatus.Planned)return Result.Failure(ProfessionalRestrictionErrors.InvalidTransition);Status=ProfessionalRestrictionStatus.Active;ActivatedAtUtc=now.ToUniversalTime();ActivatedByUserId=actor;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Lift(string reason,DateTimeOffset now,UserId actor){if(Status!=ProfessionalRestrictionStatus.Active)return Result.Failure(ProfessionalRestrictionErrors.InvalidTransition);reason=(reason??string.Empty).Trim();if(reason.Length is <1 or >1000)return Result.Failure(ProfessionalRestrictionErrors.LiftReasonRequired);Status=ProfessionalRestrictionStatus.Lifted;LiftReason=reason;LiftedAtUtc=now.ToUniversalTime();LiftedByUserId=actor;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Cancel(string reason,DateTimeOffset now,UserId actor){if(Status!=ProfessionalRestrictionStatus.Planned)return Result.Failure(ProfessionalRestrictionErrors.InvalidTransition);reason=(reason??string.Empty).Trim();if(reason.Length is <1 or >1000)return Result.Failure(ProfessionalRestrictionErrors.CancellationReasonRequired);Status=ProfessionalRestrictionStatus.Cancelled;CancellationReason=reason;CancelledAtUtc=now.ToUniversalTime();CancelledByUserId=actor;SetModifiedAudit(now,actor);return Result.Success();}
    public bool AppliesOn(DateOnly date)=>Status==ProfessionalRestrictionStatus.Active&&StartDate<=date&&(!EndDate.HasValue||EndDate.Value>=date);
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;} public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string? N(string? v)=>string.IsNullOrWhiteSpace(v)?null:v.Trim();
}

public static class ProfessionalRestrictionErrors
{
    public static readonly Error InvalidIdentifier=Error.Validation("Workforce.ProfessionalRestriction.InvalidIdentifier","errors.workforce.professionalRestriction.invalidIdentifier");
    public static readonly Error InvalidPeriod=Error.Validation("Workforce.ProfessionalRestriction.InvalidPeriod","errors.workforce.professionalRestriction.invalidPeriod");
    public static readonly Error ReasonRequired=Error.Validation("Workforce.ProfessionalRestriction.ReasonRequired","errors.workforce.professionalRestriction.reasonRequired");
    public static readonly Error LiftReasonRequired=Error.Validation("Workforce.ProfessionalRestriction.LiftReasonRequired","errors.workforce.professionalRestriction.liftReasonRequired");
    public static readonly Error CancellationReasonRequired=Error.Validation("Workforce.ProfessionalRestriction.CancellationReasonRequired","errors.workforce.professionalRestriction.cancellationReasonRequired");
    public static readonly Error InvalidScope=Error.Validation("Workforce.ProfessionalRestriction.InvalidScope","errors.workforce.professionalRestriction.invalidScope");
    public static readonly Error InvalidTransition=Error.Conflict("Workforce.ProfessionalRestriction.InvalidTransition","errors.workforce.professionalRestriction.invalidTransition");
    public static readonly Error NotEditable=Error.Conflict("Workforce.ProfessionalRestriction.NotEditable","errors.workforce.professionalRestriction.notEditable");
    public static readonly Error Overlap=Error.Conflict("Workforce.ProfessionalRestriction.Overlap","errors.workforce.professionalRestriction.overlap");
    public static readonly Error NotFound=Error.NotFound("Workforce.ProfessionalRestriction.NotFound","errors.workforce.professionalRestriction.notFound");
}

public interface IProfessionalRestrictionRepository
{
    Task<ProfessionalRestriction?> GetAsync(OrganizationId organizationId,ProfessionalRestrictionId id,bool tracking,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalRestriction>> ListAsync(OrganizationId organizationId,EmployeeId? employeeId,ProfessionalRestrictionStatus? status,ProfessionalRestrictionActivity? activity,CancellationToken ct=default);
    Task<bool> HasOverlapAsync(OrganizationId organizationId,EmployeeId employeeId,ProfessionalRestrictionActivity activity,DateOnly from,DateOnly? to,string? countryCode,string? licenseCategoryCode,BranchId? branchId,ProfessionalRestrictionId? excluding,CancellationToken ct=default);
    void Add(ProfessionalRestriction restriction);
}
