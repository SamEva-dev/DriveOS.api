using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Dashboard;

public sealed class GetOrganizationMarketplaceDashboardQueryHandler(
    IMarketplaceDashboardReadService readService)
    : IQueryHandler<GetOrganizationMarketplaceDashboardQuery,OrganizationMarketplaceDashboardResponse>
{
    public async Task<Result<OrganizationMarketplaceDashboardResponse>> Handle(
        GetOrganizationMarketplaceDashboardQuery q,
        CancellationToken ct)
    {
        if(q.To<q.From)
            return Result.Failure<OrganizationMarketplaceDashboardResponse>(
                Error.Validation(
                    "ProfessionalMarketplace.Dashboard.InvalidPeriod",
                    "errors.professionalMarketplace.dashboard.invalidPeriod"));

        return Result.Success(await readService.GetOrganizationAsync(q.OrganizationId,q.From,q.To,ct));
    }
}

public sealed class GetProfessionalMarketplaceDashboardQueryHandler(
    IMarketplaceDashboardReadService readService)
    : IQueryHandler<GetProfessionalMarketplaceDashboardQuery,ProfessionalMarketplaceDashboardResponse>
{
    public async Task<Result<ProfessionalMarketplaceDashboardResponse>> Handle(
        GetProfessionalMarketplaceDashboardQuery q,
        CancellationToken ct)
    {
        if(q.To<q.From)
            return Result.Failure<ProfessionalMarketplaceDashboardResponse>(
                Error.Validation(
                    "ProfessionalMarketplace.Dashboard.InvalidPeriod",
                    "errors.professionalMarketplace.dashboard.invalidPeriod"));

        return Result.Success(await readService.GetProfessionalAsync(q.ProfessionalProfileId,q.From,q.To,ct));
    }
}


public sealed class GetCurrentProfessionalMarketplaceDashboardQueryHandler(
    IProfessionalProfileRepository profiles,
    IMarketplaceDashboardReadService readService)
    : IQueryHandler<GetCurrentProfessionalMarketplaceDashboardQuery,ProfessionalMarketplaceDashboardResponse>
{
    public async Task<Result<ProfessionalMarketplaceDashboardResponse>> Handle(
        GetCurrentProfessionalMarketplaceDashboardQuery q,
        CancellationToken ct)
    {
        if(q.To<q.From)
            return Result.Failure<ProfessionalMarketplaceDashboardResponse>(
                Error.Validation(
                    "ProfessionalMarketplace.Dashboard.InvalidPeriod",
                    "errors.professionalMarketplace.dashboard.invalidPeriod"));

        ProfessionalProfile? profile=await profiles.FindByUserAsync(q.UserId,ct);
        if(profile is null || (q.ExpectedProfileId is not null && profile.Id!=q.ExpectedProfileId))
            return Result.Failure<ProfessionalMarketplaceDashboardResponse>(ProfessionalProfileErrors.NotFound);

        return Result.Success(await readService.GetProfessionalAsync(profile.Id,q.From,q.To,ct));
    }
}
