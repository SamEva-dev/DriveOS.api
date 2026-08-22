using DriveOS.SharedKernel.Auditing;
using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;

/// <summary>
/// Tenant-scoped configuration of an external examination-system connector. The aggregate stores only
/// non-secret connection metadata and secret references; actual credentials/tokens belong to the platform secret store.
/// </summary>
public sealed class ExamProviderConnection : AggregateRoot<ExamProviderConnectionId>, IAuditableEntity
{
    private ExamProviderConnection() { }

    private ExamProviderConnection(ExamProviderConnectionId id, OrganizationId organizationId, string providerCode,
        string displayName, string countryCode, ExamPlaceProviderKind kind, ExamProviderAuthenticationMode authenticationMode,
        string? baseUrl, string? credentialReference, int requestsPerMinute, DateTimeOffset nowUtc) : base(id)
    {
        OrganizationId = organizationId;
        ProviderCode = providerCode.Trim().ToLowerInvariant();
        DisplayName = displayName.Trim();
        CountryCode = countryCode.Trim().ToUpperInvariant();
        Kind = kind;
        AuthenticationMode = authenticationMode;
        BaseUrl = NormalizeUrl(baseUrl);
        CredentialReference = Normalize(credentialReference);
        RequestsPerMinute = requestsPerMinute;
        Status = authenticationMode is ExamProviderAuthenticationMode.OAuth2AuthorizationCode
            ? ExamProviderConnectionStatus.PendingAuthorization
            : ExamProviderConnectionStatus.Draft;
        CreatedAtUtc = nowUtc.ToUniversalTime();
    }

    public OrganizationId OrganizationId { get; private set; }
    public string ProviderCode { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public ExamPlaceProviderKind Kind { get; private set; }
    public ExamProviderAuthenticationMode AuthenticationMode { get; private set; }
    public string? BaseUrl { get; private set; }
    /// <summary>Opaque key/URI into the configured platform secret store. Never contains a password, token or client secret.</summary>
    public string? CredentialReference { get; private set; }
    public int RequestsPerMinute { get; private set; }
    public ExamProviderConnectionStatus Status { get; private set; }
    public DateTimeOffset? LastTestedAtUtc { get; private set; }
    public DateTimeOffset? LastSuccessfulAtUtc { get; private set; }
    public string? LastErrorCode { get; private set; }
    public int ConsecutiveFailureCount { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<ExamProviderConnection> Create(ExamProviderConnectionId id, OrganizationId organizationId,
        string providerCode, string displayName, string countryCode, ExamPlaceProviderKind kind,
        ExamProviderAuthenticationMode authenticationMode, string? baseUrl, string? credentialReference,
        int requestsPerMinute, DateTimeOffset nowUtc)
    {
        if (id.IsEmpty) return Result.Failure<ExamProviderConnection>(ExamProviderConnectionErrors.InvalidIdentifier);
        if (organizationId.IsEmpty) return Result.Failure<ExamProviderConnection>(ExamProviderConnectionErrors.InvalidOrganization);
        if (string.IsNullOrWhiteSpace(providerCode) || string.IsNullOrWhiteSpace(displayName)) return Result.Failure<ExamProviderConnection>(ExamProviderConnectionErrors.InvalidProvider);
        if (string.IsNullOrWhiteSpace(countryCode)) return Result.Failure<ExamProviderConnection>(ExamProviderConnectionErrors.InvalidCountry);
        if (requestsPerMinute is < 1 or > 600) return Result.Failure<ExamProviderConnection>(ExamProviderConnectionErrors.InvalidProvider);
        if (!IsValidUrl(baseUrl)) return Result.Failure<ExamProviderConnection>(ExamProviderConnectionErrors.InvalidEndpoint);
        return Result.Success(new ExamProviderConnection(id, organizationId, providerCode, displayName, countryCode, kind,
            authenticationMode, baseUrl, credentialReference, requestsPerMinute, nowUtc));
    }

    public Result Configure(string displayName, string? baseUrl, string? credentialReference, int requestsPerMinute,
        UserId actorUserId, DateTimeOffset nowUtc)
    {
        if (Status == ExamProviderConnectionStatus.Revoked) return Result.Failure(ExamProviderConnectionErrors.Revoked);
        if (string.IsNullOrWhiteSpace(displayName) || requestsPerMinute is < 1 or > 600) return Result.Failure(ExamProviderConnectionErrors.InvalidProvider);
        if (!IsValidUrl(baseUrl)) return Result.Failure(ExamProviderConnectionErrors.InvalidEndpoint);
        DisplayName = displayName.Trim();
        BaseUrl = NormalizeUrl(baseUrl);
        CredentialReference = Normalize(credentialReference);
        RequestsPerMinute = requestsPerMinute;
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public void MarkAuthorizationPending(UserId actorUserId, DateTimeOffset nowUtc)
    {
        Status = ExamProviderConnectionStatus.PendingAuthorization;
        SetModifiedAudit(nowUtc, actorUserId);
    }

    public void RecordConnectionSuccess(UserId actorUserId, DateTimeOffset nowUtc)
    {
        LastTestedAtUtc = nowUtc.ToUniversalTime();
        LastSuccessfulAtUtc = nowUtc.ToUniversalTime();
        LastErrorCode = null;
        ConsecutiveFailureCount = 0;
        Status = ExamProviderConnectionStatus.Active;
        SetModifiedAudit(nowUtc, actorUserId);
    }

    public void RecordConnectionFailure(string errorCode, UserId actorUserId, DateTimeOffset nowUtc)
    {
        LastTestedAtUtc = nowUtc.ToUniversalTime();
        LastErrorCode = string.IsNullOrWhiteSpace(errorCode) ? "Exams.ProviderConnection.TestFailed" : errorCode.Trim();
        ConsecutiveFailureCount++;
        Status = ExamProviderConnectionStatus.Degraded;
        SetModifiedAudit(nowUtc, actorUserId);
    }

    public Result Suspend(UserId actorUserId, DateTimeOffset nowUtc)
    {
        if (Status == ExamProviderConnectionStatus.Revoked) return Result.Failure(ExamProviderConnectionErrors.Revoked);
        Status = ExamProviderConnectionStatus.Suspended;
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public void Revoke(UserId actorUserId, DateTimeOffset nowUtc)
    {
        Status = ExamProviderConnectionStatus.Revoked;
        CredentialReference = null;
        SetModifiedAudit(nowUtc, actorUserId);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? NormalizeUrl(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().TrimEnd('/');
    private static bool IsValidUrl(string? value) => string.IsNullOrWhiteSpace(value)
        || (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.IsLoopback));

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;

        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }
}
