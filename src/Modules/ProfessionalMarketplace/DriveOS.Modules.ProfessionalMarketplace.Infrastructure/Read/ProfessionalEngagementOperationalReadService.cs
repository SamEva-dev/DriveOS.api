using DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Read;

internal sealed class ProfessionalEngagementOperationalReadService(
    ProfessionalMarketplaceDbContext db) : IProfessionalEngagementOperationalReadService
{
    public async Task<ExternalProfessionalOperationalEligibility> CheckAsync(
        OrganizationId organizationId,
        UserId userId,
        DateOnly date,
        BranchId? branchId = null,
        string? teachingCategoryCode = null,
        CancellationToken cancellationToken = default)
    {
        var profile = await db.ProfessionalProfiles.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.Status,
                x.ComplianceStatus,
                x.VerificationBadge
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
            return new(false, false, "professional-marketplace.profile.not-found", null);

        if (profile.Status != ProfessionalProfileStatus.Active)
            return new(true, false, $"professional-marketplace.profile.status.{profile.Status.ToString().ToLowerInvariant()}", null);

        if (profile.ComplianceStatus != ProfessionalComplianceStatus.Compliant ||
            profile.VerificationBadge != MarketplaceVerificationBadge.Verified)
            return new(true, false, "professional-marketplace.profile.compliance-not-verified", null);

        List<ProfessionalEngagement> candidates = await db.ProfessionalEngagements.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId
                && x.ProfessionalProfileId == profile.Id
                && x.Status == ProfessionalEngagementStatus.Active
                && x.StartsOn <= date
                && x.EndsOn >= date)
            .OrderByDescending(x => x.ActivatedAtUtc)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
            return new(true, false, "professional-marketplace.engagement.not-active", null);

        ProfessionalEngagement? engagement = candidates.FirstOrDefault(x =>
            (!x.BranchId.HasValue || !branchId.HasValue || x.BranchId.Value == branchId.Value)
            && CategoryMatches(x, teachingCategoryCode));

        if (engagement is null)
        {
            bool branchMismatch = branchId.HasValue && candidates.All(x =>
                x.BranchId.HasValue && x.BranchId.Value != branchId.Value);

            return new(
                true,
                false,
                branchMismatch
                    ? "professional-marketplace.engagement.branch-mismatch"
                    : "professional-marketplace.engagement.category-mismatch",
                null);
        }

        if (!engagement.IsOperationallyReady)
            return new(true, false, "professional-marketplace.engagement.preparation-incomplete", engagement.Id);

        return new(true, true, null, engagement.Id);
    }

    private static bool CategoryMatches(ProfessionalEngagement engagement, string? categoryCode)
    {
        if (string.IsNullOrWhiteSpace(categoryCode))
            return true;

        string normalized = categoryCode.Trim().ToUpperInvariant();
        return engagement.TermsSnapshot.TeachingCategoryCodes.Contains(
            normalized,
            StringComparer.Ordinal);
    }
}
