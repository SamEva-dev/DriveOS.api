using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;

public sealed record ProfessionalSchedulingPreparationRequest(
    OrganizationId OrganizationId,
    BranchId? BranchId,
    UserId ProfessionalUserId,
    string DisplayName,
    string TimeZoneId,
    string[] TeachingCategoryCodes,
    DateOnly StartsOn,
    DateOnly EndsOn);

public sealed record ProfessionalSchedulingPreparationResult(
    bool IsPrepared,
    Guid? CalendarResourceId,
    string? ReasonCode);

/// <summary>
/// BC-13 port implemented at the composition root. The implementation may use Scheduling,
/// but ProfessionalMarketplace Application remains independent from SchedulingCapacity.
/// </summary>
public interface IProfessionalSchedulingPreparationGateway
{
    Task<ProfessionalSchedulingPreparationResult> PrepareAsync(
        ProfessionalSchedulingPreparationRequest request,
        CancellationToken cancellationToken = default);
}
