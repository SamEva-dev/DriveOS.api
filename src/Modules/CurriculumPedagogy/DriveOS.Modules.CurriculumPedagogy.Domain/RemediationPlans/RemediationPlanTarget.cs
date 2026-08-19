using DriveOS.SharedKernel.Domain;using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.CurriculumPedagogy.Domain.RemediationPlans;
public sealed class RemediationPlanTarget:Entity<RemediationPlanTargetId>{private RemediationPlanTarget(){}internal RemediationPlanTarget(RemediationPlanTargetId id,CompetencyId competencyId,string objective):base(id){CompetencyId=competencyId;Objective=objective;}public CompetencyId CompetencyId{get;private set;}public string Objective{get;private set;}=string.Empty;}
