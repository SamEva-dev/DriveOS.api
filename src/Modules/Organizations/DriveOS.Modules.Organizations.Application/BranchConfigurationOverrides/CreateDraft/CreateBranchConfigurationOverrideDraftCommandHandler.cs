using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.CreateDraft;

public sealed class CreateBranchConfigurationOverrideDraftCommandHandler(
    IBranchReadService branchReadService,
    IOrganizationConfigurationReadService organizationConfigurationReadService,
    IBranchConfigurationOverrideRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<CreateBranchConfigurationOverrideDraftCommand, BranchConfigurationOverrideId>
{
    public async Task<Result<BranchConfigurationOverrideId>> Handle(
        CreateBranchConfigurationOverrideDraftCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure<BranchConfigurationOverrideId>(
                BranchConfigurationOverrideErrors.CurrentUserRequired
            );

        var branch = await branchReadService.GetByIdAsync(
            command.OrganizationId,
            command.BranchId,
            cancellationToken
        );

        if (branch is null)
            return Result.Failure<BranchConfigurationOverrideId>(BranchErrors.NotFound);

        var baseConfiguration = await organizationConfigurationReadService.GetByIdAsync(
            command.OrganizationId,
            command.BaseConfigurationId,
            cancellationToken
        );

        if (baseConfiguration is null)
            return Result.Failure<BranchConfigurationOverrideId>(
                BranchConfigurationOverrideErrors.BaseConfigurationNotFound
            );

        if (baseConfiguration.Status != (int)OrganizationConfigurationStatus.Published)
            return Result.Failure<BranchConfigurationOverrideId>(
                BranchConfigurationOverrideErrors.BaseConfigurationMustBePublished
            );

        if (
            !string.Equals(
                baseConfiguration.CountryCode,
                command.CountryCode,
                StringComparison.OrdinalIgnoreCase
            )
        )
            return Result.Failure<BranchConfigurationOverrideId>(
                BranchConfigurationOverrideErrors.CountryCodeMismatch
            );

        if (
            await repository.VersionExistsAsync(
                command.OrganizationId,
                command.BranchId,
                command.VersionNumber,
                cancellationToken
            )
        )
            return Result.Failure<BranchConfigurationOverrideId>(
                BranchConfigurationOverrideErrors.VersionAlreadyExists
            );

        Result<BranchOverridePayload> payloadResult = BranchOverridePayload.Create(
            command.PayloadJson
        );
        if (payloadResult.IsFailure)
            return Result.Failure<BranchConfigurationOverrideId>(payloadResult.Error);

        Result<BranchConfigurationOverride> creationResult =
            BranchConfigurationOverride.CreateDraft(
                BranchConfigurationOverrideId.New(),
                command.OrganizationId,
                command.BranchId,
                command.BaseConfigurationId,
                command.VersionNumber,
                command.CountryCode,
                payloadResult.Value
            );

        if (creationResult.IsFailure)
            return Result.Failure<BranchConfigurationOverrideId>(creationResult.Error);

        await repository.AddAsync(creationResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(creationResult.Value.Id);
    }
}
