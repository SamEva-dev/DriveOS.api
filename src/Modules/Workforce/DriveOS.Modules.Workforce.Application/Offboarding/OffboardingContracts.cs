using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Workforce.Domain.Offboarding;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Application.Offboarding;

public sealed record OffboardingChecklistItemResponse(Guid Id,string Kind,bool IsAutomatic,string Status,int BlockerCount,string? Note,DateTimeOffset? ResolvedAtUtc,Guid? ResolvedByUserId,string? WaiverReason,DateTimeOffset? LastEvaluatedAtUtc);
public sealed record OffboardingResponse(Guid Id,Guid EmployeeId,DateOnly PlannedEndDate,string Reason,string Status,DateTimeOffset? CompletedAtUtc,Guid? CompletedByUserId,IReadOnlyList<OffboardingChecklistItemResponse> Items);
public sealed record OffboardingDependencySnapshot(int BranchAssignments,int JobPositions,int EmploymentContracts,int EquipmentAssignments,int Timesheets,int ProfessionalRestrictions);
public interface IOffboardingDependencyReadService { Task<OffboardingDependencySnapshot> GetAsync(OrganizationId organizationId,EmployeeId employeeId,DateOnly plannedEndDate,CancellationToken ct=default); }

public sealed record GetEmployeeOffboardingQuery(OrganizationId OrganizationId,EmployeeId EmployeeId):IQuery<OffboardingResponse>;
public sealed record RefreshOffboardingCommand(OrganizationId OrganizationId,EmployeeId EmployeeId,UserId ActorUserId):ICommand;
public sealed record CompleteOffboardingChecklistItemCommand(OrganizationId OrganizationId,EmployeeId EmployeeId,OffboardingChecklistItemKind Kind,string? Note,UserId ActorUserId):ICommand;
public sealed record WaiveOffboardingChecklistItemCommand(OrganizationId OrganizationId,EmployeeId EmployeeId,OffboardingChecklistItemKind Kind,string Reason,UserId ActorUserId):ICommand;
public sealed record CompleteOffboardingCommand(OrganizationId OrganizationId,EmployeeId EmployeeId,string CompletionReason,UserId ActorUserId):ICommand;

public interface IEmployeeApplicationAccessRevoker
{
    Task<DriveOS.SharedKernel.Results.Result> RevokeAsync(OrganizationId organizationId,UserId userId,string reason,CancellationToken ct=default);
}
public sealed record RevokeOffboardingAccessCommand(OrganizationId OrganizationId,EmployeeId EmployeeId,string Reason,UserId ActorUserId):ICommand;
