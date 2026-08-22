using DriveOS.Modules.ExamsCertification.Application.Providers;
using DriveOS.Modules.ExamsCertification.Application.Providers.Connections;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Providers;

internal sealed class ExamProviderConnectionTester(IExamPlaceProviderResolver resolver) : IExamProviderConnectionTester
{
    public async Task<ExamProviderConnectionTestResult> TestAsync(OrganizationId organizationId, string providerCode,
        CancellationToken cancellationToken = default)
    {
        IExamPlaceProvider? provider = resolver.Resolve(providerCode);
        if (provider is null)
            return new(false, "Exams.ProviderConnection.AdapterNotInstalled", Array.Empty<string>());

        string[] capabilities = Enum.GetValues<DriveOS.Modules.ExamsCertification.Domain.Providers.ExamPlaceProviderCapability>()
            .Where(x => x != 0 && provider.Descriptor.Capabilities.HasFlag(x))
            .Select(x => x.ToString())
            .ToArray();

        if (!provider.Descriptor.IsEnabled)
            return new(false, "Exams.ProviderConnection.AdapterUnavailable", capabilities);

        if (provider is IExamProviderHealthProbe probe)
        {
            ExamProviderHealthProbeResult result = await probe.ProbeAsync(organizationId, cancellationToken);
            return new(result.Success, result.ErrorCode, capabilities);
        }

        return new(true, null, capabilities);
    }
}
