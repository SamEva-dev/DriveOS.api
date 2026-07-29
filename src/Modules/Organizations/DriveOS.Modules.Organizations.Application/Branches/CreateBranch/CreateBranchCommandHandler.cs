using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application
    .Branches.CreateBranch;

internal sealed class CreateBranchCommandHandler(
    IOrganizationRepository organizationRepository,
    IBranchRepository branchRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateBranchCommand, BranchId>
{
    public async Task<Result<BranchId>> Handle(
        CreateBranchCommand command,
        CancellationToken cancellationToken)
    {
        Organization? organization =
            await organizationRepository.GetByIdAsync(
                command.OrganizationId,
                asNoTracking: true,
                cancellationToken);

        if (organization is null)
        {
            return Result.Failure<BranchId>(
                BranchErrors.OrganizationNotFound);
        }

        if (organization.Status is
            OrganizationStatus.Suspended or
            OrganizationStatus.Closed or
            OrganizationStatus.Archived)
        {
            return Result.Failure<BranchId>(
                BranchErrors.OrganizationUnavailable);
        }

        Result<BranchName> nameResult =
            BranchName.Create(command.Name);

        if (nameResult.IsFailure)
        {
            return Result.Failure<BranchId>(nameResult.Error);
        }

        Result<BranchCode> codeResult =
            BranchCode.Create(command.Code);

        if (codeResult.IsFailure)
        {
            return Result.Failure<BranchId>(codeResult.Error);
        }

        if (await branchRepository.ExistsByNameAsync(
                command.OrganizationId,
                nameResult.Value.NormalizedValue,
                cancellationToken))
        {
            return Result.Failure<BranchId>(
                BranchErrors.DuplicateName);
        }

        if (await branchRepository.ExistsByCodeAsync(
                command.OrganizationId,
                codeResult.Value,
                cancellationToken))
        {
            return Result.Failure<BranchId>(
                BranchErrors.DuplicateCode);
        }

        Result<BranchAddress> addressResult =
            BranchAddress.Create(
                command.AddressLine1,
                command.AddressLine2,
                command.PostalCode,
                command.City,
                organization.CountryCode);

        if (addressResult.IsFailure)
        {
            return Result.Failure<BranchId>(
                addressResult.Error);
        }

        Result<Branch> branchResult = Branch.Create(
            BranchId.New(),
            command.OrganizationId,
            nameResult.Value,
            codeResult.Value,
            command.BranchType,
            addressResult.Value,
            command.TimeZoneId,
            command.IsPrimary);

        if (branchResult.IsFailure)
        {
            return Result.Failure<BranchId>(
                branchResult.Error);
        }

        if (command.IsPrimary)
        {
            Branch? currentPrimary =
                await branchRepository.GetPrimaryAsync(
                    command.OrganizationId,
                    asNoTracking: false,
                    cancellationToken);

            currentPrimary?.RemovePrimaryDesignation();
        }

        await branchRepository.AddAsync(
            branchResult.Value,
            cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(branchResult.Value.Id);
    }
}
