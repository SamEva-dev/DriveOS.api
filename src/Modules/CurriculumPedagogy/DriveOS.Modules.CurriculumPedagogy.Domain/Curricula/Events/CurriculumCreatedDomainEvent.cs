using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula.Events;

public sealed record CurriculumCreatedDomainEvent(
    CurriculumId CurriculumId,
    OrganizationId OrganizationId,
    string Code,
    string CountryCode,
    string LicenseCategoryCode) : DomainEvent;
