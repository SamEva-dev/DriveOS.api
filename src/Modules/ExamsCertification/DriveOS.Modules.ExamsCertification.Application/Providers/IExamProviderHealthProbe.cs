using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Providers;

public interface IExamProviderHealthProbe
{
    Task<ExamProviderHealthProbeResult> ProbeAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
}

public sealed record ExamProviderHealthProbeResult(bool Success, string? ErrorCode = null);
