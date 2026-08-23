namespace DriveOS.Api.Integrations.RegulatoryTrainingRecords.France;

/// <summary>
/// Safe default until DriveOS receives the authoritative French editor integration contract.
/// It performs no network I/O and never interprets SecretReference as secret material.
/// </summary>
internal sealed class FrenchLivretNumeriqueOfficialClientUnavailable
    : IFrenchLivretNumeriqueOfficialClient
{
    public Task<FrenchLivretNumeriqueOfficialClientResult> SubmitAsync(
        FrenchLivretNumeriqueSubmission submission,
        string secretReference,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new FrenchLivretNumeriqueOfficialClientResult(
            FrenchLivretNumeriqueOfficialClientOutcome.Unavailable,
            Code: "fr-livret-numerique.official-editor-contract-not-configured",
            Detail: "The official DSR/RdvPermis editor transport contract is not configured in DriveOS.",
            RetryAfter: TimeSpan.FromHours(6)));
    }
}
