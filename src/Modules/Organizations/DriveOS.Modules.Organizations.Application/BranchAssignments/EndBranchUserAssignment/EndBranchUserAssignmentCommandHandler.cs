using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.BranchAssignments.EndBranchUserAssignment;

internal sealed class EndBranchUserAssignmentCommandHandler(
    IBranchUserAssignmentRepository assignmentRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IClock clock
) : ICommandHandler<EndBranchUserAssignmentCommand>
{
    public async Task<Result> Handle(
        EndBranchUserAssignmentCommand command,
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

        Result<BranchAssignmentReason> reasonResult = BranchAssignmentReason.Create(command.Reason);

        if (reasonResult.IsFailure)
        {
            return Result.Failure(reasonResult.Error);
        }

        DateTimeOffset now = clock.UtcNow;

        Result result = assignment.End(reasonResult.Value, now, currentUser.UserId.Value, now);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
