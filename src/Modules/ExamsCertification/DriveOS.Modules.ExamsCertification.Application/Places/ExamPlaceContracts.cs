using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Places;

public sealed record CreateExamCenterCommand(
    OrganizationId OrganizationId,
    string Name,
    string CountryCode,
    string TimeZoneId,
    string? AdministrativeAreaCode,
    string? Address,
    string? ExternalProviderCode,
    string? ExternalCenterId,
    UserId ActorUserId) : ICommand<ExamCenterId>;

public sealed record CreateExamPlaceCommand(
    OrganizationId OrganizationId,
    ExamCenterId ExamCenterId,
    string ExamType,
    string LicenseCategory,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string TimeZoneId,
    ExamPlaceSource Source,
    string ProviderCode,
    string? ExternalPlaceId,
    UserId ActorUserId) : ICommand<ExamPlaceId>;

public sealed record GetExamCentersQuery(OrganizationId OrganizationId) : IQuery<IReadOnlyList<ExamCenterResponse>>;
public sealed record GetAvailableExamPlacesQuery(OrganizationId OrganizationId, DateTimeOffset FromUtc, DateTimeOffset ToUtc, string? LicenseCategory) : IQuery<IReadOnlyList<ExamPlaceResponse>>;

public sealed record ExamCenterResponse(Guid Id, string Name, string CountryCode, string TimeZoneId,
    string? AdministrativeAreaCode, string? Address, string? ExternalProviderCode, string? ExternalCenterId, string Status);

public sealed record ExamPlaceResponse(Guid Id, Guid ExamCenterId, string ExamType, string LicenseCategory,
    DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc, string TimeZoneId, string Source, string ProviderCode,
    string? ExternalPlaceId, string Status, DateTimeOffset LastObservedAtUtc, DateTimeOffset? HoldExpiresAtUtc,
    Guid? AssignedStudentId, Guid? ExamRegistrationId);
