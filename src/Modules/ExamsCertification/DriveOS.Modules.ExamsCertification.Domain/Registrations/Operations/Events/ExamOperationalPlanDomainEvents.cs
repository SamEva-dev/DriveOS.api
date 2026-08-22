using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations.Events;

public sealed record ExamOperationalPlanCreatedDomainEvent(
    ExamOperationalPlanId PlanId, OrganizationId OrganizationId, ExamRegistrationId RegistrationId, PersonId StudentId) : DomainEvent;

public sealed record ExamOperationalPlanRefreshedDomainEvent(
    ExamOperationalPlanId PlanId, OrganizationId OrganizationId, ExamRegistrationId RegistrationId, int ConvocationVersion, bool HasConflicts) : DomainEvent;
