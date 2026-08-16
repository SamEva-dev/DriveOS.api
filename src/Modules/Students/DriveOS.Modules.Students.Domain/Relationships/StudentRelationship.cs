using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.Students.Domain.Events;

namespace DriveOS.Modules.Students.Domain.Relationships;

public sealed class StudentRelationship : AggregateRoot<StudentRelationshipId>
{
    private StudentRelationship() { }

    private StudentRelationship(
        StudentRelationshipId id,
        OrganizationId org,
        PersonId studentId,
        Guid partyId,
        RelatedPartyKind partyKind,
        string displayName,
        string? email,
        string? phone,
        StudentRelationshipType type,
        StudentRelationshipPermissions permissions,
        FinancialScope financialScope,
        CommunicationScope communicationScope,
        DateOnly from,
        DateOnly? to,
        bool primary,
        UserId actor,
        DateTimeOffset now
    )
        : base(id)
    {
        OrganizationId = org;
        StudentId = studentId;
        PersonOrOrganizationId = partyId;
        PartyKind = partyKind;
        DisplayName = displayName;
        Email = email;
        Phone = phone;
        RelationshipType = type;
        Permissions = permissions;
        FinancialScope = financialScope;
        CommunicationScope = communicationScope;
        EffectiveFrom = from;
        EffectiveTo = to;
        IsPrimaryPayer = primary;
        Status = StudentRelationshipStatus.Active;
        CreatedByUserId = actor;
        CreatedAtUtc = now;
        RaiseDomainEvent(new StudentAggregateChangedDomainEvent<StudentRelationshipId>(Id, StudentId, OrganizationId, "Created"));
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public Guid PersonOrOrganizationId { get; private set; }
    public RelatedPartyKind PartyKind { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public StudentRelationshipType RelationshipType { get; private set; }
    public StudentRelationshipPermissions Permissions { get; private set; }
    public FinancialScope FinancialScope { get; private set; }
    public CommunicationScope CommunicationScope { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public bool IsPrimaryPayer { get; private set; }
    public StudentRelationshipStatus Status { get; private set; }
    public DateTimeOffset? InvitedAtUtc { get; private set; }
    public UserId? InvitedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public DateTimeOffset? ModifiedAtUtc { get; private set; }
    public UserId? ModifiedByUserId { get; private set; }
    public string? StatusReason { get; private set; }

    public static Result<StudentRelationship> Create(
        OrganizationId org,
        PersonId studentId,
        Guid partyId,
        RelatedPartyKind partyKind,
        string displayName,
        string? email,
        string? phone,
        StudentRelationshipType type,
        StudentRelationshipPermissions permissions,
        FinancialScope financialScope,
        CommunicationScope communicationScope,
        DateOnly from,
        DateOnly? to,
        bool primary,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (org.IsEmpty || studentId.IsEmpty || partyId == Guid.Empty)
            return Result.Failure<StudentRelationship>(StudentRelationshipErrors.InvalidOwner);
        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Failure<StudentRelationship>(StudentRelationshipErrors.RequiredData);
        if (to.HasValue && to < from)
            return Result.Failure<StudentRelationship>(StudentRelationshipErrors.InvalidPeriod);
        if (primary && type != StudentRelationshipType.Payer)
            return Result.Failure<StudentRelationship>(
                StudentRelationshipErrors.InvalidPrimaryPayer
            );
        return Result.Success(
            new StudentRelationship(
                StudentRelationshipId.New(),
                org,
                studentId,
                partyId,
                partyKind,
                displayName.Trim(),
                Norm(email),
                Norm(phone),
                type,
                permissions,
                financialScope,
                communicationScope,
                from,
                to,
                primary,
                actor,
                now
            )
        );
    }

    public Result Update(
        StudentRelationshipType type,
        StudentRelationshipPermissions permissions,
        FinancialScope financialScope,
        CommunicationScope communicationScope,
        DateOnly from,
        DateOnly? to,
        bool primary,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (Status == StudentRelationshipStatus.Revoked)
            return Result.Failure(StudentRelationshipErrors.Revoked);
        if (to.HasValue && to < from)
            return Result.Failure(StudentRelationshipErrors.InvalidPeriod);
        if (primary && type != StudentRelationshipType.Payer)
            return Result.Failure(StudentRelationshipErrors.InvalidPrimaryPayer);
        RelationshipType = type;
        Permissions = permissions;
        FinancialScope = financialScope;
        CommunicationScope = communicationScope;
        EffectiveFrom = from;
        EffectiveTo = to;
        IsPrimaryPayer = primary;
        Touch(actor, now);
        return Result.Success();
    }

    public Result Invite(UserId actor, DateTimeOffset now)
    {
        if (Status != StudentRelationshipStatus.Active)
            return Result.Failure(StudentRelationshipErrors.NotActive);
        if (string.IsNullOrWhiteSpace(Email))
            return Result.Failure(StudentRelationshipErrors.InvitationContactRequired);
        InvitedAtUtc = now;
        InvitedByUserId = actor;
        Touch(actor, now);
        return Result.Success();
    }

    public Result Suspend(string reason, UserId actor, DateTimeOffset now)
    {
        if (Status != StudentRelationshipStatus.Active)
            return Result.Failure(StudentRelationshipErrors.NotActive);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StudentRelationshipErrors.ReasonRequired);
        Status = StudentRelationshipStatus.Suspended;
        StatusReason = reason.Trim();
        IsPrimaryPayer = false;
        Touch(actor, now);
        return Result.Success();
    }

    public Result Revoke(string reason, UserId actor, DateTimeOffset now)
    {
        if (Status == StudentRelationshipStatus.Revoked)
            return Result.Failure(StudentRelationshipErrors.Revoked);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StudentRelationshipErrors.ReasonRequired);
        Status = StudentRelationshipStatus.Revoked;
        StatusReason = reason.Trim();
        IsPrimaryPayer = false;
        Touch(actor, now);
        return Result.Success();
    }

    public void ClearPrimaryPayer(UserId actor, DateTimeOffset now)
    {
        if (!IsPrimaryPayer)
            return;
        IsPrimaryPayer = false;
        Touch(actor, now);
    }

    private void Touch(UserId actor, DateTimeOffset now)
    {
        ModifiedByUserId = actor;
        ModifiedAtUtc = now;
    }

    private static string? Norm(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
