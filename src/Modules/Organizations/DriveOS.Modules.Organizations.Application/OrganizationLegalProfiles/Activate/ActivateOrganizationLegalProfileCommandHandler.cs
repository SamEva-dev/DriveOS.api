using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;
using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Activate;

internal sealed class ActivateOrganizationLegalProfileCommandHandler(
    IOrganizationLegalProfileRepository repository,
    IOrganizationLegalProfileComplianceService complianceService,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<ActivateOrganizationLegalProfileCommand>
{
    public async Task<Result> Handle(ActivateOrganizationLegalProfileCommand command, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure(OrganizationLegalProfileErrors.CurrentUserRequired);

        OrganizationLegalProfile? profile = await repository.GetForUpdateAsync(command.OrganizationId, cancellationToken);
        if (profile is null)
            return Result.Failure(OrganizationLegalProfileErrors.NotFound);
        if (profile.Revision != command.ExpectedRevision)
            return Result.Failure(OrganizationLegalProfileErrors.ConcurrentUpdate);

        OrganizationLegalProfileComplianceResult compliance = complianceService.Validate(profile);
        if (!compliance.IsCompliant)
            return Result.Failure(OrganizationLegalProfileComplianceErrors.ActivationBlocked(compliance.Issues));

        Result result = profile.Activate();
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
