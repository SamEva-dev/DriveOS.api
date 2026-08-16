using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Relationships;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Relationships;

public sealed record StudentRelationshipListResponse(
    Guid StudentId,
    IReadOnlyList<StudentRelationshipItem> Items
);

public sealed record StudentRelationshipItem(
    Guid Id,
    Guid PersonOrOrganizationId,
    RelatedPartyKind PartyKind,
    string DisplayName,
    string? Email,
    string? Phone,
    StudentRelationshipType RelationshipType,
    StudentRelationshipPermissions Permissions,
    FinancialScope FinancialScope,
    CommunicationScope CommunicationScope,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsPrimaryPayer,
    StudentRelationshipStatus Status,
    DateTimeOffset? InvitedAtUtc,
    string? StatusReason
);

public sealed record GetStudentRelationshipsQuery(OrganizationId OrganizationId, PersonId StudentId)
    : IQuery<StudentRelationshipListResponse>;

public sealed record CreateStudentRelationshipCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid PartyId,
    RelatedPartyKind PartyKind,
    string DisplayName,
    string? Email,
    string? Phone,
    StudentRelationshipType RelationshipType,
    StudentRelationshipPermissions Permissions,
    FinancialScope FinancialScope,
    CommunicationScope CommunicationScope,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsPrimaryPayer,
    bool CanManagePayers,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record UpdateStudentRelationshipCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid RelationshipId,
    StudentRelationshipType RelationshipType,
    StudentRelationshipPermissions Permissions,
    FinancialScope FinancialScope,
    CommunicationScope CommunicationScope,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsPrimaryPayer,
    bool CanManagePayers,
    UserId ActorUserId
) : ICommand;

public sealed record SuspendStudentRelationshipCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid RelationshipId,
    string Reason,
    UserId ActorUserId
) : ICommand;

public sealed record RevokeStudentRelationshipCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid RelationshipId,
    string Reason,
    UserId ActorUserId
) : ICommand;

public sealed record InviteStudentRelationshipCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid RelationshipId,
    UserId ActorUserId
) : ICommand;

public interface IStudentRelationshipService
{
    Task<StudentRelationshipListResponse?> GetAsync(
        OrganizationId org,
        PersonId studentId,
        CancellationToken ct = default
    );
    Task<Result<Guid>> CreateAsync(
        CreateStudentRelationshipCommand command,
        CancellationToken ct = default
    );
    Task<Result> UpdateAsync(
        UpdateStudentRelationshipCommand command,
        CancellationToken ct = default
    );
    Task<Result> SuspendAsync(
        SuspendStudentRelationshipCommand command,
        CancellationToken ct = default
    );
    Task<Result> RevokeAsync(
        RevokeStudentRelationshipCommand command,
        CancellationToken ct = default
    );
    Task<Result> InviteAsync(
        InviteStudentRelationshipCommand command,
        CancellationToken ct = default
    );
}

public static class StudentRelationshipApplicationErrors
{
    public static readonly Error StudentNotFound = Error.NotFound(
        "Students.Relationships.Student.NotFound",
        "errors.students.relationships.student.notFound"
    );
    public static readonly Error Duplicate = Error.Conflict(
        "Students.Relationships.Duplicate",
        "errors.students.relationships.duplicate"
    );
    public static readonly Error PayerManagementForbidden = Error.Forbidden(
        "Students.Relationships.Payers.Forbidden",
        "errors.students.relationships.payers.forbidden"
    );
}
