using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Reactivate;

internal sealed class ReactivateOrganizationSequenceCommandHandler(
    IOrganizationSequenceRepository sequenceRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReactivateOrganizationSequenceCommand>
{
    public async Task<Result> Handle(
        ReactivateOrganizationSequenceCommand command,
        CancellationToken cancellationToken)
    {
        OrganizationSequence? sequence =
            await sequenceRepository.GetForUpdateAsync(
                command.SequenceId,
                command.OrganizationId,
                cancellationToken);

        if (sequence is null)
        {
            return Result.Failure(OrganizationSequenceErrors.NotFound);
        }

        if (sequence.Revision != command.ExpectedRevision)
        {
            return Result.Failure(OrganizationSequenceErrors.ConcurrentUpdate);
        }

        Result result = sequence.Reactivate();

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
