using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula.Events;

public sealed record CurriculumModuleUpdatedDomainEvent(
    CurriculumId CurriculumId,
    CurriculumVersionId CurriculumVersionId,
    CurriculumModuleId ModuleId,
    string Name,
    int Order) : DomainEvent;
