using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.LicenseCategories.Events;

public sealed record LicenseCategoryDefinitionArchivedDomainEvent(
    LicenseCategoryDefinitionId LicenseCategoryDefinitionId,
    OrganizationId OrganizationId,
    UserId ActorUserId,
    DateTimeOffset ArchivedAtUtc) : DomainEvent;
