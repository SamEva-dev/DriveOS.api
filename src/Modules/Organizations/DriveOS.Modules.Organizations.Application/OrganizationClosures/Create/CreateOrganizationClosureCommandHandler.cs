using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.Create;

internal sealed class CreateOrganizationClosureCommandHandler(
    IOrganizationReadService organizationReadService,
    IOrganizationClosureRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IClock clock)
    : ICommandHandler<CreateOrganizationClosureCommand, OrganizationClosureId>
{
    public async Task<Result<OrganizationClosureId>> Handle(CreateOrganizationClosureCommand command, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure<OrganizationClosureId>(OrganizationClosureErrors.CurrentUserRequired);

        if (await organizationReadService.GetByIdAsync(command.OrganizationId, cancellationToken) is null)
            return Result.Failure<OrganizationClosureId>(OrganizationErrors.NotFound);

        if (await repository.HasOpenClosureAsync(command.OrganizationId, cancellationToken))
            return Result.Failure<OrganizationClosureId>(OrganizationClosureErrors.ActiveClosureAlreadyExists);

        Result<OrganizationClosure> result = OrganizationClosure.Create(
            OrganizationClosureId.New(), command.OrganizationId, command.ReasonCode,
            command.ReasonDetails, command.RequestedEffectiveAtUtc, command.DataDisposition,
            command.RetentionUntilUtc, currentUser.UserId.Value, clock.UtcNow);

        if (result.IsFailure)
            return Result.Failure<OrganizationClosureId>(result.Error);

        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}
