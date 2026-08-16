using DriveOS.Modules.Students.Domain.Students;
using DriveOS.Modules.Students.Domain.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Enrollments;

public sealed class Enrollment : AggregateRoot<DraftEnrollmentId>, IAuditableEntity
{
    private Enrollment() { }

    private Enrollment(DraftEnrollmentId id, OrganizationId organizationId, PersonId studentId,
        BranchId branchId, LeadId? sourceLeadId, string trainingCode,
        EnrollmentSource source, string? idempotencyKey, string? regulatoryCountryCode,
        string? preferredLanguageCode, bool? requiredConsentsAccepted) : base(id)
    {
        OrganizationId = organizationId;
        StudentId = studentId;
        BranchId = branchId;
        SourceLeadId = sourceLeadId;
        TrainingCode = trainingCode;
        Source = source;
        IdempotencyKey = idempotencyKey;
        RegulatoryCountryCode = regulatoryCountryCode;
        PreferredLanguageCode = preferredLanguageCode;
        RequiredConsentsAccepted = requiredConsentsAccepted;
        Status = EnrollmentStatus.Draft;
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public BranchId BranchId { get; private set; }
    public LeadId? SourceLeadId { get; private set; }
    public string TrainingCode { get; private set; } = string.Empty;
    public EnrollmentSource Source { get; private set; }
    public string? IdempotencyKey { get; private set; }
    public string? RegulatoryCountryCode { get; private set; }
    public string? PreferredLanguageCode { get; private set; }
    public bool? RequiredConsentsAccepted { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Enrollment> CreateDraft(DraftEnrollmentId id,
        OrganizationId organizationId, PersonId studentId, BranchId branchId,
        LeadId? sourceLeadId, string trainingCode)
    {
        if (id.IsEmpty) return Result.Failure<Enrollment>(EnrollmentErrors.InvalidId);
        if (organizationId.IsEmpty || studentId.IsEmpty || branchId.IsEmpty || sourceLeadId is { IsEmpty: true })
            return Result.Failure<Enrollment>(EnrollmentErrors.InvalidOwner);
        string code = trainingCode?.Trim() ?? string.Empty;
        if (code.Length == 0) return Result.Failure<Enrollment>(EnrollmentErrors.TrainingCodeRequired);
        if (code.Length > 100) return Result.Failure<Enrollment>(EnrollmentErrors.TrainingCodeTooLong);
        var enrollment = new Enrollment(id, organizationId, studentId, branchId, sourceLeadId,
            code, EnrollmentSource.CrmConversion, null, null, null, null);
        enrollment.RaiseDomainEvent(new EnrollmentCreatedDomainEvent(enrollment.Id, studentId, organizationId));
        return Result.Success(enrollment);
    }

    public static Result<Enrollment> CreateDirectDraft(DraftEnrollmentId id,
        OrganizationId organizationId, PersonId studentId, BranchId branchId,
        string trainingCode, EnrollmentSource source, string idempotencyKey,
        string regulatoryCountryCode, string preferredLanguageCode,
        bool requiredConsentsAccepted)
    {
        if (source == EnrollmentSource.CrmConversion)
            return Result.Failure<Enrollment>(EnrollmentErrors.InvalidDirectSource);
        string key = idempotencyKey?.Trim() ?? string.Empty;
        string country = regulatoryCountryCode?.Trim().ToUpperInvariant() ?? string.Empty;
        string language = preferredLanguageCode?.Trim().ToLowerInvariant() ?? string.Empty;
        if (key.Length is < 8 or > 100)
            return Result.Failure<Enrollment>(EnrollmentErrors.InvalidIdempotencyKey);
        if (country.Length is < 2 or > 3 || language.Length is < 2 or > 10)
            return Result.Failure<Enrollment>(EnrollmentErrors.InvalidLocale);
        if (!requiredConsentsAccepted)
            return Result.Failure<Enrollment>(EnrollmentErrors.RequiredConsentsMissing);
        Result<Enrollment> draft = CreateDraft(id, organizationId, studentId, branchId, null, trainingCode);
        if (draft.IsFailure) return draft;
        Enrollment enrollment = draft.Value;
        enrollment.Source = source;
        enrollment.IdempotencyKey = key;
        enrollment.RegulatoryCountryCode = country;
        enrollment.PreferredLanguageCode = language;
        enrollment.RequiredConsentsAccepted = true;
        return Result.Success(enrollment);
    }

    public void SetCreatedAudit(DateTimeOffset at, UserId? by)
    { if (CreatedAtUtc == default) { CreatedAtUtc = at; CreatedByUserId = by; } }
    public void SetModifiedAudit(DateTimeOffset at, UserId? by)
    { LastModifiedAtUtc = at; LastModifiedByUserId = by; }
    public Result Activate(UserId actor,DateTimeOffset now)
    {
        if(Status==EnrollmentStatus.Active)return Result.Failure(DriveOS.Modules.Students.Domain.Checklists.EnrollmentChecklistErrors.AlreadyActive);
        if(Status is EnrollmentStatus.Cancelled or EnrollmentStatus.Suspended)return Result.Failure(EnrollmentErrors.InvalidStatusTransition);
        Status=EnrollmentStatus.Active;SetModifiedAudit(now,actor);
        RaiseDomainEvent(new EnrollmentStatusChangedDomainEvent(Id,StudentId,OrganizationId,Status.ToString()));
        return Result.Success();
    }

    public Result TransferToBranch(BranchId targetBranchId, UserId actor, DateTimeOffset now)
    {
        if (targetBranchId.IsEmpty)
            return Result.Failure(EnrollmentErrors.InvalidOwner);
        BranchId = targetBranchId;
        SetModifiedAudit(now, actor);
        RaiseDomainEvent(new EnrollmentBranchChangedDomainEvent(Id, StudentId, OrganizationId, BranchId));
        return Result.Success();
    }

    public Result Suspend(UserId actor, DateTimeOffset now)
    {
        if (Status != EnrollmentStatus.Active)
            return Result.Failure(EnrollmentErrors.InvalidStatusTransition);
        Status = EnrollmentStatus.Suspended;
        SetModifiedAudit(now, actor);
        RaiseDomainEvent(new EnrollmentStatusChangedDomainEvent(Id, StudentId, OrganizationId, Status.ToString()));
        return Result.Success();
    }

    public Result Reactivate(UserId actor, DateTimeOffset now)
    {
        if (Status != EnrollmentStatus.Suspended)
            return Result.Failure(EnrollmentErrors.InvalidStatusTransition);
        Status = EnrollmentStatus.Active;
        SetModifiedAudit(now, actor);
        RaiseDomainEvent(new EnrollmentStatusChangedDomainEvent(Id, StudentId, OrganizationId, Status.ToString()));
        return Result.Success();
    }

    public Result Close(UserId actor, DateTimeOffset now)
    {
        if (Status is not (EnrollmentStatus.Active or EnrollmentStatus.Suspended))
            return Result.Failure(EnrollmentErrors.InvalidStatusTransition);
        Status = EnrollmentStatus.Closed;
        SetModifiedAudit(now, actor);
        RaiseDomainEvent(new EnrollmentStatusChangedDomainEvent(Id, StudentId, OrganizationId, Status.ToString()));
        return Result.Success();
    }

    public Result ReopenAsActive(UserId actor, DateTimeOffset now)
    {
        if (Status != EnrollmentStatus.Closed) return Result.Failure(EnrollmentErrors.InvalidStatusTransition);
        Status = EnrollmentStatus.Active; SetModifiedAudit(now, actor);
        RaiseDomainEvent(new EnrollmentStatusChangedDomainEvent(Id, StudentId, OrganizationId, Status.ToString()));
        return Result.Success();
    }

    public Result ReopenAsSuspended(UserId actor, DateTimeOffset now)
    {
        if (Status != EnrollmentStatus.Closed) return Result.Failure(EnrollmentErrors.InvalidStatusTransition);
        Status = EnrollmentStatus.Suspended; SetModifiedAudit(now, actor);
        RaiseDomainEvent(new EnrollmentStatusChangedDomainEvent(Id, StudentId, OrganizationId, Status.ToString()));
        return Result.Success();
    }
}
