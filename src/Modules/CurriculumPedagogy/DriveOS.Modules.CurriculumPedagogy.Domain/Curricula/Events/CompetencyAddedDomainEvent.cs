using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula.Events;

public sealed record CompetencyAddedDomainEvent(
    CurriculumId CurriculumId,
    CurriculumVersionId CurriculumVersionId,
    CurriculumModuleId ModuleId,
    CompetencyId CompetencyId,
    string Code,
    int Order) : DomainEvent;
