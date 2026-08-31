using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Dashboard;

public interface IMarketplaceDashboardReadService
{
    Task<OrganizationMarketplaceDashboardResponse> GetOrganizationAsync(
        OrganizationId organizationId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<ProfessionalMarketplaceDashboardResponse> GetProfessionalAsync(
        ProfessionalProfileId professionalProfileId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}
