using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.RegulatoryIntegrations.Application.Administration;

public interface IRegulatoryTrainingRecordAdministrationService
{
    Task<Result<RegulatoryTrainingRecordSubmissionPage>> SearchAsync(
        OrganizationId organizationId,
        RegulatoryTrainingRecordSubmissionFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<RegulatoryTrainingRecordSubmissionDetail>> GetAsync(
        OrganizationId organizationId,
        Guid submissionId,
        CancellationToken cancellationToken = default);

    Task<Result<RegulatoryTrainingRecordSynchronizationSummary>> GetSummaryAsync(
        OrganizationId organizationId,
        string? countryCode,
        string? providerCode,
        CancellationToken cancellationToken = default);

    Task<Result<StudentRegulatoryTrainingRecordOverview>> GetStudentOverviewAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId? trainingPathId,
        string countryCode,
        string providerCode,
        CancellationToken cancellationToken = default);

    Task<Result<string>> GetProjectionPayloadAsync(
        OrganizationId organizationId,
        Guid submissionId,
        CancellationToken cancellationToken = default);

    Task<Result> RetryAsync(
        OrganizationId organizationId,
        Guid submissionId,
        DateTimeOffset requestedAtUtc,
        CancellationToken cancellationToken = default);
}
