using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Students.Domain.Events;

public sealed record StudentCreatedDomainEvent(PersonId StudentId, OrganizationId OrganizationId)
    : DomainEvent;

public sealed record StudentIdentityChangedDomainEvent(PersonId StudentId, OrganizationId OrganizationId)
    : DomainEvent;

public sealed record StudentIdentityVerifiedDomainEvent(PersonId StudentId, OrganizationId OrganizationId)
    : DomainEvent;

public sealed record StudentStatusChangedDomainEvent(
    PersonId StudentId,
    OrganizationId OrganizationId,
    string Status)
    : DomainEvent;

public sealed record EnrollmentCreatedDomainEvent(
    DraftEnrollmentId EnrollmentId,
    PersonId StudentId,
    OrganizationId OrganizationId)
    : DomainEvent;

public sealed record EnrollmentStatusChangedDomainEvent(
    DraftEnrollmentId EnrollmentId,
    PersonId StudentId,
    OrganizationId OrganizationId,
    string Status)
    : DomainEvent;

public sealed record EnrollmentBranchChangedDomainEvent(
    DraftEnrollmentId EnrollmentId,
    PersonId StudentId,
    OrganizationId OrganizationId,
    BranchId BranchId)
    : DomainEvent;

public sealed record StudentAggregateChangedDomainEvent<TId>(
    TId AggregateId,
    PersonId StudentId,
    OrganizationId OrganizationId,
    string Change)
    : DomainEvent
    where TId : notnull;
