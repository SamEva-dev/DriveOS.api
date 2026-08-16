using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Suspend;

internal sealed class SuspendOrganizationSequenceCommandHandler(
    IOrganizationSequenceRepository sequenceRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<SuspendOrganizationSequenceCommand>
{
    public async Task<Result> Handle(
        SuspendOrganizationSequenceCommand command,
        CancellationToken cancellationToken
    )
    {
        OrganizationSequence? sequence = await sequenceRepository.GetForUpdateAsync(
            command.SequenceId,
            command.OrganizationId,
            cancellationToken
        );

        if (sequence is null)
        {
            return Result.Failure(OrganizationSequenceErrors.NotFound);
        }

        if (sequence.Revision != command.ExpectedRevision)
        {
            return Result.Failure(OrganizationSequenceErrors.ConcurrentUpdate);
        }

        Result result = sequence.Suspend();

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
