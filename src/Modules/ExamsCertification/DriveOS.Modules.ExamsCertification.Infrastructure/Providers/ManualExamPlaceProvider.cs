using DriveOS.Modules.ExamsCertification.Application.Providers;
using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Providers;

/// <summary>
/// Safe baseline provider. It deliberately performs no external call; manual and file imports will persist places in BC-11.
/// </summary>
internal sealed class ManualExamPlaceProvider : IExamPlaceProvider
{
    public ExamPlaceProviderDescriptor Descriptor { get; } = new(
        "manual",
        "*",
        ExamPlaceProviderKind.Manual,
        ExamPlaceProviderCapability.None,
        true);

    public Task<IReadOnlyCollection<ExternalExamPlace>> GetAvailablePlacesAsync(
        ExamPlaceAvailabilityRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<ExternalExamPlace>>(Array.Empty<ExternalExamPlace>());

    public Task<IReadOnlyCollection<ExternalAssignedExam>> GetAssignedExamsAsync(
        OrganizationId organizationId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<ExternalAssignedExam>>(Array.Empty<ExternalAssignedExam>());
}
