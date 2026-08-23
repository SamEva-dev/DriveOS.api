using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using DriveOS.Modules.ExamsCertification.Application.Registrations.File;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.File;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.ExamsCertification;

/// <summary>
/// BC-11 adapter over the provider-independent regulatory training-record port.
/// ExamsCertification consumes a normalized requirement and remains unaware of
/// DSR, RdvPermis or any other country-specific transport contract.
/// </summary>
internal sealed class RegulatoryExamFileRequirementGateway(
    IRegulatoryTrainingRecordGateway regulatoryTrainingRecordGateway) : IRegulatoryExamFileRequirementGateway
{
    public async Task<Result<RegulatoryExamFileRequirement>> EvaluateAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        string? countryCode,
        string examType,
        string licenseCategory,
        CancellationToken cancellationToken = default)
    {
        Result<RegulatoryTrainingRecordEvaluation> result = await regulatoryTrainingRecordGateway.EvaluateAsync(
            new RegulatoryTrainingRecordContext(
                organizationId,
                studentId,
                trainingPathId,
                countryCode,
                examType,
                licenseCategory),
            cancellationToken);

        if (result.IsFailure)
            return Result.Failure<RegulatoryExamFileRequirement>(result.Error);

        RegulatoryTrainingRecordEvaluation evaluation = result.Value;

        return Result.Success(new RegulatoryExamFileRequirement(
            evaluation.Required,
            Map(evaluation.Status),
            BuildEvidence(evaluation)));
    }

    private static ExamRegistrationRequirementStatus Map(RegulatoryTrainingRecordStatus status) => status switch
    {
        RegulatoryTrainingRecordStatus.NotApplicable => ExamRegistrationRequirementStatus.NotApplicable,
        RegulatoryTrainingRecordStatus.Compliant => ExamRegistrationRequirementStatus.Compliant,
        RegulatoryTrainingRecordStatus.Warning => ExamRegistrationRequirementStatus.Warning,
        RegulatoryTrainingRecordStatus.Blocked => ExamRegistrationRequirementStatus.Blocked,
        RegulatoryTrainingRecordStatus.Pending => ExamRegistrationRequirementStatus.Pending,
        RegulatoryTrainingRecordStatus.Unavailable => ExamRegistrationRequirementStatus.Pending,
        _ => ExamRegistrationRequirementStatus.Pending
    };

    private static string BuildEvidence(RegulatoryTrainingRecordEvaluation evaluation)
    {
        string prefix = $"provider={evaluation.ProviderCode};status={evaluation.Status}";
        if (!string.IsNullOrWhiteSpace(evaluation.ExternalReference))
            prefix += $";externalReference={evaluation.ExternalReference}";

        return string.IsNullOrWhiteSpace(evaluation.Evidence)
            ? prefix
            : $"{prefix};{evaluation.Evidence}";
    }
}
