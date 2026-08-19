using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula.Events;

public sealed record CurriculumModuleAddedDomainEvent(
    CurriculumId CurriculumId,
    CurriculumVersionId CurriculumVersionId,
    CurriculumModuleId ModuleId,
    string Code,
    int Order) : DomainEvent;
