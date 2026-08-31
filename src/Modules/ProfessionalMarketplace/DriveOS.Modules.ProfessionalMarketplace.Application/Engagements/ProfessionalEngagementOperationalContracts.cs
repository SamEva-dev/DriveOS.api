using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;

/// <summary>
/// Operational eligibility projection for consumers outside BC-13.
/// It deliberately exposes no ProfessionalMarketplace persistence model.
/// </summary>
public sealed record ExternalProfessionalOperationalEligibility(
    bool IsKnownExternalProfessional,
    bool IsEligible,
    string? ReasonCode,
    ProfessionalEngagementId? EngagementId);

public interface IProfessionalEngagementOperationalReadService
{
    Task<ExternalProfessionalOperationalEligibility> CheckAsync(
        OrganizationId organizationId,
        UserId userId,
        DateOnly date,
        BranchId? branchId = null,
        string? teachingCategoryCode = null,
        CancellationToken cancellationToken = default);
}
