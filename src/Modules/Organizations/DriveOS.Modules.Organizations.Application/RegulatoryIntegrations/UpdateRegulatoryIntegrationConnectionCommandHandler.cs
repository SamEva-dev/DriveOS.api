using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;

public sealed class UpdateRegulatoryIntegrationConnectionCommandHandler(
    IRegulatoryIntegrationConnectionRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateRegulatoryIntegrationConnectionCommand>
{
    public async Task<Result> Handle(UpdateRegulatoryIntegrationConnectionCommand command, CancellationToken cancellationToken)
    {
        RegulatoryIntegrationConnection? connection = await repository.GetForUpdateAsync(command.OrganizationId, command.ConnectionId, cancellationToken);
        if (connection is null) return Result.Failure(RegulatoryIntegrationConnectionErrors.NotFound);
        if (connection.Revision != command.ExpectedRevision) return Result.Failure(RegulatoryIntegrationConnectionErrors.ConcurrentUpdate);
        Result update = connection.Update(command.ExternalAccountReference, command.SecretReference);
        if (update.IsFailure) return update;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
