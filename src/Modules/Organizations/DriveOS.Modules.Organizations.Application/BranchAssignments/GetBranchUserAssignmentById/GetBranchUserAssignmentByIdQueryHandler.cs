using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application
    .BranchAssignments.Models;
using DriveOS.Modules.Organizations.Domain
    .BranchAssignments;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application
    .BranchAssignments.GetBranchUserAssignmentById;

internal sealed class
    GetBranchUserAssignmentByIdQueryHandler(
        IBranchUserAssignmentReadService readService)
    : IQueryHandler<
        GetBranchUserAssignmentByIdQuery,
        BranchUserAssignmentItem>
{
    public async Task<
        Result<BranchUserAssignmentItem>>
        Handle(
            GetBranchUserAssignmentByIdQuery query,
            CancellationToken cancellationToken)
    {
        BranchUserAssignmentItem? item =
            await readService.GetByIdAsync(
                query.OrganizationId,
                query.AssignmentId,
                cancellationToken);

        return item is null
            ? Result.Failure<
                BranchUserAssignmentItem>(
                    BranchUserAssignmentErrors
                        .NotFound)
            : Result.Success(item);
    }
}