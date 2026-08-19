using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.LicenseCategories.Events;

public sealed record LicenseCategoryDefinitionActivatedDomainEvent(
    LicenseCategoryDefinitionId LicenseCategoryDefinitionId,
    OrganizationId OrganizationId,
    UserId ActorUserId,
    DateTimeOffset ActivatedAtUtc) : DomainEvent;
