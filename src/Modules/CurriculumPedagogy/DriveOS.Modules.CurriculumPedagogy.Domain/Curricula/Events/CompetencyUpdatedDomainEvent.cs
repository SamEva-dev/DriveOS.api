using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula.Events;

public sealed record CompetencyUpdatedDomainEvent(
    CurriculumId CurriculumId,
    CurriculumVersionId CurriculumVersionId,
    CurriculumModuleId ModuleId,
    CompetencyId CompetencyId,
    string Name,
    int Order,
    bool IsRequired) : DomainEvent;
