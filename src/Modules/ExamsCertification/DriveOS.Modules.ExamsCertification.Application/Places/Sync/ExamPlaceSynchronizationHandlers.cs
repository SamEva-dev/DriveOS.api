using System.Security.Cryptography;
using System.Text;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Application.Providers;
using DriveOS.Modules.ExamsCertification.Application.Providers.Connections;
using DriveOS.Modules.ExamsCertification.Domain.Places;
using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Places.Sync;

public sealed class SynchronizeExamPlacesCommandHandler(
    IExamPlaceProviderResolver providerResolver,
    IExamCenterRepository centerRepository,
    IExamPlaceRepository placeRepository,
    IExamsCertificationUnitOfWork unitOfWork,
    IExamProviderExecutionGuard executionGuard,
    IClock clock)
    : ICommandHandler<SynchronizeExamPlacesCommand, ExamPlaceSynchronizationResponse>
{
    public async Task<Result<ExamPlaceSynchronizationResponse>> Handle(
        SynchronizeExamPlacesCommand command,
        CancellationToken cancellationToken)
    {
        if (command.ToUtc <= command.FromUtc)
            return Result.Failure<ExamPlaceSynchronizationResponse>(ExamPlaceSynchronizationErrors.InvalidPeriod);
        if (string.IsNullOrWhiteSpace(command.ProviderCode))
            return Result.Failure<ExamPlaceSynchronizationResponse>(ExamPlaceSynchronizationErrors.InvalidProvider);

        IExamPlaceProvider? provider = providerResolver.Resolve(command.ProviderCode);
        if (provider is null || !provider.Descriptor.IsEnabled)
            return Result.Failure<ExamPlaceSynchronizationResponse>(ExamPlaceSynchronizationErrors.ProviderNotFound);

        if (!provider.Descriptor.Capabilities.HasFlag(ExamPlaceProviderCapability.ReadAvailablePlaces))
            return Result.Failure<ExamPlaceSynchronizationResponse>(ExamPlaceSynchronizationErrors.ProviderDoesNotExposeAvailability);

        var request = new ExamPlaceAvailabilityRequest(
            command.OrganizationId,
            command.CountryCode,
            command.AdministrativeAreaCode,
            command.ExamCategory,
            command.FromUtc,
            command.ToUtc,
            command.CenterExternalIds);

        IReadOnlyCollection<ExternalExamPlace> places;
        try
        {
            places = await executionGuard.ExecuteAsync(command.OrganizationId, provider.Descriptor.Code,
                ct => provider.GetAvailablePlacesAsync(request, ct), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Result.Failure<ExamPlaceSynchronizationResponse>(ExamPlaceSynchronizationErrors.ProviderUnavailable(provider.Descriptor.Code));
        }

        return await SynchronizeAsync(
            command.OrganizationId,
            new ExamPlaceSynchronizationInput(provider.Descriptor.Code, places, command.FromUtc, command.ToUtc, command.ExamCategory,
                command.CenterExternalIds, true, ExamPlaceSource.ExternalProvider, command.ActorUserId),
            centerRepository,
            placeRepository,
            unitOfWork,
            clock.UtcNow,
            cancellationToken);
    }

    internal static async Task<Result<ExamPlaceSynchronizationResponse>> SynchronizeAsync(
        OrganizationId organizationId,
        ExamPlaceSynchronizationInput input,
        IExamCenterRepository centerRepository,
        IExamPlaceRepository placeRepository,
        IExamsCertificationUnitOfWork unitOfWork,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.ProviderCode))
            return Result.Failure<ExamPlaceSynchronizationResponse>(ExamPlaceSynchronizationErrors.InvalidProvider);
        if (input.ScopeToUtc <= input.ScopeFromUtc)
            return Result.Failure<ExamPlaceSynchronizationResponse>(ExamPlaceSynchronizationErrors.InvalidPeriod);

        int centersCreated = 0;
        int centersUpdated = 0;
        int placesCreated = 0;
        int placesUpdated = 0;
        int placesReactivated = 0;
        int placesMarkedUnavailable = 0;
        int placesPreservedBecauseAssigned = 0;
        var warnings = new List<string>();
        var newlyAvailablePlaceIds = new List<Guid>();
        var observedAvailablePlaceIds = new List<Guid>();
        var observedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var centerCache = new Dictionary<string, ExamCenter>(StringComparer.OrdinalIgnoreCase);
        var placeCache = new Dictionary<string, ExamPlace>(StringComparer.OrdinalIgnoreCase);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (ExternalExamPlace external in input.Places)
            {
                Result validation = ValidateExternal(external);
                if (validation.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<ExamPlaceSynchronizationResponse>(validation.Error);
                }

                string externalCenterId = NormalizeExternalCenterId(external);
                if (!centerCache.TryGetValue(externalCenterId, out ExamCenter? center))
                {
                    center = await centerRepository.FindByExternalIdForUpdateAsync(
                        organizationId, input.ProviderCode, externalCenterId, cancellationToken);

                    if (center is null)
                    {
                        Result<ExamCenter> createdCenter = ExamCenter.Create(
                            ExamCenterId.New(), organizationId, external.CenterName, external.CountryCode,
                            external.TimeZoneId ?? "UTC", external.AdministrativeAreaCode,
                            GetMetadata(external, "address"), input.ProviderCode, externalCenterId);
                        if (createdCenter.IsFailure)
                        {
                            await unitOfWork.RollbackTransactionAsync(cancellationToken);
                            return Result.Failure<ExamPlaceSynchronizationResponse>(createdCenter.Error);
                        }

                        center = createdCenter.Value;
                        center.SetCreatedAudit(nowUtc, input.ActorUserId);
                        centerRepository.Add(center);
                        centersCreated++;
                    }

                    centerCache[externalCenterId] = center;
                }

                if (center.SynchronizeExternalProfile(
                    external.CenterName,
                    external.CountryCode,
                    external.TimeZoneId ?? center.TimeZoneId,
                    external.AdministrativeAreaCode,
                    GetMetadata(external, "address"),
                    nowUtc,
                    input.ActorUserId))
                {
                    centersUpdated++;
                }

                int capacity = Math.Max(1, external.Capacity);
                int availableCapacity = Math.Clamp(external.AvailableCapacity, 0, capacity);
                for (int ordinal = 1; ordinal <= availableCapacity; ordinal++)
                {
                    string externalUnitId = MaterializeExternalUnitId(external.ExternalPlaceId, capacity, ordinal);
                    observedKeys.Add(externalUnitId);

                    if (!placeCache.TryGetValue(externalUnitId, out ExamPlace? place))
                    {
                        place = await placeRepository.FindByExternalIdForUpdateAsync(
                            organizationId, input.ProviderCode, externalUnitId, cancellationToken);

                        if (place is null)
                        {
                            Result<ExamPlace> creation = ExamPlace.Create(
                                ExamPlaceId.New(), organizationId, center.Id,
                                external.ExamType, external.ExamCategory,
                                external.StartsAtUtc, external.EndsAtUtc,
                                external.TimeZoneId ?? center.TimeZoneId,
                                input.Source, input.ProviderCode,
                                externalUnitId, nowUtc);
                            if (creation.IsFailure)
                            {
                                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                                return Result.Failure<ExamPlaceSynchronizationResponse>(creation.Error);
                            }

                            place = creation.Value;
                            place.SetCreatedAudit(nowUtc, input.ActorUserId);
                            placeRepository.Add(place);
                            placesCreated++;
                            newlyAvailablePlaceIds.Add(place.Id.Value);
                            observedAvailablePlaceIds.Add(place.Id.Value);
                            placeCache[externalUnitId] = place;
                            continue;
                        }

                        placeCache[externalUnitId] = place;
                    }

                    ExamPlaceStatus before = place.Status;
                    bool changed = place.SynchronizeExternalAvailability(
                        center.Id,
                        external.ExamType,
                        external.ExamCategory,
                        external.StartsAtUtc,
                        external.EndsAtUtc,
                        external.TimeZoneId ?? center.TimeZoneId,
                        nowUtc,
                        input.ActorUserId);

                    if (changed) placesUpdated++;
                    if (before == ExamPlaceStatus.Expired && place.Status == ExamPlaceStatus.Available)
                    {
                        placesReactivated++;
                        newlyAvailablePlaceIds.Add(place.Id.Value);
                    }
                    if (place.Status == ExamPlaceStatus.Available)
                        observedAvailablePlaceIds.Add(place.Id.Value);
                }

                if (availableCapacity < capacity)
                    warnings.Add($"{external.ExternalPlaceId}: availableCapacity={availableCapacity}, capacity={capacity}");
            }

            if (input.MarkMissingAsUnavailable)
            {
                IReadOnlyList<ExamPlace> existing = await placeRepository.ListExternalForUpdateAsync(
                    organizationId, input.ProviderCode, input.ScopeFromUtc, input.ScopeToUtc, input.ScopeExamCategory,
                    input.ScopeCenterExternalIds, cancellationToken);

                foreach (ExamPlace place in existing)
                {
                    if (string.IsNullOrWhiteSpace(place.ExternalPlaceId) || observedKeys.Contains(place.ExternalPlaceId))
                        continue;

                    ExamPlaceStatus before = place.Status;
                    bool changed = place.MarkUnavailableFromProvider(nowUtc, input.ActorUserId);
                    if (changed)
                        placesMarkedUnavailable++;
                    else if (before is ExamPlaceStatus.Assigned or ExamPlaceStatus.Confirmed or ExamPlaceStatus.Consumed)
                        placesPreservedBecauseAssigned++;
                }
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(new ExamPlaceSynchronizationResponse(
                input.ProviderCode,
                nowUtc,
                input.Places.Count,
                centersCreated,
                centersUpdated,
                placesCreated,
                placesUpdated,
                placesReactivated,
                placesMarkedUnavailable,
                placesPreservedBecauseAssigned,
                newlyAvailablePlaceIds,
                observedAvailablePlaceIds.Distinct().ToArray(),
                warnings));
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static Result ValidateExternal(ExternalExamPlace external)
    {
        if (string.IsNullOrWhiteSpace(external.ExternalPlaceId)
            || string.IsNullOrWhiteSpace(external.CenterName)
            || string.IsNullOrWhiteSpace(external.CountryCode)
            || string.IsNullOrWhiteSpace(external.ExamType)
            || string.IsNullOrWhiteSpace(external.ExamCategory)
            || external.EndsAtUtc <= external.StartsAtUtc
            || external.Capacity < 1
            || external.AvailableCapacity < 0
            || external.AvailableCapacity > external.Capacity)
        {
            return Result.Failure(ExamPlaceSynchronizationErrors.InvalidExternalPlace);
        }

        return Result.Success();
    }

    private static string NormalizeExternalCenterId(ExternalExamPlace external)
    {
        if (!string.IsNullOrWhiteSpace(external.ExternalCenterId)) return external.ExternalCenterId.Trim();
        return "center:" + StableHash($"{external.CountryCode}|{external.AdministrativeAreaCode}|{external.CenterName}");
    }

    internal static string MaterializeExternalUnitId(string externalPlaceId, int capacity, int ordinal) =>
        capacity <= 1 ? externalPlaceId.Trim() : $"{externalPlaceId.Trim()}#unit:{ordinal}";

    internal static string StableHash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant()[..24];

    private static string? GetMetadata(ExternalExamPlace external, string key) =>
        external.Metadata is not null && external.Metadata.TryGetValue(key, out string? value) ? value : null;
}

public sealed class ImportExamPlacesCommandHandler(
    IExamCenterRepository centerRepository,
    IExamPlaceRepository placeRepository,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock)
    : ICommandHandler<ImportExamPlacesCommand, ExamPlaceSynchronizationResponse>
{
    public Task<Result<ExamPlaceSynchronizationResponse>> Handle(ImportExamPlacesCommand command, CancellationToken cancellationToken)
    {
        if (command.Rows.Count == 0)
            return Task.FromResult(Result.Failure<ExamPlaceSynchronizationResponse>(ExamPlaceSynchronizationErrors.EmptyImport));

        string providerCode = string.IsNullOrWhiteSpace(command.ProviderCode) ? "file-import" : command.ProviderCode.Trim();
        IReadOnlyCollection<ExternalExamPlace> external = command.Rows.Select(row =>
        {
            string externalCenterId = !string.IsNullOrWhiteSpace(row.ExternalCenterId)
                ? row.ExternalCenterId.Trim()
                : "center:" + SynchronizeExamPlacesCommandHandler.StableHash($"{row.CountryCode}|{row.AdministrativeAreaCode}|{row.CenterName}");

            string externalPlaceId = !string.IsNullOrWhiteSpace(row.ExternalPlaceId)
                ? row.ExternalPlaceId.Trim()
                : "place:" + SynchronizeExamPlacesCommandHandler.StableHash($"{externalCenterId}|{row.ExamType}|{row.LicenseCategory}|{row.StartsAtUtc:O}|{row.EndsAtUtc:O}");

            Dictionary<string, string>? metadata = string.IsNullOrWhiteSpace(row.Address)
                ? null
                : new Dictionary<string, string> { ["address"] = row.Address.Trim() };

            return new ExternalExamPlace(
                externalPlaceId,
                externalCenterId,
                row.CenterName,
                row.CountryCode,
                row.AdministrativeAreaCode,
                row.ExamType,
                row.LicenseCategory,
                row.StartsAtUtc,
                row.EndsAtUtc,
                row.Capacity,
                row.AvailableCapacity,
                row.TimeZoneId,
                metadata);
        }).ToArray();

        DateTimeOffset fromUtc = external.Min(x => x.StartsAtUtc);
        DateTimeOffset toUtc = external.Max(x => x.EndsAtUtc).AddTicks(1);

        return SynchronizeExamPlacesCommandHandler.SynchronizeAsync(
            command.OrganizationId,
            new ExamPlaceSynchronizationInput(providerCode, external, fromUtc, toUtc, null, null, false,
                ExamPlaceSource.ImportedFile, command.ActorUserId),
            centerRepository,
            placeRepository,
            unitOfWork,
            clock.UtcNow,
            cancellationToken);
    }
}
