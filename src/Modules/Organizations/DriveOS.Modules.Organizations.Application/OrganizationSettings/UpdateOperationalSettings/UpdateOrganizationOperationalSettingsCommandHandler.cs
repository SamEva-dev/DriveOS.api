using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateOperationalSettings;

public sealed class UpdateOrganizationOperationalSettingsCommandHandler(
    IOrganizationSettingsRepository repository,
    IBranchReadService branchReadService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateOrganizationOperationalSettingsCommand>
{
    public async Task<Result> Handle(UpdateOrganizationOperationalSettingsCommand command, CancellationToken cancellationToken)
    {
        var settings = await repository.GetForUpdateAsync(command.OrganizationId, cancellationToken);
        if (settings is null) return Result.Failure(OrganizationSettingsErrors.NotFound);
        if (settings.Version != command.ExpectedVersion) return Result.Failure(OrganizationSettingsErrors.ConcurrentUpdate);

        if (command.DefaultBranchId is not null)
        {
            var branch = await branchReadService.GetByIdAsync(
                command.OrganizationId,
                command.DefaultBranchId.Value,
                cancellationToken);

            if (branch is null)
            {
                return Result.Failure(OrganizationSettingsErrors.DefaultBranchNotOwned);
            }
        }

        Result<OrganizationOperationalSettings> valueResult = OrganizationOperationalSettings.Create(
            command.DefaultSessionDurationMinutes,
            command.DefaultBookingLeadTimeMinutes,
            command.DefaultCancellationDelayHours,
            command.AllowStudentSelfBooking,
            command.RequireBranchForOperations,
            command.DefaultBranchId);
        if (valueResult.IsFailure) return Result.Failure(valueResult.Error);

        Result updateResult = settings.UpdateOperationalSettings(valueResult.Value);
        if (updateResult.IsFailure) return updateResult;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
