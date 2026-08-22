using DriveOS.Modules.ExamsCertification.Application.Registrations.Submissions;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Providers;

internal sealed class ExamRegistrationSubmissionProviderResolver(IEnumerable<IExamRegistrationSubmissionProvider> providers)
    : IExamRegistrationSubmissionProviderResolver
{
    private readonly IReadOnlyDictionary<string, IExamRegistrationSubmissionProvider> _providers = providers
        .ToDictionary(x => x.Descriptor.Code, StringComparer.OrdinalIgnoreCase);

    public IExamRegistrationSubmissionProvider? Resolve(string providerCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerCode);
        return _providers.GetValueOrDefault(providerCode);
    }
}
