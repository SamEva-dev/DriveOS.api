using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;

public sealed class GetRegulatoryIntegrationConnectionsQueryHandler(IRegulatoryIntegrationConnectionReadService readService)
    : IQueryHandler<GetRegulatoryIntegrationConnectionsQuery, IReadOnlyList<RegulatoryIntegrationConnectionResponse>>
{
    public async Task<Result<IReadOnlyList<RegulatoryIntegrationConnectionResponse>>> Handle(GetRegulatoryIntegrationConnectionsQuery query, CancellationToken cancellationToken)
        => Result.Success(await readService.GetAsync(query.OrganizationId, cancellationToken));
}
