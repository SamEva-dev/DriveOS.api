using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;

public sealed class ConfigureRegulatoryIntegrationConnectionCommandHandler(
    IOrganizationReadService organizationReadService,
    IBranchReadService branchReadService,
    IRegulatoryIntegrationConnectionRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<ConfigureRegulatoryIntegrationConnectionCommand, RegulatoryIntegrationConnectionId>
{
    public async Task<Result<RegulatoryIntegrationConnectionId>> Handle(ConfigureRegulatoryIntegrationConnectionCommand command, CancellationToken cancellationToken)
    {
        if (await organizationReadService.GetByIdAsync(command.OrganizationId, cancellationToken) is null)
            return Result.Failure<RegulatoryIntegrationConnectionId>(OrganizationErrors.NotFound);

        if (command.BranchId.HasValue && await branchReadService.GetByIdAsync(command.OrganizationId, command.BranchId.Value, cancellationToken) is null)
            return Result.Failure<RegulatoryIntegrationConnectionId>(RegulatoryIntegrationConnectionErrors.BranchNotOwned);

        if (await repository.ExistsAsync(command.OrganizationId, command.BranchId, command.CountryCode.Trim().ToUpperInvariant(), command.ProviderCode.Trim().ToLowerInvariant(), cancellationToken))
            return Result.Failure<RegulatoryIntegrationConnectionId>(RegulatoryIntegrationConnectionErrors.AlreadyExists);

        Result<RegulatoryIntegrationConnection> created = RegulatoryIntegrationConnection.Create(
            RegulatoryIntegrationConnectionId.New(), command.OrganizationId, command.BranchId,
            command.CountryCode, command.ProviderCode, command.ExternalAccountReference, command.SecretReference);
        if (created.IsFailure) return Result.Failure<RegulatoryIntegrationConnectionId>(created.Error);
        await repository.AddAsync(created.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }
}
