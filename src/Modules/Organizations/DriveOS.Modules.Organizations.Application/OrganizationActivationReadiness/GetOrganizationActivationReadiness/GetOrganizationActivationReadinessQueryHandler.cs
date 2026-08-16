using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.GetOrganizationActivationReadiness;

internal sealed class GetOrganizationActivationReadinessQueryHandler(
    IOrganizationRepository organizationRepository,
    IOrganizationActivationReadinessService readinessService,
    IOrganizationActivationReadinessReportCache readinessCache
) : IQueryHandler<GetOrganizationActivationReadinessQuery, OrganizationActivationReadinessReport>
{
    public async Task<Result<OrganizationActivationReadinessReport>> Handle(
        GetOrganizationActivationReadinessQuery query,
        CancellationToken cancellationToken
    )
    {
        Organization? organization = await organizationRepository.GetByIdAsync(
            query.OrganizationId,
            asNoTracking: true,
            cancellationToken
        );

        if (organization is null)
        {
            return Result.Failure<OrganizationActivationReadinessReport>(
                OrganizationErrors.NotFoundById(query.OrganizationId)
            );
        }

        OrganizationActivationReadinessReport report = await readinessCache.GetOrCreateAsync(
            query.OrganizationId,
            token => readinessService.EvaluateAsync(query.OrganizationId, token),
            cancellationToken
        );

        return Result.Success(report);
    }
}
