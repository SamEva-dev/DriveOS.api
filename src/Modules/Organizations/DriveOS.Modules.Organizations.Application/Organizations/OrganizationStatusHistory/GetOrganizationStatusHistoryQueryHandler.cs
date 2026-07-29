using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizationById;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.Organizations.OrganizationStatusHistory;

internal sealed class GetOrganizationStatusHistoryQueryHandler(
    IOrganizationReadService organizationReadService)
    : IQueryHandler<
        GetOrganizationStatusHistoryQuery,
        IReadOnlyList<OrganizationStatusHistoryItem>>
{
    public async Task<Result<IReadOnlyList<OrganizationStatusHistoryItem>>> Handle(
        GetOrganizationStatusHistoryQuery query,
        CancellationToken cancellationToken)
    {
        OrganizationId organizationId = new(query.OrganizationId);

        OrganizationResponse? organization =
            await organizationReadService.GetByIdAsync(
                organizationId,
                cancellationToken);

        if (organization is null)
        {
            return Result.Failure<IReadOnlyList<OrganizationStatusHistoryItem>>(
                OrganizationErrors.NotFoundById(organizationId));
        }

        IReadOnlyList<OrganizationStatusHistoryItem> history =
            await organizationReadService.GetStatusHistoryAsync(
                organizationId,
                cancellationToken);

        return Result.Success(history);
    }
}
