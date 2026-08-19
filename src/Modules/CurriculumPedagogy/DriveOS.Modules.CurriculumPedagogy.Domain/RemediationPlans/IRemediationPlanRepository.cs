using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.CurriculumPedagogy.Domain.RemediationPlans;
public interface IRemediationPlanRepository{Task<RemediationPlan?>GetByIdForUpdateAsync(OrganizationId organizationId,RemediationPlanId id,CancellationToken ct=default);Task<bool>HasOpenPlanAsync(OrganizationId organizationId,TrainingPathId trainingPathId,CancellationToken ct=default);Task AddAsync(RemediationPlan plan,CancellationToken ct=default);}
