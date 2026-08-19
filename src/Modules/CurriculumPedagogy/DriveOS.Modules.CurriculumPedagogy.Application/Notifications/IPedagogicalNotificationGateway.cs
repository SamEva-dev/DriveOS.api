using DriveOS.SharedKernel.Identifiers;
using DriveOS.Modules.CurriculumPedagogy.Domain.Readiness;

namespace DriveOS.Modules.CurriculumPedagogy.Application.Notifications;

public interface IPedagogicalNotificationGateway
{
    Task<Guid?> QueueTrainingPathSuspendedAsync(OrganizationId organizationId, PersonId studentId, string reason, CancellationToken cancellationToken = default);
    Task<Guid?> QueueTrainingPathReactivatedAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default);
    Task<Guid?> QueuePedagogicalReviewCompletedAsync(OrganizationId organizationId, PersonId studentId, string recommendations, decimal? remainingHours, CancellationToken cancellationToken = default);
    Task<Guid?> QueueRemediationActivatedAsync(OrganizationId organizationId, PersonId studentId, string recommendation, DateOnly reviewDate, CancellationToken cancellationToken = default);
    Task<Guid?> QueueRemediationCompletedAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default);
    Task<Guid?> QueueReadinessDecisionAsync(OrganizationId organizationId, PersonId studentId, PedagogicalReadinessDecisionStatus decision, string rationale, string? conditions, CancellationToken cancellationToken = default);
}
