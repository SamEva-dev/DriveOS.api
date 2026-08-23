using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Workforce.Domain.Events;

public sealed record EmployeeCreatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    EmployeeId EmployeeId,
    OrganizationId EmployerOrganizationId,
    PersonId PersonId) : IDomainEvent;

public sealed record EmployeeIdentityUpdatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    EmployeeId EmployeeId,
    OrganizationId EmployerOrganizationId) : IDomainEvent;

public sealed record EmployeeOnboardingStartedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    EmployeeId EmployeeId,
    OrganizationId EmployerOrganizationId,
    UserId ActorUserId) : IDomainEvent;

public sealed record EmployeeActivatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    EmployeeId EmployeeId,
    OrganizationId EmployerOrganizationId,
    UserId? LinkedUserId,
    UserId ActorUserId) : IDomainEvent;

public sealed record EmployeeSuspendedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    EmployeeId EmployeeId,
    OrganizationId EmployerOrganizationId,
    string Reason,
    UserId ActorUserId) : IDomainEvent;

public sealed record EmployeeReactivatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    EmployeeId EmployeeId,
    OrganizationId EmployerOrganizationId,
    UserId? LinkedUserId,
    UserId ActorUserId) : IDomainEvent;

public sealed record EmploymentTerminationStartedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    EmployeeId EmployeeId,
    OrganizationId EmployerOrganizationId,
    DateOnly PlannedEndDate,
    string Reason,
    UserId ActorUserId) : IDomainEvent;

public sealed record EmploymentEndedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    EmployeeId EmployeeId,
    OrganizationId EmployerOrganizationId,
    DateOnly EndDate,
    UserId? LinkedUserId,
    string Reason,
    UserId ActorUserId) : IDomainEvent;

public sealed record EmployeeBranchAssignedDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, EmployeeId EmployeeId, OrganizationId EmployerOrganizationId, EmployeeBranchAssignmentId AssignmentId, BranchId BranchId, DateOnly StartDate, DateOnly? EndDate, bool IsPrimary, UserId ActorUserId) : IDomainEvent;
public sealed record EmployeeBranchAssignmentUpdatedDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, EmployeeId EmployeeId, OrganizationId EmployerOrganizationId, EmployeeBranchAssignmentId AssignmentId, BranchId BranchId, DateOnly StartDate, DateOnly? EndDate, bool IsPrimary, UserId ActorUserId) : IDomainEvent;
public sealed record EmployeeBranchAssignmentEndedDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, EmployeeId EmployeeId, OrganizationId EmployerOrganizationId, EmployeeBranchAssignmentId AssignmentId, BranchId BranchId, DateOnly EndDate, UserId ActorUserId) : IDomainEvent;
public sealed record EmployeeBranchAssignmentCancelledDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, EmployeeId EmployeeId, OrganizationId EmployerOrganizationId, EmployeeBranchAssignmentId AssignmentId, BranchId BranchId, UserId ActorUserId) : IDomainEvent;

public sealed record JobPositionCreatedDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, JobPositionId JobPositionId, OrganizationId OrganizationId, string Code, DriveOS.Modules.Workforce.Domain.JobPositions.ProfessionalFunction ProfessionalFunction) : IDomainEvent;
public sealed record JobPositionUpdatedDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, JobPositionId JobPositionId, OrganizationId OrganizationId, string Code, DriveOS.Modules.Workforce.Domain.JobPositions.ProfessionalFunction ProfessionalFunction, UserId ActorUserId) : IDomainEvent;
public sealed record JobPositionDeactivatedDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, JobPositionId JobPositionId, OrganizationId OrganizationId, UserId ActorUserId) : IDomainEvent;
public sealed record JobPositionReactivatedDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, JobPositionId JobPositionId, OrganizationId OrganizationId, UserId ActorUserId) : IDomainEvent;
public sealed record EmployeeJobPositionAssignedDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, EmployeeId EmployeeId, OrganizationId EmployerOrganizationId, EmployeeJobPositionAssignmentId AssignmentId, JobPositionId JobPositionId, BranchId? BranchId, DateOnly StartDate, DateOnly? EndDate, bool IsPrimary, UserId ActorUserId) : IDomainEvent;
public sealed record EmployeeJobPositionAssignmentUpdatedDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, EmployeeId EmployeeId, OrganizationId EmployerOrganizationId, EmployeeJobPositionAssignmentId AssignmentId, JobPositionId JobPositionId, BranchId? BranchId, DateOnly StartDate, DateOnly? EndDate, bool IsPrimary, UserId ActorUserId) : IDomainEvent;
public sealed record EmployeeJobPositionAssignmentEndedDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, EmployeeId EmployeeId, OrganizationId EmployerOrganizationId, EmployeeJobPositionAssignmentId AssignmentId, JobPositionId JobPositionId, DateOnly EndDate, UserId ActorUserId) : IDomainEvent;
public sealed record EmployeeJobPositionAssignmentCancelledDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, EmployeeId EmployeeId, OrganizationId EmployerOrganizationId, EmployeeJobPositionAssignmentId AssignmentId, JobPositionId JobPositionId, UserId ActorUserId) : IDomainEvent;

public sealed record EmployeeRehiredDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    EmployeeId PreviousEmployeeId,
    EmployeeId NewEmployeeId,
    OrganizationId EmployerOrganizationId,
    PersonId PersonId,
    UserId? UserId,
    DateOnly EmploymentStartDate,
    UserId ActorUserId) : IDomainEvent;
