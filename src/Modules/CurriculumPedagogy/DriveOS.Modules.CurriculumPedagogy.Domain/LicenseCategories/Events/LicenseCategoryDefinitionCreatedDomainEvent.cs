using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.LicenseCategories.Events;

public sealed record LicenseCategoryDefinitionCreatedDomainEvent(
    LicenseCategoryDefinitionId LicenseCategoryDefinitionId,
    OrganizationId OrganizationId,
    string CountryCode,
    string LicenseCategoryCode,
    string Name) : DomainEvent;
