using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Create;

internal sealed class CreateOrganizationSequenceCommandHandler(
    IOrganizationReadService organizationReadService,
    IBranchReadService branchReadService,
    IOrganizationSequenceRepository sequenceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<CreateOrganizationSequenceCommand, OrganizationSequenceId>
{
    public async Task<Result<OrganizationSequenceId>> Handle(
        CreateOrganizationSequenceCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure<OrganizationSequenceId>(
                OrganizationSequenceErrors.CurrentUserRequired);
        }

        var organization = await organizationReadService.GetByIdAsync(
            command.OrganizationId,
            cancellationToken);

        if (organization is null)
        {
            return Result.Failure<OrganizationSequenceId>(OrganizationErrors.NotFound);
        }

        if (organization.Status is "Suspended" or "Closed" or "Archived")
        {
            return Result.Failure<OrganizationSequenceId>(
                OrganizationSequenceErrors.OrganizationUnavailable);
        }

        if (command.Scope == OrganizationSequenceScope.Branch)
        {
            if (command.BranchId is null)
            {
                return Result.Failure<OrganizationSequenceId>(
                    OrganizationSequenceErrors.BranchRequired);
            }

            var branch = await branchReadService.GetByIdAsync(
                command.OrganizationId,
                command.BranchId.Value,
                cancellationToken);

            if (branch is null)
            {
                return Result.Failure<OrganizationSequenceId>(
                    OrganizationSequenceErrors.BranchNotFound);
            }
        }

        string normalizedCode = command.Code.Trim().ToUpperInvariant();

        if (await sequenceRepository.ExistsAsync(
                command.OrganizationId,
                command.BranchId,
                normalizedCode,
                cancellationToken))
        {
            return Result.Failure<OrganizationSequenceId>(
                OrganizationSequenceErrors.AlreadyExists);
        }

        Result<SequencePattern> patternResult =
            SequencePattern.Create(command.Pattern);

        if (patternResult.IsFailure)
        {
            return Result.Failure<OrganizationSequenceId>(patternResult.Error);
        }

        Result<OrganizationSequence> sequenceResult = OrganizationSequence.Create(
            OrganizationSequenceId.New(),
            command.OrganizationId,
            command.BranchId,
            command.Scope,
            normalizedCode,
            patternResult.Value,
            command.Padding,
            command.InitialValue,
            command.ResetPolicy);

        if (sequenceResult.IsFailure)
        {
            return Result.Failure<OrganizationSequenceId>(sequenceResult.Error);
        }

        await sequenceRepository.AddAsync(sequenceResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(sequenceResult.Value.Id);
    }
}
