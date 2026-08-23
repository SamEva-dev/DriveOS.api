using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.RegulatoryIdentities;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.RegulatoryIdentities;

public sealed record StudentRegulatoryIdentityResponse(
    Guid Id,
    Guid StudentId,
    string CountryCode,
    string IdentifierType,
    string Value,
    StudentRegulatoryIdentitySource Source,
    StudentRegulatoryIdentityStatus Status,
    DateTimeOffset DeclaredAtUtc,
    DateTimeOffset? VerifiedAtUtc,
    string? VerificationMethod,
    string? DecisionReason,
    DateTimeOffset? SupersededAtUtc,
    Guid? SupersededById);

public sealed record StudentRegulatoryIdentifierSnapshot(
    string CountryCode,
    string IdentifierType,
    string Value,
    bool Verified,
    DateTimeOffset DeclaredAtUtc,
    DateTimeOffset? VerifiedAtUtc);

public sealed record GetStudentRegulatoryIdentitiesQuery(OrganizationId OrganizationId, PersonId StudentId)
    : IQuery<IReadOnlyList<StudentRegulatoryIdentityResponse>>;

public sealed record DeclareStudentRegulatoryIdentityCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    string CountryCode,
    string IdentifierType,
    string Value,
    StudentRegulatoryIdentitySource Source,
    UserId ActorUserId)
    : ICommand<StudentRegulatoryIdentityResponse>;

public sealed record VerifyStudentRegulatoryIdentityCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    StudentRegulatoryIdentityId IdentityId,
    string VerificationMethod,
    string? Reason,
    UserId ActorUserId)
    : ICommand<StudentRegulatoryIdentityResponse>;

public sealed record RejectStudentRegulatoryIdentityCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    StudentRegulatoryIdentityId IdentityId,
    string Reason,
    UserId ActorUserId)
    : ICommand<StudentRegulatoryIdentityResponse>;

public interface IStudentRegulatoryIdentityService
{
    Task<IReadOnlyList<StudentRegulatoryIdentityResponse>> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken cancellationToken = default);

    Task<Result<StudentRegulatoryIdentityResponse>> DeclareAsync(
        DeclareStudentRegulatoryIdentityCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<StudentRegulatoryIdentityResponse>> VerifyAsync(
        VerifyStudentRegulatoryIdentityCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<StudentRegulatoryIdentityResponse>> RejectAsync(
        RejectStudentRegulatoryIdentityCommand command,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Read-only cross-context projection used by regulatory integration adapters.
/// Consumers never access the Students DbContext directly.
/// </summary>
public interface IStudentRegulatoryIdentityReadService
{
    Task<StudentRegulatoryIdentifierSnapshot?> ResolveCurrentAsync(
        OrganizationId organizationId,
        PersonId studentId,
        string countryCode,
        string identifierType,
        CancellationToken cancellationToken = default);
}
