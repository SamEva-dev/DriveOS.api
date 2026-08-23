using DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;

/// <summary>
/// Tenant-owned mapping between an organization/optional branch and a national regulatory provider.
/// Credentials are never stored here; SecretReference only points to an external secret store entry.
/// </summary>
public sealed class RegulatoryIntegrationConnection : AggregateRoot<RegulatoryIntegrationConnectionId>, IAuditableEntity
{
    private RegulatoryIntegrationConnection() { }

    private RegulatoryIntegrationConnection(
        RegulatoryIntegrationConnectionId id,
        OrganizationId organizationId,
        BranchId? branchId,
        string countryCode,
        string providerCode,
        string externalAccountReference,
        string? secretReference)
        : base(id)
    {
        OrganizationId = organizationId;
        BranchId = branchId;
        ScopeKey = branchId.HasValue ? $"branch:{branchId.Value.Value:N}" : "organization";
        CountryCode = countryCode;
        ProviderCode = providerCode;
        ExternalAccountReference = externalAccountReference;
        SecretReference = secretReference;
        Status = RegulatoryIntegrationConnectionStatus.Draft;
        Revision = 1;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public string ScopeKey { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public string ProviderCode { get; private set; } = string.Empty;
    public string ExternalAccountReference { get; private set; } = string.Empty;
    public string? SecretReference { get; private set; }
    public RegulatoryIntegrationConnectionStatus Status { get; private set; }
    public int Revision { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<RegulatoryIntegrationConnection> Create(
        RegulatoryIntegrationConnectionId id,
        OrganizationId organizationId,
        BranchId? branchId,
        string? countryCode,
        string? providerCode,
        string? externalAccountReference,
        string? secretReference)
    {
        string country = (countryCode ?? string.Empty).Trim().ToUpperInvariant();
        if (country.Length != 2 || country.Any(c => !char.IsLetter(c)))
            return Result.Failure<RegulatoryIntegrationConnection>(RegulatoryIntegrationConnectionErrors.InvalidCountryCode);

        string provider = (providerCode ?? string.Empty).Trim().ToLowerInvariant();
        if (provider.Length is < 2 or > 100)
            return Result.Failure<RegulatoryIntegrationConnection>(RegulatoryIntegrationConnectionErrors.InvalidProviderCode);

        string account = (externalAccountReference ?? string.Empty).Trim();
        if (account.Length is < 1 or > 200)
            return Result.Failure<RegulatoryIntegrationConnection>(RegulatoryIntegrationConnectionErrors.InvalidExternalAccountReference);

        string? secret = string.IsNullOrWhiteSpace(secretReference) ? null : secretReference.Trim();
        if (secret?.Length > 300)
            return Result.Failure<RegulatoryIntegrationConnection>(RegulatoryIntegrationConnectionErrors.InvalidSecretReference);

        var connection = new RegulatoryIntegrationConnection(id, organizationId, branchId, country, provider, account, secret);
        connection.RaiseDomainEvent(new RegulatoryIntegrationConnectionConfiguredDomainEvent(id, organizationId, branchId, country, provider));
        return Result.Success(connection);
    }

    public Result Update(string externalAccountReference, string? secretReference)
    {
        if (Status == RegulatoryIntegrationConnectionStatus.Ended)
            return Result.Failure(RegulatoryIntegrationConnectionErrors.Ended);
        string account = (externalAccountReference ?? string.Empty).Trim();
        if (account.Length is < 1 or > 200)
            return Result.Failure(RegulatoryIntegrationConnectionErrors.InvalidExternalAccountReference);
        string? secret = string.IsNullOrWhiteSpace(secretReference) ? null : secretReference.Trim();
        if (secret?.Length > 300)
            return Result.Failure(RegulatoryIntegrationConnectionErrors.InvalidSecretReference);
        ExternalAccountReference = account;
        SecretReference = secret;
        Revision++;
        return Result.Success();
    }

    public Result Activate() { if (Status == RegulatoryIntegrationConnectionStatus.Ended) return Result.Failure(RegulatoryIntegrationConnectionErrors.Ended); Status = RegulatoryIntegrationConnectionStatus.Active; Revision++; return Result.Success(); }
    public Result Suspend() { if (Status == RegulatoryIntegrationConnectionStatus.Ended) return Result.Failure(RegulatoryIntegrationConnectionErrors.Ended); Status = RegulatoryIntegrationConnectionStatus.Suspended; Revision++; return Result.Success(); }
    public Result End() { Status = RegulatoryIntegrationConnectionStatus.Ended; Revision++; return Result.Success(); }
    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId) { if (CreatedAtUtc == default) { CreatedAtUtc = createdAtUtc; CreatedByUserId = createdByUserId; } }
    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId) { LastModifiedAtUtc = modifiedAtUtc; LastModifiedByUserId = modifiedByUserId; }
}
