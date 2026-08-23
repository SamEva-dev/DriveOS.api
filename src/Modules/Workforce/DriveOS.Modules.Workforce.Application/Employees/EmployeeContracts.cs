using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Application.Employees;
public sealed record CreateEmployeeCommand(OrganizationId EmployerOrganizationId, EmployeeId EmployeeId, PersonId PersonId, UserId? UserId, string EmployeeNumber, DateOnly EmploymentStartDate, DateOnly? EmploymentEndDate, UserId ActorUserId) : ICommand<EmployeeId>;
public sealed record UpdateEmployeeIdentityCommand(OrganizationId EmployerOrganizationId, EmployeeId EmployeeId, UserId? UserId, string EmployeeNumber, DateOnly EmploymentStartDate, DateOnly? EmploymentEndDate, UserId ActorUserId) : ICommand;
public sealed record GetEmployeeQuery(OrganizationId EmployerOrganizationId, EmployeeId EmployeeId) : IQuery<EmployeeResponse>;
public sealed record GetEmployeeEmploymentHistoryQuery(OrganizationId EmployerOrganizationId, EmployeeId EmployeeId) : IQuery<IReadOnlyList<EmployeeResponse>>;
public sealed record GetEmployeesQuery(OrganizationId EmployerOrganizationId, EmploymentStatus? Status) : IQuery<IReadOnlyList<EmployeeResponse>>;
public sealed record EmployeeResponse(Guid Id, Guid EmployerOrganizationId, Guid PersonId, Guid? UserId, string EmployeeNumber, DateOnly EmploymentStartDate, DateOnly? EmploymentEndDate, string Status, Guid? RehiredFromEmployeeId, DateTimeOffset CreatedAtUtc, DateTimeOffset? LastModifiedAtUtc);

public sealed record RehireEmployeeCommand(OrganizationId EmployerOrganizationId, EmployeeId PreviousEmployeeId, EmployeeId NewEmployeeId, UserId? UserId, bool ReusePreviousUserLink, string EmployeeNumber, DateOnly EmploymentStartDate, DateOnly? EmploymentEndDate, UserId ActorUserId) : ICommand<EmployeeId>;
public sealed record StartEmployeeOnboardingCommand(OrganizationId EmployerOrganizationId, EmployeeId EmployeeId, UserId ActorUserId) : ICommand;
public sealed record ActivateEmployeeCommand(OrganizationId EmployerOrganizationId, EmployeeId EmployeeId, UserId ActorUserId) : ICommand;
public sealed record SuspendEmployeeCommand(OrganizationId EmployerOrganizationId, EmployeeId EmployeeId, string Reason, UserId ActorUserId) : ICommand;
public sealed record ReactivateEmployeeCommand(OrganizationId EmployerOrganizationId, EmployeeId EmployeeId, UserId ActorUserId) : ICommand;
public sealed record StartEmploymentTerminationCommand(OrganizationId EmployerOrganizationId, EmployeeId EmployeeId, DateOnly PlannedEndDate, string Reason, UserId ActorUserId) : ICommand;
public sealed record EndEmploymentCommand(OrganizationId EmployerOrganizationId, EmployeeId EmployeeId, DateOnly EndDate, string Reason, UserId ActorUserId) : ICommand;
