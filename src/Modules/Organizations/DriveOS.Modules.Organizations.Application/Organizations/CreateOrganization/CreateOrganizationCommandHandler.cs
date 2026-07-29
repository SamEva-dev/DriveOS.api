using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application
    .Organizations.CreateOrganization;

public sealed class CreateOrganizationCommandHandler
    : ICommandHandler<CreateOrganizationCommand, OrganizationId>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrganizationId>> Handle(
        CreateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        string normalizedLegalName =
            command.LegalName.Trim();

        string normalizedCountryCode =
            command.CountryCode.Trim().ToUpperInvariant();

        bool alreadyExists =
            await _organizationRepository.ExistsByLegalNameAsync(
                normalizedLegalName,
                normalizedCountryCode,
                cancellationToken);

        if (alreadyExists)
        {
            return Result.Failure<OrganizationId>(
                OrganizationErrors.LegalNameAlreadyExists);
        }

        var organizationType =
            (OrganizationType)command.OrganizationType;

        Result<Organization> creationResult =
            Organization.Create(
                OrganizationId.New(),
                normalizedLegalName,
                normalizedCountryCode,
                organizationType);

        if (creationResult.IsFailure)
        {
            return Result.Failure<OrganizationId>(
                creationResult.Error);
        }

        Organization organization =
            creationResult.Value;

        await _organizationRepository.AddAsync(
            organization,
            cancellationToken);

        await _unitOfWork.CommitAsync(
            cancellationToken);

        return Result.Success(organization.Id);
    }
}