using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.Students.Domain.Events;

namespace DriveOS.Modules.Students.Domain.Guardians;

public sealed class GuardianRelationship : AggregateRoot<GuardianRelationshipId>
{
    private GuardianRelationship() { }

    private GuardianRelationship(
        GuardianRelationshipId id,
        OrganizationId org,
        PersonId studentId,
        PersonId guardianId,
        string firstName,
        string lastName,
        string? email,
        string? phone,
        GuardianRelationshipType type,
        string legalBasis,
        ParentalAuthorityStatus authority,
        GuardianPermissions permissions,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool financialRights,
        bool signatureRights,
        string notificationPreferences,
        UserId actor,
        DateTimeOffset now
    )
        : base(id)
    {
        OrganizationId = org;
        StudentId = studentId;
        GuardianPersonId = guardianId;
        GuardianFirstName = firstName;
        GuardianLastName = lastName;
        GuardianEmail = email;
        GuardianPhone = phone;
        RelationshipType = type;
        LegalBasis = legalBasis;
        ParentalAuthorityStatus = authority;
        Permissions = permissions;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        FinancialRights = financialRights;
        SignatureRights = signatureRights;
        NotificationPreferences = notificationPreferences;
        Status = GuardianRelationshipStatus.Active;
        CreatedByUserId = actor;
        CreatedAtUtc = now;
        RaiseDomainEvent(new StudentAggregateChangedDomainEvent<GuardianRelationshipId>(Id, StudentId, OrganizationId, "Created"));
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public PersonId GuardianPersonId { get; private set; }
    public string GuardianFirstName { get; private set; } = string.Empty;
    public string GuardianLastName { get; private set; } = string.Empty;
    public string? GuardianEmail { get; private set; }
    public string? GuardianPhone { get; private set; }
    public GuardianRelationshipType RelationshipType { get; private set; }
    public string LegalBasis { get; private set; } = string.Empty;
    public ParentalAuthorityStatus ParentalAuthorityStatus { get; private set; }
    public GuardianPermissions Permissions { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public bool FinancialRights { get; private set; }
    public bool SignatureRights { get; private set; }
    public string NotificationPreferences { get; private set; } = string.Empty;
    public GuardianRelationshipStatus Status { get; private set; }
    public DateTimeOffset? InvitedAtUtc { get; private set; }
    public UserId? InvitedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public DateTimeOffset? ModifiedAtUtc { get; private set; }
    public UserId? ModifiedByUserId { get; private set; }
    public string? RevocationReason { get; private set; }

    public static Result<GuardianRelationship> Create(
        OrganizationId org,
        PersonId studentId,
        PersonId guardianId,
        string firstName,
        string lastName,
        string? email,
        string? phone,
        GuardianRelationshipType type,
        string legalBasis,
        ParentalAuthorityStatus authority,
        GuardianPermissions permissions,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool financialRights,
        bool signatureRights,
        string notificationPreferences,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (org.IsEmpty || studentId.IsEmpty || guardianId.IsEmpty || studentId == guardianId)
            return Result.Failure<GuardianRelationship>(GuardianErrors.InvalidOwner);
        if (
            string.IsNullOrWhiteSpace(firstName)
            || string.IsNullOrWhiteSpace(lastName)
            || string.IsNullOrWhiteSpace(legalBasis)
        )
            return Result.Failure<GuardianRelationship>(GuardianErrors.RequiredData);
        if (effectiveTo.HasValue && effectiveTo < effectiveFrom)
            return Result.Failure<GuardianRelationship>(GuardianErrors.InvalidPeriod);
        return Result.Success(
            new GuardianRelationship(
                GuardianRelationshipId.New(),
                org,
                studentId,
                guardianId,
                firstName.Trim(),
                lastName.Trim(),
                Norm(email),
                Norm(phone),
                type,
                legalBasis.Trim(),
                authority,
                permissions,
                effectiveFrom,
                effectiveTo,
                financialRights,
                signatureRights,
                notificationPreferences?.Trim() ?? string.Empty,
                actor,
                now
            )
        );
    }

    public Result Update(
        GuardianRelationshipType type,
        string legalBasis,
        ParentalAuthorityStatus authority,
        GuardianPermissions permissions,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool financialRights,
        bool signatureRights,
        string notificationPreferences,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (Status == GuardianRelationshipStatus.Revoked)
            return Result.Failure(GuardianErrors.Revoked);
        if (string.IsNullOrWhiteSpace(legalBasis))
            return Result.Failure(GuardianErrors.RequiredData);
        if (effectiveTo.HasValue && effectiveTo < effectiveFrom)
            return Result.Failure(GuardianErrors.InvalidPeriod);
        RelationshipType = type;
        LegalBasis = legalBasis.Trim();
        ParentalAuthorityStatus = authority;
        Permissions = permissions;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        FinancialRights = financialRights;
        SignatureRights = signatureRights;
        NotificationPreferences = notificationPreferences?.Trim() ?? string.Empty;
        ModifiedByUserId = actor;
        ModifiedAtUtc = now;
        return Result.Success();
    }

    public Result Invite(UserId actor, DateTimeOffset now)
    {
        if (Status != GuardianRelationshipStatus.Active)
            return Result.Failure(GuardianErrors.NotActive);
        if (string.IsNullOrWhiteSpace(GuardianEmail))
            return Result.Failure(GuardianErrors.InvitationContactRequired);
        InvitedByUserId = actor;
        InvitedAtUtc = now;
        return Result.Success();
    }

    public Result Revoke(string reason, UserId actor, DateTimeOffset now)
    {
        if (Status == GuardianRelationshipStatus.Revoked)
            return Result.Failure(GuardianErrors.Revoked);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(GuardianErrors.ReasonRequired);
        Status = GuardianRelationshipStatus.Revoked;
        RevocationReason = reason.Trim();
        ModifiedByUserId = actor;
        ModifiedAtUtc = now;
        return Result.Success();
    }

    private static string? Norm(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
