using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Organizations;

public sealed class OrganizationStatusHistoryEntry
{
    private OrganizationStatusHistoryEntry()
    {
    }

    private OrganizationStatusHistoryEntry(
        Guid id,
        OrganizationId organizationId,
        OrganizationStatus previousStatus,
        OrganizationStatus newStatus,
        OrganizationStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc)
    {
        Id = id;
        OrganizationId = organizationId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason;
        ChangedByUserId = changedByUserId;
        ChangedAtUtc = changedAtUtc;
    }

    public Guid Id { get; private set; }

    public OrganizationId OrganizationId { get; private set; }

    public OrganizationStatus PreviousStatus { get; private set; }

    public OrganizationStatus NewStatus { get; private set; }

    public OrganizationStatusChangeReason Reason { get; private set; } = null!;

    public Guid ChangedByUserId { get; private set; }

    public DateTimeOffset ChangedAtUtc { get; private set; }

    internal static OrganizationStatusHistoryEntry Create(
        OrganizationId organizationId,
        OrganizationStatus previousStatus,
        OrganizationStatus newStatus,
        OrganizationStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc)
    {
        if (organizationId.IsEmpty)
        {
            throw new ArgumentException(
                "The organization identifier is required.",
                nameof(organizationId));
        }

        if (previousStatus == newStatus)
        {
            throw new InvalidOperationException(
                "The previous and new statuses cannot be identical.");
        }

        if (changedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "The user identifier is required.",
                nameof(changedByUserId));
        }

        ArgumentNullException.ThrowIfNull(reason);

        return new OrganizationStatusHistoryEntry(
            Guid.NewGuid(),
            organizationId,
            previousStatus,
            newStatus,
            reason,
            changedByUserId,
            changedAtUtc);
    }
}
