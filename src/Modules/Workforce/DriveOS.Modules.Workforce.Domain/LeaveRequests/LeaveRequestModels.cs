using DriveOS.Modules.Workforce.Domain.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.LeaveRequests;

public enum LeaveRequestStatus { Draft, Submitted, Approved, Rejected, Cancelled }
public enum LeaveDayPortion { FullDay, Morning, Afternoon }

/// <summary>
/// Employee request for a period of leave. Policy rules are snapshotted when the request is created so later policy edits do not silently rewrite an existing request.
/// </summary>
public sealed class LeaveRequest : AggregateRoot<LeaveRequestId>, IAuditableEntity
{
    private LeaveRequest() { }
    private LeaveRequest(LeaveRequestId id, OrganizationId organizationId, EmployeeId employeeId, LeavePolicyId leavePolicyId,
        string policyCode, string countryCode, DateOnly startDate, DateOnly endDate, LeaveDayPortion startPortion,
        LeaveDayPortion endPortion, string? reason, DocumentId? evidenceDocumentId, bool requiresApproval,
        bool requiresEvidence, bool allowHalfDay, int? minimumNoticeDays, int? maximumConsecutiveDays,
        DateTimeOffset nowUtc) : base(id)
    {
        OrganizationId = organizationId; EmployeeId = employeeId; LeavePolicyId = leavePolicyId;
        PolicyCode = policyCode.Trim().ToUpperInvariant(); CountryCode = countryCode.Trim().ToUpperInvariant();
        StartDate = startDate; EndDate = endDate; StartPortion = startPortion; EndPortion = endPortion;
        Reason = NormalizeOptional(reason); EvidenceDocumentId = evidenceDocumentId;
        RequiresApproval = requiresApproval; RequiresEvidence = requiresEvidence; AllowHalfDay = allowHalfDay;
        MinimumNoticeDays = minimumNoticeDays; MaximumConsecutiveDays = maximumConsecutiveDays;
        Status = LeaveRequestStatus.Draft;
        RaiseDomainEvent(new LeaveRequestCreatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, OrganizationId, EmployeeId, LeavePolicyId));
    }

    public OrganizationId OrganizationId { get; private set; }
    public EmployeeId EmployeeId { get; private set; }
    public LeavePolicyId LeavePolicyId { get; private set; }
    public string PolicyCode { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public LeaveDayPortion StartPortion { get; private set; }
    public LeaveDayPortion EndPortion { get; private set; }
    public string? Reason { get; private set; }
    public DocumentId? EvidenceDocumentId { get; private set; }
    public bool RequiresApproval { get; private set; }
    public bool RequiresEvidence { get; private set; }
    public bool AllowHalfDay { get; private set; }
    public int? MinimumNoticeDays { get; private set; }
    public int? MaximumConsecutiveDays { get; private set; }
    public LeaveRequestStatus Status { get; private set; }
    public DateTimeOffset? SubmittedAtUtc { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
    public UserId? DecidedByUserId { get; private set; }
    public string? DecisionReason { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public UserId? CancelledByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<LeaveRequest> Create(LeaveRequestId id, OrganizationId organizationId, EmployeeId employeeId,
        LeavePolicyId leavePolicyId, string policyCode, string countryCode, DateOnly startDate, DateOnly endDate,
        LeaveDayPortion startPortion, LeaveDayPortion endPortion, string? reason, DocumentId? evidenceDocumentId,
        bool requiresApproval, bool requiresEvidence, bool allowHalfDay, int? minimumNoticeDays,
        int? maximumConsecutiveDays, DateTimeOffset nowUtc)
    {
        var error = Validate(id, organizationId, employeeId, leavePolicyId, policyCode, countryCode, startDate, endDate,
            startPortion, endPortion, allowHalfDay, maximumConsecutiveDays);
        return error is null
            ? Result.Success(new LeaveRequest(id, organizationId, employeeId, leavePolicyId, policyCode, countryCode,
                startDate, endDate, startPortion, endPortion, reason, evidenceDocumentId, requiresApproval,
                requiresEvidence, allowHalfDay, minimumNoticeDays, maximumConsecutiveDays, nowUtc))
            : Result.Failure<LeaveRequest>(error);
    }

    public Result Update(DateOnly startDate, DateOnly endDate, LeaveDayPortion startPortion, LeaveDayPortion endPortion,
        string? reason, DocumentId? evidenceDocumentId, DateTimeOffset nowUtc, UserId actor)
    {
        if (Status != LeaveRequestStatus.Draft) return Result.Failure(LeaveRequestErrors.OnlyDraftCanBeEdited);
        var error = Validate(Id, OrganizationId, EmployeeId, LeavePolicyId, PolicyCode, CountryCode, startDate, endDate,
            startPortion, endPortion, AllowHalfDay, MaximumConsecutiveDays);
        if (error is not null) return Result.Failure(error);
        StartDate = startDate; EndDate = endDate; StartPortion = startPortion; EndPortion = endPortion;
        Reason = NormalizeOptional(reason); EvidenceDocumentId = evidenceDocumentId; SetModifiedAudit(nowUtc, actor);
        RaiseDomainEvent(new LeaveRequestUpdatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, OrganizationId, EmployeeId, actor));
        return Result.Success();
    }

    public Result Submit(DateOnly today, DateTimeOffset nowUtc, UserId actor)
    {
        if (Status != LeaveRequestStatus.Draft) return Result.Failure(LeaveRequestErrors.InvalidTransition);
        if (RequiresEvidence && EvidenceDocumentId is null) return Result.Failure(LeaveRequestErrors.EvidenceRequired);
        if (MinimumNoticeDays is int notice && StartDate.DayNumber - today.DayNumber < notice)
            return Result.Failure(LeaveRequestErrors.MinimumNoticeNotMet);
        SubmittedAtUtc = nowUtc.ToUniversalTime(); SetModifiedAudit(nowUtc, actor);
        if (RequiresApproval)
        {
            Status = LeaveRequestStatus.Submitted;
            RaiseDomainEvent(new LeaveRequestSubmittedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, OrganizationId, EmployeeId, actor));
        }
        else
        {
            Status = LeaveRequestStatus.Approved; DecidedAtUtc = nowUtc.ToUniversalTime(); DecidedByUserId = actor;
            RaiseDomainEvent(new LeaveRequestAutoApprovedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, OrganizationId, EmployeeId, actor));
        }
        return Result.Success();
    }

    public Result Approve(DateTimeOffset nowUtc, UserId actor, string? reason)
    {
        if (Status != LeaveRequestStatus.Submitted) return Result.Failure(LeaveRequestErrors.InvalidTransition);
        Status = LeaveRequestStatus.Approved; DecidedAtUtc = nowUtc.ToUniversalTime(); DecidedByUserId = actor;
        DecisionReason = NormalizeOptional(reason); SetModifiedAudit(nowUtc, actor);
        RaiseDomainEvent(new LeaveRequestApprovedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, OrganizationId, EmployeeId, actor));
        return Result.Success();
    }

    public Result Reject(DateTimeOffset nowUtc, UserId actor, string reason)
    {
        if (Status != LeaveRequestStatus.Submitted) return Result.Failure(LeaveRequestErrors.InvalidTransition);
        if (string.IsNullOrWhiteSpace(reason)) return Result.Failure(LeaveRequestErrors.DecisionReasonRequired);
        Status = LeaveRequestStatus.Rejected; DecidedAtUtc = nowUtc.ToUniversalTime(); DecidedByUserId = actor;
        DecisionReason = reason.Trim(); SetModifiedAudit(nowUtc, actor);
        RaiseDomainEvent(new LeaveRequestRejectedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, OrganizationId, EmployeeId, actor, DecisionReason));
        return Result.Success();
    }

    public Result Cancel(DateOnly today, DateTimeOffset nowUtc, UserId actor, string? reason)
    {
        if (Status is LeaveRequestStatus.Cancelled or LeaveRequestStatus.Rejected) return Result.Failure(LeaveRequestErrors.InvalidTransition);
        if (Status == LeaveRequestStatus.Approved && StartDate < today) return Result.Failure(LeaveRequestErrors.StartedLeaveCannotBeCancelled);
        Status = LeaveRequestStatus.Cancelled; CancelledAtUtc = nowUtc.ToUniversalTime(); CancelledByUserId = actor;
        DecisionReason = NormalizeOptional(reason) ?? DecisionReason; SetModifiedAudit(nowUtc, actor);
        RaiseDomainEvent(new LeaveRequestCancelledDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, OrganizationId, EmployeeId, actor));
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at, UserId? by) { if (CreatedAtUtc != default) return; CreatedAtUtc = at.ToUniversalTime(); CreatedByUserId = by; }
    public void SetModifiedAudit(DateTimeOffset at, UserId? by) { LastModifiedAtUtc = at.ToUniversalTime(); LastModifiedByUserId = by; }

    private static Error? Validate(LeaveRequestId id, OrganizationId org, EmployeeId employeeId, LeavePolicyId policyId,
        string policyCode, string countryCode, DateOnly startDate, DateOnly endDate, LeaveDayPortion startPortion,
        LeaveDayPortion endPortion, bool allowHalfDay, int? maxDays)
    {
        if (id.IsEmpty) return LeaveRequestErrors.InvalidIdentifier;
        if (org.IsEmpty || employeeId.IsEmpty || policyId.IsEmpty) return LeaveRequestErrors.InvalidReference;
        if (string.IsNullOrWhiteSpace(policyCode)) return LeaveRequestErrors.InvalidPolicySnapshot;
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Trim().Length != 2) return LeaveRequestErrors.InvalidPolicySnapshot;
        if (endDate < startDate) return LeaveRequestErrors.InvalidPeriod;
        if (!allowHalfDay && (startPortion != LeaveDayPortion.FullDay || endPortion != LeaveDayPortion.FullDay)) return LeaveRequestErrors.HalfDayNotAllowed;
        if (startDate == endDate && startPortion != LeaveDayPortion.FullDay && endPortion != LeaveDayPortion.FullDay && startPortion != endPortion) return LeaveRequestErrors.InvalidDayPortion;
        var calendarDays = endDate.DayNumber - startDate.DayNumber + 1;
        if (maxDays is int max && calendarDays > max) return LeaveRequestErrors.MaximumDurationExceeded;
        return null;
    }
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
