using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.BranchAssignments.ReactivateBranchUserAssignment;

internal sealed class ReactivateBranchUserAssignmentCommandHandler(
    IBranchUserAssignmentRepository assignmentRepository,
    IOrganizationActivationReadinessCacheInvalidator readinessCacheInvalidator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IClock clock
) : ICommandHandler<ReactivateBranchUserAssignmentCommand>
{
    public async Task<Result> Handle(
        ReactivateBranchUserAssignmentCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure(OrganizationErrors.CurrentUserRequired);
        }

        BranchUserAssignment? assignment = await assignmentRepository.GetByIdAsync(
            command.AssignmentId,
            asNoTracking: false,
            cancellationToken
        );

        if (assignment is null || assignment.OrganizationId != command.OrganizationId)
        {
            return Result.Failure(BranchUserAssignmentErrors.NotFound);
        }

        bool duplicateExists = await assignmentRepository.HasAnotherOpenAssignmentAsync(
            command.OrganizationId,
            assignment.BranchId,
            assignment.UserId,
            assignment.Role,
            assignment.Id,
            cancellationToken
        );

        if (duplicateExists)
        {
            return Result.Failure(BranchUserAssignmentErrors.DuplicateActiveAssignment);
        }

        if (assignment.AssignmentType == BranchAssignmentType.Primary)
        {
            bool primaryExists = await assignmentRepository.HasAnotherPrimaryAssignmentAsync(
                command.OrganizationId,
                assignment.UserId,
                assignment.Id,
                cancellationToken
            );

            if (primaryExists)
            {
                return Result.Failure(BranchUserAssignmentErrors.PrimaryAssignmentAlreadyExists);
            }
        }

        Result<BranchAssignmentReason> reasonResult = BranchAssignmentReason.Create(command.Reason);

        if (reasonResult.IsFailure)
        {
            return Result.Failure(reasonResult.Error);
        }

        Result result = assignment.Reactivate(
            reasonResult.Value,
            currentUser.UserId.Value,
            clock.UtcNow
        );

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        readinessCacheInvalidator.Invalidate(command.OrganizationId);

        return Result.Success();
    }
}
