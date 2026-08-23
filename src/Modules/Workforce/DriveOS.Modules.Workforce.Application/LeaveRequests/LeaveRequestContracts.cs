using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Workforce.Domain.LeaveRequests;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Application.LeaveRequests;

public sealed record CreateLeaveRequestCommand(OrganizationId OrganizationId, LeaveRequestId LeaveRequestId, EmployeeId EmployeeId, LeavePolicyId LeavePolicyId, DateOnly StartDate, DateOnly EndDate, LeaveDayPortion StartPortion, LeaveDayPortion EndPortion, string? Reason, DocumentId? EvidenceDocumentId, UserId ActorUserId) : ICommand<LeaveRequestId>;
public sealed record UpdateLeaveRequestCommand(OrganizationId OrganizationId, LeaveRequestId LeaveRequestId, DateOnly StartDate, DateOnly EndDate, LeaveDayPortion StartPortion, LeaveDayPortion EndPortion, string? Reason, DocumentId? EvidenceDocumentId, UserId ActorUserId) : ICommand;
public sealed record SubmitLeaveRequestCommand(OrganizationId OrganizationId, LeaveRequestId LeaveRequestId, UserId ActorUserId) : ICommand;
public sealed record ApproveLeaveRequestCommand(OrganizationId OrganizationId, LeaveRequestId LeaveRequestId, string? Reason, UserId ActorUserId) : ICommand;
public sealed record RejectLeaveRequestCommand(OrganizationId OrganizationId, LeaveRequestId LeaveRequestId, string Reason, UserId ActorUserId) : ICommand;
public sealed record CancelLeaveRequestCommand(OrganizationId OrganizationId, LeaveRequestId LeaveRequestId, string? Reason, UserId ActorUserId) : ICommand;
public sealed record GetLeaveRequestQuery(OrganizationId OrganizationId, LeaveRequestId LeaveRequestId) : IQuery<LeaveRequestResponse>;
public sealed record GetLeaveRequestsQuery(OrganizationId OrganizationId, EmployeeId? EmployeeId, LeaveRequestStatus? Status, DateOnly? From, DateOnly? To) : IQuery<IReadOnlyList<LeaveRequestResponse>>;
public sealed record LeaveRequestResponse(Guid Id, Guid EmployeeId, Guid LeavePolicyId, string PolicyCode, string CountryCode, DateOnly StartDate, DateOnly EndDate, string StartPortion, string EndPortion, string? Reason, Guid? EvidenceDocumentId, bool RequiresApproval, bool RequiresEvidence, string Status, DateTimeOffset? SubmittedAtUtc, DateTimeOffset? DecidedAtUtc, Guid? DecidedByUserId, string? DecisionReason, DateTimeOffset? CancelledAtUtc);
