using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Guardians;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Guardians;

public sealed record GuardianListResponse(
    Guid StudentId,
    bool GuardianRightsReviewRequired,
    IReadOnlyList<GuardianItem> Items
);

public sealed record GuardianItem(
    Guid Id,
    Guid GuardianPersonId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    GuardianRelationshipType RelationshipType,
    string LegalBasis,
    ParentalAuthorityStatus ParentalAuthorityStatus,
    GuardianPermissions Permissions,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool FinancialRights,
    bool SignatureRights,
    string NotificationPreferences,
    GuardianRelationshipStatus Status,
    DateTimeOffset? InvitedAtUtc
);

public sealed record GetGuardiansQuery(OrganizationId OrganizationId, PersonId StudentId)
    : IQuery<GuardianListResponse>;

public sealed record CreateGuardianCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    PersonId GuardianPersonId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    GuardianRelationshipType RelationshipType,
    string LegalBasis,
    ParentalAuthorityStatus ParentalAuthorityStatus,
    GuardianPermissions Permissions,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool FinancialRights,
    bool SignatureRights,
    string NotificationPreferences,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record UpdateGuardianCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid RelationshipId,
    GuardianRelationshipType RelationshipType,
    string LegalBasis,
    ParentalAuthorityStatus ParentalAuthorityStatus,
    GuardianPermissions Permissions,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool FinancialRights,
    bool SignatureRights,
    string NotificationPreferences,
    UserId ActorUserId
) : ICommand;

public sealed record RevokeGuardianCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid RelationshipId,
    string Reason,
    UserId ActorUserId
) : ICommand;

public sealed record InviteGuardianCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid RelationshipId,
    UserId ActorUserId
) : ICommand;

public interface IGuardianService
{
    Task<GuardianListResponse?> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken ct = default
    );
    Task<Result<Guid>> CreateAsync(CreateGuardianCommand command, CancellationToken ct = default);
    Task<Result> UpdateAsync(UpdateGuardianCommand command, CancellationToken ct = default);
    Task<Result> RevokeAsync(RevokeGuardianCommand command, CancellationToken ct = default);
    Task<Result> InviteAsync(InviteGuardianCommand command, CancellationToken ct = default);
}

public static class GuardianApplicationErrors
{
    public static readonly Error StudentNotFound = Error.NotFound(
        "Students.Guardians.Student.NotFound",
        "errors.students.guardians.student.notFound"
    );
    public static readonly Error Duplicate = Error.Conflict(
        "Students.Guardians.Duplicate",
        "errors.students.guardians.duplicate"
    );
}
