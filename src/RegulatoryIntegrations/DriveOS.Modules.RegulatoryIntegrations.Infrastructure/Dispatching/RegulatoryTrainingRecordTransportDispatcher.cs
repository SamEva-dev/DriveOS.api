using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;

namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Dispatching;

internal sealed class RegulatoryTrainingRecordTransportDispatcher(
    IEnumerable<IRegulatoryTrainingRecordTransportProvider> providers)
    : IRegulatoryTrainingRecordTransportDispatcher
{
    private readonly IReadOnlyDictionary<string, IRegulatoryTrainingRecordTransportProvider> _providers =
        providers.ToDictionary(x => x.ProviderCode, StringComparer.OrdinalIgnoreCase);

    public Task<RegulatoryTrainingRecordTransportResult> DispatchAsync(
        RegulatoryTrainingRecordTransportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(request.ProviderCode, out IRegulatoryTrainingRecordTransportProvider? provider))
        {
            return Task.FromResult(RegulatoryTrainingRecordTransportResult.Unavailable(
                "regulatory-provider-not-registered",
                $"No transport provider is registered for '{request.ProviderCode}'.",
                TimeSpan.FromHours(6)));
        }

        return provider.SendAsync(request, cancellationToken);
    }
}
