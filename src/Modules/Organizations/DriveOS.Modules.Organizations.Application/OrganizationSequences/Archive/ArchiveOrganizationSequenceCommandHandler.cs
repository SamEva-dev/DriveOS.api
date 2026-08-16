using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Archive;

internal sealed class ArchiveOrganizationSequenceCommandHandler(
    IOrganizationSequenceRepository sequenceRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<ArchiveOrganizationSequenceCommand>
{
    public async Task<Result> Handle(
        ArchiveOrganizationSequenceCommand command,
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

        Result result = sequence.Archive();

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
