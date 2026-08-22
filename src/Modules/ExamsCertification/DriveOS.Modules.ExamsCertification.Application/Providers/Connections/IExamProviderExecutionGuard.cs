using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Providers.Connections;

public interface IExamProviderExecutionGuard
{
    Task<T> ExecuteAsync<T>(OrganizationId organizationId, string providerCode,
        Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
}
