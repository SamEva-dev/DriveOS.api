using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;

public sealed class CalendarResource : AggregateRoot<CalendarResourceId>, IAuditableEntity
{
    private CalendarResource() { }

    private CalendarResource(
        CalendarResourceId id,
        OrganizationId organizationId,
        BranchId? branchId,
        CalendarResourceType resourceType,
        Guid externalResourceId,
        string displayName,
        int capacity,
        string timeZoneId)
        : base(id)
    {
        OrganizationId = organizationId;
        BranchId = branchId;
        ResourceType = resourceType;
        ExternalResourceId = externalResourceId;
        DisplayName = displayName;
        Capacity = capacity;
        TimeZoneId = timeZoneId;
        Status = CalendarResourceStatus.Active;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public CalendarResourceType ResourceType { get; private set; }
    public Guid ExternalResourceId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public string TimeZoneId { get; private set; } = string.Empty;
    public CalendarResourceStatus Status { get; private set; }
    public string? RestrictionReason { get; private set; }
    public string? UnavailabilityReason { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<CalendarResource> Create(
        CalendarResourceId id,
        OrganizationId organizationId,
        BranchId? branchId,
        CalendarResourceType resourceType,
        Guid externalResourceId,
        string displayName,
        int capacity,
        string timeZoneId)
    {
        if (id.IsEmpty)
            return Result.Failure<CalendarResource>(CalendarResourceErrors.InvalidIdentifier);
        if (organizationId.IsEmpty)
            return Result.Failure<CalendarResource>(CalendarResourceErrors.InvalidOrganization);
        if (branchId.HasValue && branchId.Value.IsEmpty)
            return Result.Failure<CalendarResource>(CalendarResourceErrors.InvalidBranch);
        if (externalResourceId == Guid.Empty)
            return Result.Failure<CalendarResource>(CalendarResourceErrors.InvalidExternalResource);
        if (!Enum.IsDefined(resourceType))
            return Result.Failure<CalendarResource>(CalendarResourceErrors.InvalidType);

        string normalizedDisplayName = displayName?.Trim() ?? string.Empty;
        if (normalizedDisplayName.Length is < 1 or > 200)
            return Result.Failure<CalendarResource>(CalendarResourceErrors.InvalidDisplayName);
        if (!CalendarResourceCapacityPolicy.IsValid(resourceType, capacity))
            return Result.Failure<CalendarResource>(CalendarResourceErrors.InvalidCapacity);

        string normalizedTimeZone = timeZoneId?.Trim() ?? string.Empty;
        if (normalizedTimeZone.Length is < 1 or > 100)
            return Result.Failure<CalendarResource>(CalendarResourceErrors.InvalidTimeZone);

        var resource = new CalendarResource(
            id,
            organizationId,
            branchId,
            resourceType,
            externalResourceId,
            normalizedDisplayName,
            capacity,
            normalizedTimeZone);

        resource.RaiseDomainEvent(new CalendarResourceCreatedDomainEvent(
            resource.Id,
            resource.OrganizationId,
            resource.ResourceType,
            resource.ExternalResourceId));

        return Result.Success(resource);
    }

    public Result UpdateMetadata(
        BranchId? branchId,
        string displayName,
        int capacity,
        string timeZoneId)
    {
        if (Status == CalendarResourceStatus.Archived)
            return Result.Failure(CalendarResourceErrors.ModificationNotAllowed);

        string normalizedDisplayName = displayName?.Trim() ?? string.Empty;
        if (normalizedDisplayName.Length is < 1 or > 200)
            return Result.Failure(CalendarResourceErrors.InvalidDisplayName);
        if (!CalendarResourceCapacityPolicy.IsValid(ResourceType, capacity))
            return Result.Failure(CalendarResourceErrors.InvalidCapacity);

        string normalizedTimeZone = timeZoneId?.Trim() ?? string.Empty;
        if (normalizedTimeZone.Length is < 1 or > 100)
            return Result.Failure(CalendarResourceErrors.InvalidTimeZone);

        BranchId = branchId;
        DisplayName = normalizedDisplayName;
        Capacity = capacity;
        TimeZoneId = normalizedTimeZone;

        RaiseDomainEvent(new CalendarResourceUpdatedDomainEvent(Id, OrganizationId));
        return Result.Success();
    }

    public Result Restrict(string reason)
    {
        if (Status is CalendarResourceStatus.Archived or CalendarResourceStatus.Unavailable)
            return Result.Failure(CalendarResourceErrors.RestrictionNotAllowed);

        string normalizedReason = reason?.Trim() ?? string.Empty;
        if (normalizedReason.Length is < 1 or > 500)
            return Result.Failure(CalendarResourceErrors.RestrictionNotAllowed);

        Status = CalendarResourceStatus.Restricted;
        RestrictionReason = normalizedReason;
        UnavailabilityReason = null;

        RaiseDomainEvent(new CalendarResourceRestrictedDomainEvent(Id, OrganizationId, normalizedReason));
        return Result.Success();
    }

    public Result MarkUnavailable(string? reason)
    {
        if (Status == CalendarResourceStatus.Archived)
            return Result.Failure(CalendarResourceErrors.AvailabilityChangeNotAllowed);

        string? normalizedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        if (normalizedReason is { Length: > 500 })
            return Result.Failure(CalendarResourceErrors.AvailabilityChangeNotAllowed);

        Status = CalendarResourceStatus.Unavailable;
        RestrictionReason = null;
        UnavailabilityReason = normalizedReason;

        RaiseDomainEvent(new CalendarResourceUnavailableDomainEvent(Id, OrganizationId, normalizedReason));
        return Result.Success();
    }

    public Result Activate()
    {
        if (Status is CalendarResourceStatus.Archived or CalendarResourceStatus.Active)
            return Result.Failure(CalendarResourceErrors.ActivationNotAllowed);

        Status = CalendarResourceStatus.Active;
        RestrictionReason = null;
        UnavailabilityReason = null;

        RaiseDomainEvent(new CalendarResourceActivatedDomainEvent(Id, OrganizationId));
        return Result.Success();
    }

    public Result Archive()
    {
        if (Status == CalendarResourceStatus.Archived)
            return Result.Failure(CalendarResourceErrors.ArchiveNotAllowed);

        Status = CalendarResourceStatus.Archived;
        RestrictionReason = null;
        UnavailabilityReason = null;

        RaiseDomainEvent(new CalendarResourceArchivedDomainEvent(Id, OrganizationId));
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }
}
