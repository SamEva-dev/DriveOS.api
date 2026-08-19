using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula.Events;

public sealed record CurriculumVersionCreatedDomainEvent(
    CurriculumId CurriculumId,
    OrganizationId OrganizationId,
    CurriculumVersionId CurriculumVersionId,
    int VersionNumber,
    CurriculumVersionId? SourceVersionId,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    UserId CreatedByUserId,
    DateTimeOffset CreatedAtUtc) : DomainEvent;
