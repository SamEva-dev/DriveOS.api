using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ExamsCertification.Application.Providers;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Places.Sync;

public sealed record SynchronizeExamPlacesCommand(
    OrganizationId OrganizationId,
    string ProviderCode,
    string CountryCode,
    string? AdministrativeAreaCode,
    string? ExamCategory,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    IReadOnlyCollection<string>? CenterExternalIds,
    UserId ActorUserId) : ICommand<ExamPlaceSynchronizationResponse>;

public sealed record ImportExamPlacesCommand(
    OrganizationId OrganizationId,
    string ProviderCode,
    IReadOnlyCollection<ExamPlaceImportRow> Rows,
    UserId ActorUserId) : ICommand<ExamPlaceSynchronizationResponse>;

public sealed record ExamPlaceImportRow(
    string? ExternalPlaceId,
    string? ExternalCenterId,
    string CenterName,
    string CountryCode,
    string? AdministrativeAreaCode,
    string ExamType,
    string LicenseCategory,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? TimeZoneId,
    int Capacity = 1,
    int AvailableCapacity = 1,
    string? Address = null);

public sealed record ExamPlaceSynchronizationResponse(
    string ProviderCode,
    DateTimeOffset SynchronizedAtUtc,
    int ExternalSlotsRead,
    int CentersCreated,
    int CentersUpdated,
    int PlacesCreated,
    int PlacesUpdated,
    int PlacesReactivated,
    int PlacesMarkedUnavailable,
    int PlacesPreservedBecauseAssigned,
    IReadOnlyList<Guid> NewlyAvailablePlaceIds,
    IReadOnlyList<Guid> ObservedAvailablePlaceIds,
    IReadOnlyList<string> Warnings);

internal sealed record ExamPlaceSynchronizationInput(
    string ProviderCode,
    IReadOnlyCollection<ExternalExamPlace> Places,
    DateTimeOffset ScopeFromUtc,
    DateTimeOffset ScopeToUtc,
    string? ScopeExamCategory,
    IReadOnlyCollection<string>? ScopeCenterExternalIds,
    bool MarkMissingAsUnavailable,
    DriveOS.Modules.ExamsCertification.Domain.Providers.ExamPlaceSource Source,
    UserId ActorUserId);
