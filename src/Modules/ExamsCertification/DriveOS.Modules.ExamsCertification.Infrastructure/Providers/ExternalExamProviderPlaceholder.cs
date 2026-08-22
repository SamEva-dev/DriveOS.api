using DriveOS.Modules.ExamsCertification.Application.Providers;
using DriveOS.Modules.ExamsCertification.Application.Registrations.Submissions;
using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Providers;

/// <summary>
/// Declares a supported integration slot without inventing undocumented endpoints. Replace this adapter with an
/// authorized transport implementation when credentials/specifications are issued by the provider.
/// </summary>
internal sealed class ExternalExamProviderPlaceholder(
    string code,
    string countryCode,
    ExamPlaceProviderKind kind,
    ExamPlaceProviderCapability capabilities) : IExamPlaceProvider, IExamProviderHealthProbe, IExamRegistrationSubmissionProvider
{
    public ExamPlaceProviderDescriptor Descriptor { get; } = new(code, countryCode, kind, capabilities, false);

    public Task<IReadOnlyCollection<ExternalExamPlace>> GetAvailablePlacesAsync(ExamPlaceAvailabilityRequest request,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException($"External exam provider adapter '{code}' is not configured.");

    public Task<IReadOnlyCollection<ExternalAssignedExam>> GetAssignedExamsAsync(OrganizationId organizationId,
        DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException($"External exam provider adapter '{code}' is not configured.");

    public Task<ExamProviderHealthProbeResult> ProbeAsync(OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        Task.FromResult(new ExamProviderHealthProbeResult(false, "Exams.ProviderConnection.AdapterUnavailable"));

    public Task<ExternalExamRegistrationSubmissionResult> SubmitAsync(
        ExternalExamRegistrationSubmissionRequest request,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException($"External exam provider adapter '{code}' is not configured.");
}
