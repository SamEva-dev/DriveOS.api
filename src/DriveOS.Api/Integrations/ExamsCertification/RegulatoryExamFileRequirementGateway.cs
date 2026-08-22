using DriveOS.Modules.ExamsCertification.Application.Registrations.File;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.File;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.ExamsCertification;

/// <summary>
/// Integration seam for country-specific regulatory training records (for example the French Livret Numérique).
/// Until a national adapter is configured, the regulatory record remains visible but non-blocking.
/// </summary>
internal sealed class RegulatoryExamFileRequirementGateway : IRegulatoryExamFileRequirementGateway
{
    public Task<Result<RegulatoryExamFileRequirement>> EvaluateAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        string? countryCode,
        string examType,
        string licenseCategory,
        CancellationToken cancellationToken = default)
    {
        var result = new RegulatoryExamFileRequirement(
            Required: false,
            Status: ExamRegistrationRequirementStatus.Pending,
            Evidence: $"country={countryCode ?? "unknown"};provider=not-configured;examType={examType};category={licenseCategory}");
        return Task.FromResult(Result.Success(result));
    }
}
