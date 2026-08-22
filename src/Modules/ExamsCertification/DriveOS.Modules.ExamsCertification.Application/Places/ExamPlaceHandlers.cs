using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Places;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Places;

public sealed class CreateExamCenterCommandHandler(IExamCenterRepository repository, IExamsCertificationUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<CreateExamCenterCommand, DriveOS.SharedKernel.Identifiers.ExamCenterId>
{
    public async Task<Result<DriveOS.SharedKernel.Identifiers.ExamCenterId>> Handle(CreateExamCenterCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(command.ExternalProviderCode) && !string.IsNullOrWhiteSpace(command.ExternalCenterId))
        {
            ExamCenter? existing = await repository.FindByExternalIdAsync(command.OrganizationId, command.ExternalProviderCode, command.ExternalCenterId, cancellationToken);
            if (existing is not null) return Result.Success(existing.Id);
        }

        var id = DriveOS.SharedKernel.Identifiers.ExamCenterId.New();
        Result<ExamCenter> creation = ExamCenter.Create(id, command.OrganizationId, command.Name, command.CountryCode,
            command.TimeZoneId, command.AdministrativeAreaCode, command.Address, command.ExternalProviderCode, command.ExternalCenterId);
        if (creation.IsFailure) return Result.Failure<DriveOS.SharedKernel.Identifiers.ExamCenterId>(creation.Error);

        creation.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId);
        repository.Add(creation.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(id);
    }
}

public sealed class CreateExamPlaceCommandHandler(IExamCenterRepository centerRepository, IExamPlaceRepository placeRepository,
    IExamsCertificationUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<CreateExamPlaceCommand, DriveOS.SharedKernel.Identifiers.ExamPlaceId>
{
    public async Task<Result<DriveOS.SharedKernel.Identifiers.ExamPlaceId>> Handle(CreateExamPlaceCommand command, CancellationToken cancellationToken)
    {
        ExamCenter? center = await centerRepository.GetByIdAsync(command.OrganizationId, command.ExamCenterId, cancellationToken);
        if (center is null) return Result.Failure<DriveOS.SharedKernel.Identifiers.ExamPlaceId>(ExamPlaceErrors.InvalidCenter);

        if (!string.IsNullOrWhiteSpace(command.ExternalPlaceId))
        {
            ExamPlace? existing = await placeRepository.FindByExternalIdAsync(command.OrganizationId, command.ProviderCode, command.ExternalPlaceId, cancellationToken);
            if (existing is not null) return Result.Success(existing.Id);
        }

        var id = DriveOS.SharedKernel.Identifiers.ExamPlaceId.New();
        Result<ExamPlace> creation = ExamPlace.Create(id, command.OrganizationId, command.ExamCenterId, command.ExamType,
            command.LicenseCategory, command.StartsAtUtc, command.EndsAtUtc, command.TimeZoneId, command.Source,
            command.ProviderCode, command.ExternalPlaceId, clock.UtcNow);
        if (creation.IsFailure) return Result.Failure<DriveOS.SharedKernel.Identifiers.ExamPlaceId>(creation.Error);

        creation.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId);
        placeRepository.Add(creation.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(id);
    }
}

public sealed class GetExamCentersQueryHandler(IExamCenterRepository repository) : IQueryHandler<GetExamCentersQuery, IReadOnlyList<ExamCenterResponse>>
{
    public async Task<Result<IReadOnlyList<ExamCenterResponse>>> Handle(GetExamCentersQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<ExamCenter> items = await repository.ListAsync(query.OrganizationId, cancellationToken);
        return Result.Success<IReadOnlyList<ExamCenterResponse>>(items.Select(x => new ExamCenterResponse(x.Id.Value, x.Name,
            x.CountryCode, x.TimeZoneId, x.AdministrativeAreaCode, x.Address, x.ExternalProviderCode, x.ExternalCenterId, x.Status.ToString())).ToArray());
    }
}

public sealed class GetAvailableExamPlacesQueryHandler(IExamPlaceRepository repository) : IQueryHandler<GetAvailableExamPlacesQuery, IReadOnlyList<ExamPlaceResponse>>
{
    public async Task<Result<IReadOnlyList<ExamPlaceResponse>>> Handle(GetAvailableExamPlacesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<ExamPlace> items = await repository.ListAvailableAsync(query.OrganizationId, query.FromUtc, query.ToUtc, query.LicenseCategory, cancellationToken);
        return Result.Success<IReadOnlyList<ExamPlaceResponse>>(items.Select(x => new ExamPlaceResponse(x.Id.Value, x.ExamCenterId.Value,
            x.ExamType, x.LicenseCategory, x.StartsAtUtc, x.EndsAtUtc, x.TimeZoneId, x.Source.ToString(), x.ProviderCode,
            x.ExternalPlaceId, x.Status.ToString(), x.LastObservedAtUtc, x.HoldExpiresAtUtc, x.AssignedStudentId?.Value,
            x.ExamRegistrationId?.Value)).ToArray());
    }
}
