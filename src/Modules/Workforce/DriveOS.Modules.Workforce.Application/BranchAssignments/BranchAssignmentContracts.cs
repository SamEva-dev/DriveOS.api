using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Workforce.Application.BranchAssignments;

public sealed record WorkforceBranchSnapshot(Guid Id, string Name, string Code, string Status);
public interface IWorkforceBranchDirectory
{
    Task<WorkforceBranchSnapshot?> GetAsync(OrganizationId organizationId, BranchId branchId, CancellationToken cancellationToken = default);
}

public sealed record AddEmployeeBranchAssignmentCommand(OrganizationId OrganizationId, EmployeeId EmployeeId, EmployeeBranchAssignmentId AssignmentId, BranchId BranchId, DateOnly StartDate, DateOnly? EndDate, bool IsPrimary, UserId ActorUserId) : ICommand<EmployeeBranchAssignmentId>;
public sealed record UpdateEmployeeBranchAssignmentCommand(OrganizationId OrganizationId, EmployeeId EmployeeId, EmployeeBranchAssignmentId AssignmentId, DateOnly StartDate, DateOnly? EndDate, bool IsPrimary, UserId ActorUserId) : ICommand;
public sealed record EndEmployeeBranchAssignmentCommand(OrganizationId OrganizationId, EmployeeId EmployeeId, EmployeeBranchAssignmentId AssignmentId, DateOnly EndDate, UserId ActorUserId) : ICommand;
public sealed record CancelEmployeeBranchAssignmentCommand(OrganizationId OrganizationId, EmployeeId EmployeeId, EmployeeBranchAssignmentId AssignmentId, UserId ActorUserId) : ICommand;
public sealed record GetEmployeeBranchAssignmentsQuery(OrganizationId OrganizationId, EmployeeId EmployeeId) : IQuery<IReadOnlyList<EmployeeBranchAssignmentResponse>>;
public sealed record EmployeeBranchAssignmentResponse(Guid Id, Guid BranchId, DateOnly StartDate, DateOnly? EndDate, bool IsPrimary, string Status, DateTimeOffset CreatedAtUtc, DateTimeOffset? LastModifiedAtUtc);
