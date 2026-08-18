using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.BranchAssignments.CreateBranchUserAssignment;

internal sealed class CreateBranchUserAssignmentCommandHandler(
    IOrganizationRepository organizationRepository,
    IBranchRepository branchRepository,
    IBranchUserAssignmentRepository assignmentRepository,
    IOrganizationActivationReadinessCacheInvalidator readinessCacheInvalidator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IClock clock
) : ICommandHandler<CreateBranchUserAssignmentCommand, BranchUserAssignmentId>
{
    public async Task<Result<BranchUserAssignmentId>> Handle(
        CreateBranchUserAssignmentCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure<BranchUserAssignmentId>(OrganizationErrors.CurrentUserRequired);
        }

        Organization? organization = await organizationRepository.GetByIdAsync(
            command.OrganizationId,
            asNoTracking: true,
            cancellationToken
        );

        if (organization is null)
        {
            return Result.Failure<BranchUserAssignmentId>(
                BranchUserAssignmentErrors.OrganizationNotFound
            );
        }

        if (
            organization.Status
            is OrganizationStatus.Suspended
                or OrganizationStatus.Closed
                or OrganizationStatus.Archived
        )
        {
            return Result.Failure<BranchUserAssignmentId>(BranchErrors.OrganizationUnavailable);
        }

        Branch? branch = await branchRepository.GetByIdAsync(
            command.BranchId,
            asNoTracking: true,
            cancellationToken
        );

        if (branch is null || branch.OrganizationId != command.OrganizationId)
        {
            return Result.Failure<BranchUserAssignmentId>(
                BranchUserAssignmentErrors.BranchNotFound
            );
        }

        if (branch.Status == BranchStatus.Closed)
        {
            return Result.Failure<BranchUserAssignmentId>(BranchUserAssignmentErrors.ClosedBranch);
        }

        bool duplicateExists = await assignmentRepository.HasOpenAssignmentAsync(
            command.OrganizationId,
            command.BranchId,
            command.UserId,
            command.Role,
            cancellationToken
        );

        if (duplicateExists)
        {
            return Result.Failure<BranchUserAssignmentId>(
                BranchUserAssignmentErrors.DuplicateActiveAssignment
            );
        }

        if (command.AssignmentType == BranchAssignmentType.Primary)
        {
            bool primaryExists = await assignmentRepository.HasPrimaryAssignmentAsync(
                command.OrganizationId,
                command.UserId,
                cancellationToken
            );

            if (primaryExists)
            {
                return Result.Failure<BranchUserAssignmentId>(
                    BranchUserAssignmentErrors.PrimaryAssignmentAlreadyExists
                );
            }
        }

        DateTimeOffset now = clock.UtcNow;

        Result<BranchUserAssignment> assignmentResult = BranchUserAssignment.Create(
            BranchUserAssignmentId.New(),
            command.OrganizationId,
            command.BranchId,
            command.UserId,
            command.Role,
            command.AssignmentType,
            now,
            command.PlannedEndAtUtc,
            currentUser.UserId.Value,
            now
        );

        if (assignmentResult.IsFailure)
        {
            return Result.Failure<BranchUserAssignmentId>(assignmentResult.Error);
        }

        await assignmentRepository.AddAsync(assignmentResult.Value, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
        readinessCacheInvalidator.Invalidate(command.OrganizationId);

        return Result.Success(assignmentResult.Value.Id);
    }
}
