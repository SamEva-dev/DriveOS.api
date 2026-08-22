using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Providers;

/// <summary>
/// Anti-corruption boundary between BC-11 and national/partner examination systems.
/// Implementations may use an official API, an authorized partner API, a local agent or manual/file import.
/// </summary>
public interface IExamPlaceProvider
{
    ExamPlaceProviderDescriptor Descriptor { get; }

    Task<IReadOnlyCollection<ExternalExamPlace>> GetAvailablePlacesAsync(
        ExamPlaceAvailabilityRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExternalAssignedExam>> GetAssignedExamsAsync(
        OrganizationId organizationId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default);
}
