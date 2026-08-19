using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula.Events;
public sealed record CurriculumVersionPublishedDomainEvent(CurriculumId CurriculumId, OrganizationId OrganizationId, CurriculumVersionId VersionId, int VersionNumber, UserId PublishedByUserId, DateTimeOffset PublishedAtUtc) : DomainEvent;
