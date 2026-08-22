using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation.Events;

public sealed record ExamPreparationCreatedDomainEvent(
    ExamPreparationId PreparationId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId) : DomainEvent;

public sealed record ExamPreparationRefreshedDomainEvent(
    ExamPreparationId PreparationId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    int Revision,
    bool IsReady) : DomainEvent;

public sealed record ExamPreparationConfirmedDomainEvent(
    ExamPreparationId PreparationId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    int Revision,
    UserId ConfirmedByUserId) : DomainEvent;

public sealed record ExamPreparationConfirmationInvalidatedDomainEvent(
    ExamPreparationId PreparationId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    int PreviousConfirmedRevision,
    int NewRevision) : DomainEvent;

public sealed record ExamPreparationUrgentChangeDetectedDomainEvent(
    ExamPreparationId PreparationId,
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    IReadOnlyCollection<string> ChangedBlockingChecks) : DomainEvent;
