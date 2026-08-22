using DriveOS.Modules.ExamsCertification.Application.Providers;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Providers;

internal sealed class ExamPlaceProviderResolver(IEnumerable<IExamPlaceProvider> providers) : IExamPlaceProviderResolver
{
    private readonly IReadOnlyDictionary<string, IExamPlaceProvider> _providers = providers
        .ToDictionary(x => x.Descriptor.Code, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<ExamPlaceProviderDescriptor> GetAvailableProviders() =>
        _providers.Values.Select(x => x.Descriptor).OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase).ToArray();

    public IExamPlaceProvider? Resolve(string providerCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerCode);
        return _providers.GetValueOrDefault(providerCode);
    }
}
