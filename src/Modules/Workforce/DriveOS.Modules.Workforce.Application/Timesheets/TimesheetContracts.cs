using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Workforce.Domain.Timesheets;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Application.Timesheets;

public sealed record CreateTimesheetCommand(OrganizationId OrganizationId,TimesheetId TimesheetId,EmployeeId EmployeeId,DateOnly PeriodFrom,DateOnly PeriodTo,UserId ActorUserId):ICommand<TimesheetId>;
public sealed record AddTimesheetEntryCommand(OrganizationId OrganizationId,TimesheetId TimesheetId,TimesheetEntryId EntryId,DateOnly Date,TimesheetActivityType ActivityType,decimal Hours,string? Description,TimesheetEntrySource Source,string? SourceReference,UserId ActorUserId):ICommand<TimesheetEntryId>;
public sealed record UpdateTimesheetEntryCommand(OrganizationId OrganizationId,TimesheetId TimesheetId,TimesheetEntryId EntryId,DateOnly Date,TimesheetActivityType ActivityType,decimal Hours,string? Description,UserId ActorUserId):ICommand;
public sealed record RemoveTimesheetEntryCommand(OrganizationId OrganizationId,TimesheetId TimesheetId,TimesheetEntryId EntryId,UserId ActorUserId):ICommand;
public sealed record SubmitTimesheetCommand(OrganizationId OrganizationId,TimesheetId TimesheetId,UserId ActorUserId):ICommand;
public sealed record StartTimesheetReviewCommand(OrganizationId OrganizationId,TimesheetId TimesheetId,UserId ActorUserId):ICommand;
public sealed record ApproveTimesheetCommand(OrganizationId OrganizationId,TimesheetId TimesheetId,string? Reason,UserId ActorUserId):ICommand;
public sealed record RejectTimesheetCommand(OrganizationId OrganizationId,TimesheetId TimesheetId,string Reason,UserId ActorUserId):ICommand;
public sealed record LockTimesheetCommand(OrganizationId OrganizationId,TimesheetId TimesheetId,UserId ActorUserId):ICommand;
public sealed record GetTimesheetQuery(OrganizationId OrganizationId,TimesheetId TimesheetId):IQuery<TimesheetResponse>;
public sealed record GetTimesheetsQuery(OrganizationId OrganizationId,EmployeeId? EmployeeId,TimesheetStatus? Status,DateOnly? From,DateOnly? To):IQuery<IReadOnlyList<TimesheetResponse>>;
public sealed record TimesheetEntryResponse(Guid Id,DateOnly Date,string ActivityType,decimal Hours,string? Description,string Source,string? SourceReference);
public sealed record TimesheetResponse(Guid Id,Guid EmployeeId,DateOnly PeriodFrom,DateOnly PeriodTo,string Status,decimal TotalHours,DateTimeOffset? SubmittedAtUtc,Guid? SubmittedByUserId,DateTimeOffset? ReviewStartedAtUtc,Guid? ReviewerUserId,DateTimeOffset? DecidedAtUtc,Guid? DecidedByUserId,string? DecisionReason,DateTimeOffset? LockedAtUtc,Guid? LockedByUserId,IReadOnlyList<TimesheetEntryResponse> Entries);
