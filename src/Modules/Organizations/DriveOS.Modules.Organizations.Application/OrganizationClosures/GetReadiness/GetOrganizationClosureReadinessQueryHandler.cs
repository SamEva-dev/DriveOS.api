using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Models;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Readiness;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.GetReadiness;

internal sealed class GetOrganizationClosureReadinessQueryHandler(
    IOrganizationClosureRepository repository,
    IOrganizationClosureReadinessService readinessService
) : IQueryHandler<GetOrganizationClosureReadinessQuery, OrganizationClosureReadinessModel>
{
    public async Task<Result<OrganizationClosureReadinessModel>> Handle(
        GetOrganizationClosureReadinessQuery query,
        CancellationToken cancellationToken
    )
    {
        OrganizationClosure? closure = await repository.GetByIdAsync(
            query.ClosureId,
            cancellationToken
        );
        if (closure is null)
            return Result.Failure<OrganizationClosureReadinessModel>(
                OrganizationClosureErrors.NotFound
            );

        OrganizationClosureReadinessReport report = await readinessService.EvaluateAsync(
            closure.OrganizationId,
            cancellationToken
        );
        return Result.Success(OrganizationClosureReadinessModel.FromReport(report));
    }
}
