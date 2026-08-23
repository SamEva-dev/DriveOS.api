using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;
using DriveOS.Modules.RegulatoryIntegrations.Application.Administration;
using DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.ExamsCertification;

/// <summary>
/// Read-side adapter used by BC-11 to evaluate the French regulatory training record.
/// It reads DriveOS' durable synchronization state only; it never calls DSR/RdvPermis directly.
/// </summary>
internal sealed class FrenchLivretNumeriqueProvider(
    IRegulatoryIntegrationConnectionReadService connections,
    IRegulatoryTrainingRecordAdministrationService trainingRecords) : IRegulatoryTrainingRecordProvider
{
    public const string Code = "fr-livret-numerique";
    public string ProviderCode => Code;

    public bool CanHandle(string? countryCode) =>
        string.Equals(countryCode?.Trim(), "FR", StringComparison.OrdinalIgnoreCase);

    public async Task<Result<RegulatoryTrainingRecordEvaluation>> EvaluateAsync(
        RegulatoryTrainingRecordContext context,
        CancellationToken cancellationToken = default)
    {
        Result<StudentRegulatoryTrainingRecordOverview> overviewResult = await trainingRecords.GetStudentOverviewAsync(
            context.OrganizationId,
            context.StudentId,
            context.TrainingPathId,
            "FR",
            ProviderCode,
            cancellationToken);

        if (overviewResult.IsFailure)
            return Result.Failure<RegulatoryTrainingRecordEvaluation>(overviewResult.Error);

        StudentRegulatoryTrainingRecordOverview overview = overviewResult.Value;

        // Existing durable submissions are authoritative evidence that the provider applies
        // to this training path, even if the current connection was later suspended/removed.
        if (overview.TotalSubmissions > 0)
            return Result.Success(MapOverview(overview));

        RegulatoryIntegrationConnectionResponse? connection = await connections.ResolveActiveAsync(
            context.OrganizationId,
            context.BranchId,
            "FR",
            ProviderCode,
            cancellationToken);

        if (connection is null)
        {
            return Result.Success(new RegulatoryTrainingRecordEvaluation(
                Required: false,
                Status: RegulatoryTrainingRecordStatus.Pending,
                ProviderCode: ProviderCode,
                Evidence: $"state=connection-not-configured;examType={context.ExamType};category={context.LicenseCategory}"));
        }

        return Result.Success(new RegulatoryTrainingRecordEvaluation(
            Required: true,
            Status: RegulatoryTrainingRecordStatus.Pending,
            ProviderCode: ProviderCode,
            ExternalReference: connection.ExternalAccountReference,
            Evidence: $"state=no-submission-yet;scope={(connection.BranchId.HasValue ? "branch" : "organization")};connectionId={connection.Id};examType={context.ExamType};category={context.LicenseCategory}"));
    }

    private RegulatoryTrainingRecordEvaluation MapOverview(StudentRegulatoryTrainingRecordOverview overview)
    {
        RegulatoryTrainingRecordStatus status = overview.CurrentStatus switch
        {
            RegulatoryTrainingRecordSubmissionStatus.Accepted => RegulatoryTrainingRecordStatus.Compliant,
            RegulatoryTrainingRecordSubmissionStatus.WaitingForData => RegulatoryTrainingRecordStatus.Blocked,
            RegulatoryTrainingRecordSubmissionStatus.Rejected => RegulatoryTrainingRecordStatus.Blocked,
            RegulatoryTrainingRecordSubmissionStatus.Failed => RegulatoryTrainingRecordStatus.Warning,
            RegulatoryTrainingRecordSubmissionStatus.Cancelled => RegulatoryTrainingRecordStatus.Warning,
            RegulatoryTrainingRecordSubmissionStatus.Pending => RegulatoryTrainingRecordStatus.Pending,
            RegulatoryTrainingRecordSubmissionStatus.Processing => RegulatoryTrainingRecordStatus.Pending,
            RegulatoryTrainingRecordSubmissionStatus.Submitted => RegulatoryTrainingRecordStatus.Pending,
            RegulatoryTrainingRecordSubmissionStatus.RetryPending => RegulatoryTrainingRecordStatus.Pending,
            RegulatoryTrainingRecordSubmissionStatus.Superseded => RegulatoryTrainingRecordStatus.Pending,
            null => RegulatoryTrainingRecordStatus.Pending,
            _ => RegulatoryTrainingRecordStatus.Pending
        };

        string? externalReference = overview.RecentSubmissions
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ExternalReference))
            ?.ExternalReference;

        string issues = overview.CurrentIssues.Count == 0
            ? "none"
            : string.Join(',', overview.CurrentIssues);

        return new RegulatoryTrainingRecordEvaluation(
            Required: true,
            Status: status,
            ProviderCode: ProviderCode,
            ExternalReference: externalReference,
            Evidence: $"state={overview.CurrentStatus?.ToString() ?? "none"};total={overview.TotalSubmissions};accepted={overview.Accepted};waitingForData={overview.WaitingForData};pending={overview.Pending};rejected={overview.Rejected};retryPending={overview.RetryPending};failed={overview.Failed};issues={issues};lastError={overview.LastErrorCode ?? "none"}");
    }
}
