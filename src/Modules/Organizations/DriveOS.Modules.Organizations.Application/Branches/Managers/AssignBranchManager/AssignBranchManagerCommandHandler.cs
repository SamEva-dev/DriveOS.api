using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application
    .Branches.Managers.AssignBranchManager;

internal sealed class
    AssignBranchManagerCommandHandler(
        IOrganizationRepository
            organizationRepository,
        IBranchRepository branchRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IClock clock)
    : ICommandHandler<
        AssignBranchManagerCommand>
{
    public async Task<Result> Handle(
        AssignBranchManagerCommand command,
        CancellationToken cancellationToken)
    {
        if (
            !currentUser.IsAuthenticated ||
            currentUser.UserId is null)
        {
            return Result.Failure(
                OrganizationErrors
                    .CurrentUserRequired);
        }

        Organization? organization =
            await organizationRepository
                .GetByIdAsync(
                    command.OrganizationId,
                    asNoTracking: true,
                    cancellationToken);

        if (organization is null)
        {
            return Result.Failure(
                BranchErrors
                    .OrganizationNotFound);
        }

        Branch? branch =
            await branchRepository
                .GetByIdAsync(
                    command.BranchId,
                    asNoTracking: false,
                    cancellationToken);

        if (
            branch is null ||
            branch.OrganizationId !=
            command.OrganizationId)
        {
            return Result.Failure(
                BranchErrors.NotFound);
        }

        DateTimeOffset now =
            clock.UtcNow;

        Result assignmentResult =
    branch.AssignPrimaryManager(
        command.ManagerUserId,
        now,
        currentUser.UserId.Value,
        now);

        if (assignmentResult.IsFailure)
        {
            return assignmentResult;
        }

        await unitOfWork.CommitAsync(
            cancellationToken);

        return Result.Success();
    }
}