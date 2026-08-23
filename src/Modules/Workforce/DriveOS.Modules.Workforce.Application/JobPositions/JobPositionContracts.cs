using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Application.JobPositions;

public sealed record CreateJobPositionCommand(OrganizationId OrganizationId, JobPositionId JobPositionId, string Code, string Name, string? Description, ProfessionalFunction ProfessionalFunction, UserId ActorUserId) : ICommand<JobPositionId>;
public sealed record UpdateJobPositionCommand(OrganizationId OrganizationId, JobPositionId JobPositionId, string Code, string Name, string? Description, ProfessionalFunction ProfessionalFunction, UserId ActorUserId) : ICommand;
public sealed record DeactivateJobPositionCommand(OrganizationId OrganizationId, JobPositionId JobPositionId, UserId ActorUserId) : ICommand;
public sealed record ReactivateJobPositionCommand(OrganizationId OrganizationId, JobPositionId JobPositionId, UserId ActorUserId) : ICommand;
public sealed record GetJobPositionsQuery(OrganizationId OrganizationId, JobPositionStatus? Status) : IQuery<IReadOnlyList<JobPositionResponse>>;
public sealed record GetJobPositionQuery(OrganizationId OrganizationId, JobPositionId JobPositionId) : IQuery<JobPositionResponse>;
public sealed record JobPositionResponse(Guid Id, string Code, string Name, string? Description, string ProfessionalFunction, string Status, DateTimeOffset CreatedAtUtc, DateTimeOffset? LastModifiedAtUtc);

public sealed record AddEmployeeJobPositionAssignmentCommand(OrganizationId OrganizationId, EmployeeId EmployeeId, EmployeeJobPositionAssignmentId AssignmentId, JobPositionId JobPositionId, BranchId? BranchId, DateOnly StartDate, DateOnly? EndDate, bool IsPrimary, UserId ActorUserId) : ICommand<EmployeeJobPositionAssignmentId>;
public sealed record UpdateEmployeeJobPositionAssignmentCommand(OrganizationId OrganizationId, EmployeeId EmployeeId, EmployeeJobPositionAssignmentId AssignmentId, DateOnly StartDate, DateOnly? EndDate, bool IsPrimary, UserId ActorUserId) : ICommand;
public sealed record EndEmployeeJobPositionAssignmentCommand(OrganizationId OrganizationId, EmployeeId EmployeeId, EmployeeJobPositionAssignmentId AssignmentId, DateOnly EndDate, UserId ActorUserId) : ICommand;
public sealed record CancelEmployeeJobPositionAssignmentCommand(OrganizationId OrganizationId, EmployeeId EmployeeId, EmployeeJobPositionAssignmentId AssignmentId, UserId ActorUserId) : ICommand;
public sealed record GetEmployeeJobPositionAssignmentsQuery(OrganizationId OrganizationId, EmployeeId EmployeeId) : IQuery<IReadOnlyList<EmployeeJobPositionAssignmentResponse>>;
public sealed record EmployeeJobPositionAssignmentResponse(Guid Id, Guid JobPositionId, Guid? BranchId, DateOnly StartDate, DateOnly? EndDate, bool IsPrimary, string Status);
