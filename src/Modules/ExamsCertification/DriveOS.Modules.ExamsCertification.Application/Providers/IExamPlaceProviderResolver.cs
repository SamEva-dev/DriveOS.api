namespace DriveOS.Modules.ExamsCertification.Application.Providers;

public interface IExamPlaceProviderResolver
{
    IReadOnlyCollection<ExamPlaceProviderDescriptor> GetAvailableProviders();
    IExamPlaceProvider? Resolve(string providerCode);
}
