using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments.Events;

public sealed record ExamResourceAssignmentCreatedDomainEvent(
    ExamResourceAssignmentId AssignmentId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId) : DomainEvent;

public sealed record ExamResourcesAssignedDomainEvent(
    ExamResourceAssignmentId AssignmentId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    BookingId BookingId) : DomainEvent;
