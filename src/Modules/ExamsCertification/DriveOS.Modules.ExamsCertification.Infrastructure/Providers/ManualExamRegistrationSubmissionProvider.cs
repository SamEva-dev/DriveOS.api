using DriveOS.Modules.ExamsCertification.Application.Providers;
using DriveOS.Modules.ExamsCertification.Application.Registrations.Submissions;
using DriveOS.Modules.ExamsCertification.Domain.Providers;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Providers;

/// <summary>
/// Manual provider never pretends an administration has accepted a registration. It only records that the dossier
/// is ready for a human to submit through the official channel, after which the official response can be recorded.
/// </summary>
internal sealed class ManualExamRegistrationSubmissionProvider : IExamRegistrationSubmissionProvider
{
    public ExamPlaceProviderDescriptor Descriptor { get; } = new(
        "manual", "*", ExamPlaceProviderKind.Manual, ExamPlaceProviderCapability.SubmitRegistration, true);

    public Task<ExternalExamRegistrationSubmissionResult> SubmitAsync(
        ExternalExamRegistrationSubmissionRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new ExternalExamRegistrationSubmissionResult(
            ExternalExamRegistrationSubmissionOutcome.AwaitingManualSubmission));
}
