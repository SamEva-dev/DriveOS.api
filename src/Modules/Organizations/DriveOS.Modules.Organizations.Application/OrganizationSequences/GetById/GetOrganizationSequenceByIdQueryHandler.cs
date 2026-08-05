using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.GetById;

internal sealed class GetOrganizationSequenceByIdQueryHandler(
    IOrganizationSequenceReadService readService)
    : IQueryHandler<GetOrganizationSequenceByIdQuery, OrganizationSequenceResponse>
{
    public async Task<Result<OrganizationSequenceResponse>> Handle(
        GetOrganizationSequenceByIdQuery query,
        CancellationToken cancellationToken)
    {
        OrganizationSequenceResponse? sequence = await readService.GetByIdAsync(
            query.OrganizationId,
            query.SequenceId,
            cancellationToken);

        return sequence is null
            ? Result.Failure<OrganizationSequenceResponse>(OrganizationSequenceErrors.NotFound)
            : Result.Success(sequence);
    }
}
