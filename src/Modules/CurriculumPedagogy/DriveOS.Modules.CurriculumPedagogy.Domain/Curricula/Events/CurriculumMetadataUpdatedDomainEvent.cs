using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula.Events;

public sealed record CurriculumMetadataUpdatedDomainEvent(
    CurriculumId CurriculumId,
    OrganizationId OrganizationId,
    string Name) : DomainEvent;
