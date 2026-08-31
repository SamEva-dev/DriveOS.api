using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Events;

public sealed record ProfessionalProfileCreatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    ProfessionalProfileId ProfessionalProfileId,
    PersonId PersonId,
    OrganizationId ProviderOrganizationId) : IDomainEvent;

public sealed record ProfessionalProfileActivatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    ProfessionalProfileId ProfessionalProfileId,
    PersonId PersonId,
    OrganizationId ProviderOrganizationId,
    UserId ActorUserId) : IDomainEvent;

public sealed record ProfessionalProfileSuspendedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    ProfessionalProfileId ProfessionalProfileId,
    OrganizationId ProviderOrganizationId,
    string Reason,
    UserId ActorUserId) : IDomainEvent;

public sealed record ProfessionalProfileCompletedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    ProfessionalProfileId ProfessionalProfileId,
    PersonId PersonId,
    OrganizationId ProviderOrganizationId,
    UserId ActorUserId) : IDomainEvent;

public sealed record ProfessionalProfileUpdatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    ProfessionalProfileId ProfessionalProfileId,
    OrganizationId ProviderOrganizationId,
    string ChangeType,
    UserId ActorUserId) : IDomainEvent;
