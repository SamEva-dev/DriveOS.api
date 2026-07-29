using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.Branches.UpdateBranch;

internal sealed class UpdateBranchCommandHandler(
    IOrganizationRepository organizationRepository,
    IBranchRepository branchRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateBranchCommand>
{
    public async Task<Result> Handle(
        UpdateBranchCommand command,
        CancellationToken cancellationToken)
    {
        Organization? organization =
            await organizationRepository.GetByIdAsync(
                command.OrganizationId,
                asNoTracking: true,
                cancellationToken);

        if (organization is null)
        {
            return Result.Failure(BranchErrors.OrganizationNotFound);
        }

        Branch? branch = await branchRepository.GetByIdAsync(
            command.BranchId,
            asNoTracking: false,
            cancellationToken);

        if (branch is null ||
            branch.OrganizationId != command.OrganizationId)
        {
            return Result.Failure(BranchErrors.NotFound);
        }

        Result<BranchName> nameResult = BranchName.Create(command.Name);

        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        if (await branchRepository.ExistsByNameAsync(
                command.OrganizationId,
                nameResult.Value.NormalizedValue,
                command.BranchId,
                cancellationToken))
        {
            return Result.Failure(BranchErrors.DuplicateName);
        }

        Result<BranchAddress> addressResult = BranchAddress.Create(
            command.AddressLine1,
            command.AddressLine2,
            command.PostalCode,
            command.City,
            organization.CountryCode);

        if (addressResult.IsFailure)
        {
            return Result.Failure(addressResult.Error);
        }

        Result updateResult = branch.UpdateGeneralInformation(
            nameResult.Value,
            command.BranchType,
            addressResult.Value,
            command.TimeZoneId);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
