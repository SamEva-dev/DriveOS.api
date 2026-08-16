using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Reserve;

internal sealed class ReserveOrganizationSequenceNumberCommandHandler(
    IOrganizationSequenceNumberGenerator numberGenerator
) : ICommandHandler<ReserveOrganizationSequenceNumberCommand, string>
{
    public Task<Result<string>> Handle(
        ReserveOrganizationSequenceNumberCommand command,
        CancellationToken cancellationToken
    ) =>
        numberGenerator.ReserveNextAsync(
            command.OrganizationId,
            command.BranchId,
            command.Code,
            cancellationToken
        );
}
