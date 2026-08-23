using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;

public sealed class ChangeRegulatoryIntegrationConnectionStatusCommandHandler(IRegulatoryIntegrationConnectionRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeRegulatoryIntegrationConnectionStatusCommand>
{
    public async Task<Result> Handle(ChangeRegulatoryIntegrationConnectionStatusCommand command, CancellationToken cancellationToken)
    {
        RegulatoryIntegrationConnection? connection = await repository.GetForUpdateAsync(command.OrganizationId, command.ConnectionId, cancellationToken);
        if (connection is null) return Result.Failure(RegulatoryIntegrationConnectionErrors.NotFound);
        if (connection.Revision != command.ExpectedRevision) return Result.Failure(RegulatoryIntegrationConnectionErrors.ConcurrentUpdate);
        Result result = command.Status switch
        {
            RegulatoryIntegrationConnectionStatus.Active => connection.Activate(),
            RegulatoryIntegrationConnectionStatus.Suspended => connection.Suspend(),
            RegulatoryIntegrationConnectionStatus.Ended => connection.End(),
            _ => Result.Failure(RegulatoryIntegrationConnectionErrors.Ended)
        };
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
