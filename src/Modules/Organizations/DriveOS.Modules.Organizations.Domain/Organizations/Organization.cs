using DriveOS.Modules.Organizations.Domain.Organizations.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Organizations;

public sealed class Organization : AggregateRoot<OrganizationId>, IAuditableEntity
{
    private Organization() { }

    private Organization(
        OrganizationId id,
        string legalName,
        string countryCode,
        OrganizationType type
    )
        : base(id)
    {
        LegalName = legalName;
        CountryCode = countryCode;
        Type = type;
        Status = OrganizationStatus.Draft;
    }

    public string LegalName { get; private set; } = string.Empty;

    public string CountryCode { get; private set; } = string.Empty;

    public OrganizationType Type { get; private set; }

    public OrganizationStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public UserId? CreatedByUserId { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public UserId? LastModifiedByUserId { get; private set; }

    private readonly List<OrganizationStatusHistoryEntry> _statusHistory = [];

    public IReadOnlyCollection<OrganizationStatusHistoryEntry> StatusHistory =>
        _statusHistory.AsReadOnly();

    public static Result<Organization> Create(
        OrganizationId id,
        string legalName,
        string countryCode,
        OrganizationType type
    )
    {
        if (id.IsEmpty)
        {
            return Result.Failure<Organization>(OrganizationErrors.EmptyId);
        }

        string normalizedLegalName = legalName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedLegalName))
        {
            return Result.Failure<Organization>(OrganizationErrors.EmptyLegalName);
        }

        const int maximumLegalNameLength = 200;

        if (normalizedLegalName.Length > maximumLegalNameLength)
        {
            return Result.Failure<Organization>(
                OrganizationErrors.LegalNameTooLong(maximumLegalNameLength)
            );
        }

        string normalizedCountryCode = countryCode?.Trim().ToUpperInvariant() ?? string.Empty;

        if (!IsValidCountryCode(normalizedCountryCode))
        {
            return Result.Failure<Organization>(OrganizationErrors.InvalidCountryCode);
        }

        if (!Enum.IsDefined(type))
        {
            return Result.Failure<Organization>(OrganizationErrors.InvalidOrganizationType);
        }

        var organization = new Organization(id, normalizedLegalName, normalizedCountryCode, type);

        organization.RaiseDomainEvent(
            new OrganizationCreatedDomainEvent(
                organization.Id,
                organization.LegalName,
                organization.CountryCode,
                organization.Type
            )
        );

        return Result.Success(organization);
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
        {
            return;
        }

        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedByUserId = modifiedByUserId;
    }

    public void SubmitForActivation(
        OrganizationStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc
    )
    {
        EnsureStatus(
            OrganizationStatus.Draft,
            "Only a draft organization can be submitted for activation."
        );

        ChangeStatus(OrganizationStatus.PendingActivation, reason, changedByUserId, changedAtUtc);
    }

    public void Activate(
        OrganizationStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc
    )
    {
        EnsureStatusIn(
            [
                OrganizationStatus.PendingActivation,
                OrganizationStatus.Restricted,
                OrganizationStatus.Suspended,
            ],
            "The organization cannot be activated from its current status."
        );

        ChangeStatus(OrganizationStatus.Active, reason, changedByUserId, changedAtUtc);
    }

    public void Restrict(
        OrganizationStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc
    )
    {
        EnsureStatus(OrganizationStatus.Active, "Only an active organization can be restricted.");

        ChangeStatus(OrganizationStatus.Restricted, reason, changedByUserId, changedAtUtc);
    }

    public void Suspend(
        OrganizationStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc
    )
    {
        EnsureStatusIn(
            [OrganizationStatus.Active, OrganizationStatus.Restricted],
            "Only an active or restricted organization can be suspended."
        );

        ChangeStatus(OrganizationStatus.Suspended, reason, changedByUserId, changedAtUtc);
    }

    public void Close(
        OrganizationStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc
    )
    {
        EnsureStatusIn(
            [
                OrganizationStatus.Active,
                OrganizationStatus.Restricted,
                OrganizationStatus.Suspended,
            ],
            "The organization cannot be closed from its current status."
        );

        ChangeStatus(OrganizationStatus.Closed, reason, changedByUserId, changedAtUtc);
    }

    private static bool IsValidCountryCode(string countryCode)
    {
        return countryCode.Length == 2 && countryCode.All(char.IsLetter);
    }

    private void ChangeStatus(
        OrganizationStatus newStatus,
        OrganizationStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc
    )
    {
        OrganizationStatus previousStatus = Status;

        Status = newStatus;

        _statusHistory.Add(
            OrganizationStatusHistoryEntry.Create(
                Id,
                previousStatus,
                newStatus,
                reason,
                changedByUserId,
                changedAtUtc
            )
        );

        RaiseDomainEvent(
            new OrganizationStatusChangedDomainEvent(
                Id,
                previousStatus,
                newStatus,
                reason.Value,
                changedByUserId,
                changedAtUtc
            )
        );
    }

    private void EnsureStatus(OrganizationStatus expectedStatus, string errorMessage)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidOperationException(errorMessage);
        }
    }

    private void EnsureStatusIn(
        IReadOnlyCollection<OrganizationStatus> allowedStatuses,
        string errorMessage
    )
    {
        if (!allowedStatuses.Contains(Status))
        {
            throw new InvalidOperationException(errorMessage);
        }
    }
}
