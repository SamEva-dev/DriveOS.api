using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.ExamsCertification;

/// <summary>
/// Resolves the country-specific regulatory training-record adapter.
/// Business bounded contexts depend only on IRegulatoryTrainingRecordGateway.
/// </summary>
internal sealed class RegulatoryTrainingRecordGateway(
    IEnumerable<IRegulatoryTrainingRecordProvider> providers) : IRegulatoryTrainingRecordGateway
{
    public Task<Result<RegulatoryTrainingRecordEvaluation>> EvaluateAsync(
        RegulatoryTrainingRecordContext context,
        CancellationToken cancellationToken = default)
    {
        IRegulatoryTrainingRecordProvider? provider = providers
            .FirstOrDefault(x => x.CanHandle(context.CountryCode));

        if (provider is null)
        {
            return Task.FromResult(Result.Success(new RegulatoryTrainingRecordEvaluation(
                Required: false,
                Status: RegulatoryTrainingRecordStatus.NotApplicable,
                ProviderCode: "none",
                Evidence: $"country={context.CountryCode ?? "unknown"};provider=none")));
        }

        return provider.EvaluateAsync(context, cancellationToken);
    }
}
