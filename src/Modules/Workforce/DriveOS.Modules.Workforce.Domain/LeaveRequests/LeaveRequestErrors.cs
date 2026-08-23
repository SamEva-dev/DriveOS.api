using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.LeaveRequests;
public static class LeaveRequestErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Workforce.LeaveRequest.InvalidIdentifier", "errors.workforce.leaveRequest.invalidIdentifier");
    public static readonly Error InvalidReference = Error.Validation("Workforce.LeaveRequest.InvalidReference", "errors.workforce.leaveRequest.invalidReference");
    public static readonly Error InvalidPolicySnapshot = Error.Validation("Workforce.LeaveRequest.InvalidPolicySnapshot", "errors.workforce.leaveRequest.invalidPolicySnapshot");
    public static readonly Error InvalidPeriod = Error.Validation("Workforce.LeaveRequest.InvalidPeriod", "errors.workforce.leaveRequest.invalidPeriod");
    public static readonly Error HalfDayNotAllowed = Error.Validation("Workforce.LeaveRequest.HalfDayNotAllowed", "errors.workforce.leaveRequest.halfDayNotAllowed");
    public static readonly Error InvalidDayPortion = Error.Validation("Workforce.LeaveRequest.InvalidDayPortion", "errors.workforce.leaveRequest.invalidDayPortion");
    public static readonly Error MaximumDurationExceeded = Error.Validation("Workforce.LeaveRequest.MaximumDurationExceeded", "errors.workforce.leaveRequest.maximumDurationExceeded");
    public static readonly Error MinimumNoticeNotMet = Error.Validation("Workforce.LeaveRequest.MinimumNoticeNotMet", "errors.workforce.leaveRequest.minimumNoticeNotMet");
    public static readonly Error EvidenceRequired = Error.Validation("Workforce.LeaveRequest.EvidenceRequired", "errors.workforce.leaveRequest.evidenceRequired");
    public static readonly Error OnlyDraftCanBeEdited = Error.Conflict("Workforce.LeaveRequest.OnlyDraftCanBeEdited", "errors.workforce.leaveRequest.onlyDraftCanBeEdited");
    public static readonly Error InvalidTransition = Error.Conflict("Workforce.LeaveRequest.InvalidTransition", "errors.workforce.leaveRequest.invalidTransition");
    public static readonly Error DecisionReasonRequired = Error.Validation("Workforce.LeaveRequest.DecisionReasonRequired", "errors.workforce.leaveRequest.decisionReasonRequired");
    public static readonly Error StartedLeaveCannotBeCancelled = Error.Conflict("Workforce.LeaveRequest.StartedLeaveCannotBeCancelled", "errors.workforce.leaveRequest.startedLeaveCannotBeCancelled");
    public static readonly Error OverlappingRequest = Error.Conflict("Workforce.LeaveRequest.OverlappingRequest", "errors.workforce.leaveRequest.overlappingRequest");
    public static readonly Error PolicyInactive = Error.Conflict("Workforce.LeaveRequest.PolicyInactive", "errors.workforce.leaveRequest.policyInactive");
    public static readonly Error EmployeeNotEligible = Error.Conflict("Workforce.LeaveRequest.EmployeeNotEligible", "errors.workforce.leaveRequest.employeeNotEligible");
    public static readonly Error PeriodOutsideEmployment = Error.Conflict("Workforce.LeaveRequest.PeriodOutsideEmployment", "errors.workforce.leaveRequest.periodOutsideEmployment");
    public static readonly Error NotFound = Error.NotFound("Workforce.LeaveRequest.NotFound", "errors.workforce.leaveRequest.notFound");
}
