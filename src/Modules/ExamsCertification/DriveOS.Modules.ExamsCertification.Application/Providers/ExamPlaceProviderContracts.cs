using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Providers;

public sealed record ExamPlaceProviderDescriptor(
    string Code,
    string CountryCode,
    ExamPlaceProviderKind Kind,
    ExamPlaceProviderCapability Capabilities,
    bool IsEnabled);

public sealed record ExternalExamPlace(
    string ExternalPlaceId,
    string ExternalCenterId,
    string CenterName,
    string CountryCode,
    string? AdministrativeAreaCode,
    string ExamType,
    string ExamCategory,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    int Capacity,
    int AvailableCapacity,
    string? TimeZoneId,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record ExternalAssignedExam(
    string ExternalRegistrationId,
    string ExternalPlaceId,
    string CandidateReference,
    DateTimeOffset StartsAtUtc,
    string Status,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record ExamPlaceAvailabilityRequest(
    OrganizationId OrganizationId,
    string CountryCode,
    string? AdministrativeAreaCode,
    string? ExamCategory,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    IReadOnlyCollection<string>? CenterExternalIds = null);
