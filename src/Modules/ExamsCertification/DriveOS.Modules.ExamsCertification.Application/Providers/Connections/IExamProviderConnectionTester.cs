using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Providers.Connections;

public interface IExamProviderConnectionTester
{
    Task<ExamProviderConnectionTestResult> TestAsync(
        OrganizationId organizationId,
        string providerCode,
        CancellationToken cancellationToken = default);
}

public sealed record ExamProviderConnectionTestResult(bool Success, string? ErrorCode, IReadOnlyCollection<string> Capabilities);
