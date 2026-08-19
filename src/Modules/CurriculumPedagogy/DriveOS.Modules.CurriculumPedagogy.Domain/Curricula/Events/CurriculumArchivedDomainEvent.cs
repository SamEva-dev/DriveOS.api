using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula.Events;

public sealed record CurriculumArchivedDomainEvent(
    CurriculumId CurriculumId,
    OrganizationId OrganizationId,
    UserId ArchivedByUserId,
    DateTimeOffset ArchivedAtUtc) : DomainEvent;
