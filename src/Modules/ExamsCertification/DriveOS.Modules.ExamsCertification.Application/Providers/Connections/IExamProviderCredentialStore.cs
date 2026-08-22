namespace DriveOS.Modules.ExamsCertification.Application.Providers.Connections;

/// <summary>
/// Platform secret-store boundary. BC-11 persists only opaque references; OAuth refresh tokens, API keys and client
/// secrets must be stored by an implementation backed by the deployment secret manager, never in exam tables.
/// </summary>
public interface IExamProviderCredentialStore
{
    Task<string> StoreAsync(string logicalName, IReadOnlyDictionary<string, string> secrets, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, string>?> ReadAsync(string credentialReference, CancellationToken cancellationToken = default);
    Task DeleteAsync(string credentialReference, CancellationToken cancellationToken = default);
}
