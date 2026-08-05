using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;

public sealed class OrganizationRepresentative :
    AggregateRoot<OrganizationRepresentativeId>,
    IAuditableEntity
{
    private OrganizationRepresentative() { }

    private OrganizationRepresentative(
        OrganizationRepresentativeId id,
        OrganizationId organizationId,
        PersonId personId,
        UserId? userId,
        OrganizationRepresentativeType representativeType,
        RepresentativeAuthorityScope authorityScope,
        bool isPrimaryOwner,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo)
        : base(id)
    {
        OrganizationId = organizationId;
        PersonId = personId;
        UserId = userId;
        RepresentativeType = representativeType;
        AuthorityScope = authorityScope;
        IsPrimaryOwner = isPrimaryOwner;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        Status = OrganizationRepresentativeStatus.Draft;
        Revision = 1;
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId PersonId { get; private set; }
    public UserId? UserId { get; private set; }
    public OrganizationRepresentativeType RepresentativeType { get; private set; }
    public RepresentativeAuthorityScope AuthorityScope { get; private set; } = null!;
    public bool IsPrimaryOwner { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public OrganizationRepresentativeStatus Status { get; private set; }
    public int Revision { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public bool IsOwner => RepresentativeType == OrganizationRepresentativeType.Owner;

    public static Result<OrganizationRepresentative> Create(
        OrganizationRepresentativeId id,
        OrganizationId organizationId,
        PersonId personId,
        UserId? userId,
        OrganizationRepresentativeType representativeType,
        RepresentativeAuthorityScope authorityScope,
        bool isPrimaryOwner,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo)
    {
        if (id.IsEmpty) return Result.Failure<OrganizationRepresentative>(OrganizationRepresentativeErrors.EmptyId);
        if (organizationId.IsEmpty) return Result.Failure<OrganizationRepresentative>(OrganizationRepresentativeErrors.EmptyOrganizationId);
        if (personId.IsEmpty) return Result.Failure<OrganizationRepresentative>(OrganizationRepresentativeErrors.EmptyPersonId);
        if (!Enum.IsDefined(representativeType)) return Result.Failure<OrganizationRepresentative>(OrganizationRepresentativeErrors.InvalidType);
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom) return Result.Failure<OrganizationRepresentative>(OrganizationRepresentativeErrors.InvalidEffectivePeriod);
        if (isPrimaryOwner && representativeType != OrganizationRepresentativeType.Owner)
            return Result.Failure<OrganizationRepresentative>(OrganizationRepresentativeErrors.PrimaryOwnerMustBeOwner);

        ArgumentNullException.ThrowIfNull(authorityScope);

        var representative = new OrganizationRepresentative(
            id, organizationId, personId, userId, representativeType,
            authorityScope, isPrimaryOwner, effectiveFrom, effectiveTo);

        representative.RaiseDomainEvent(new OrganizationRepresentativeCreatedDomainEvent(
            representative.Id,
            representative.OrganizationId,
            representative.PersonId,
            representative.RepresentativeType));

        return Result.Success(representative);
    }

    public Result Activate()
    {
        if (Status is OrganizationRepresentativeStatus.Ended or OrganizationRepresentativeStatus.Active)
            return Result.Failure(OrganizationRepresentativeErrors.InvalidStatusTransition);
        Status = OrganizationRepresentativeStatus.Active;
        Revision++;
        return Result.Success();
    }

    public Result Suspend(string? reason, UserId actorUserId)
    {
        if (Status != OrganizationRepresentativeStatus.Active || string.IsNullOrWhiteSpace(reason) || actorUserId.IsEmpty)
            return Result.Failure(OrganizationRepresentativeErrors.InvalidStatusTransition);
        ChangeStatus(OrganizationRepresentativeStatus.Suspended, reason.Trim(), actorUserId);
        return Result.Success();
    }

    public Result Reactivate(string? reason, UserId actorUserId)
    {
        if (Status != OrganizationRepresentativeStatus.Suspended || string.IsNullOrWhiteSpace(reason) || actorUserId.IsEmpty)
            return Result.Failure(OrganizationRepresentativeErrors.InvalidStatusTransition);
        ChangeStatus(OrganizationRepresentativeStatus.Active, reason.Trim(), actorUserId);
        return Result.Success();
    }

    public Result End(DateOnly effectiveTo, string? reason, UserId actorUserId, bool isLastActiveOwner)
    {
        if (Status == OrganizationRepresentativeStatus.Ended || effectiveTo < EffectiveFrom || string.IsNullOrWhiteSpace(reason) || actorUserId.IsEmpty)
            return Result.Failure(OrganizationRepresentativeErrors.InvalidStatusTransition);
        if (IsOwner && isLastActiveOwner)
            return Result.Failure(OrganizationRepresentativeErrors.LastActiveOwnerCannotBeEnded);

        EffectiveTo = effectiveTo;
        IsPrimaryOwner = false;
        ChangeStatus(OrganizationRepresentativeStatus.Ended, reason.Trim(), actorUserId);
        return Result.Success();
    }

    public Result UpdateAuthority(
        RepresentativeAuthorityScope authorityScope,
        UserId? userId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo)
    {
        ArgumentNullException.ThrowIfNull(authorityScope);
        if (Status == OrganizationRepresentativeStatus.Ended)
            return Result.Failure(OrganizationRepresentativeErrors.InvalidStatusTransition);
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            return Result.Failure(OrganizationRepresentativeErrors.InvalidEffectivePeriod);

        AuthorityScope = authorityScope;
        UserId = userId;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        Revision++;
        return Result.Success();
    }

    public Result SetPrimaryOwner()
    {
        if (!IsOwner || Status != OrganizationRepresentativeStatus.Active)
            return Result.Failure(OrganizationRepresentativeErrors.PrimaryOwnerMustBeOwner);
        IsPrimaryOwner = true;
        Revision++;
        return Result.Success();
    }

    public void ClearPrimaryOwner()
    {
        if (!IsPrimaryOwner) return;
        IsPrimaryOwner = false;
        Revision++;
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default) return;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedByUserId = modifiedByUserId;
    }

    private void ChangeStatus(OrganizationRepresentativeStatus newStatus, string reason, UserId actorUserId)
    {
        OrganizationRepresentativeStatus previous = Status;
        Status = newStatus;
        Revision++;
        RaiseDomainEvent(new OrganizationRepresentativeStatusChangedDomainEvent(
            Id, OrganizationId, previous, newStatus, actorUserId, reason));
    }
}
