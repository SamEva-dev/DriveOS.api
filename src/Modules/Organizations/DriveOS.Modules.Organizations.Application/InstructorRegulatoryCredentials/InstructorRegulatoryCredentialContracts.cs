using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.InstructorRegulatoryCredentials;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.InstructorRegulatoryCredentials;

public sealed record InstructorRegulatoryCredentialResponse(Guid Id, Guid InstructorUserId, string CountryCode, string CredentialType,
    string Identifier, string IssuingAuthority, string? JurisdictionCode, DateOnly? IssuedOn, DateOnly? ExpiresOn,
    InstructorRegulatoryCredentialSource Source, InstructorRegulatoryCredentialStatus Status, DateTimeOffset DeclaredAtUtc,
    DateTimeOffset? VerifiedAtUtc, string? VerificationMethod, string? DecisionReason, DateTimeOffset? SupersededAtUtc, Guid? SupersededById);

public sealed record InstructorRegulatoryCredentialSnapshot(string CountryCode, string CredentialType, string Identifier,
    string IssuingAuthority, string? JurisdictionCode, DateOnly? IssuedOn, DateOnly? ExpiresOn, bool Verified);

public sealed record GetInstructorRegulatoryCredentialsQuery(OrganizationId OrganizationId, UserId InstructorUserId)
    : IQuery<IReadOnlyList<InstructorRegulatoryCredentialResponse>>;
public sealed record DeclareInstructorRegulatoryCredentialCommand(OrganizationId OrganizationId, UserId InstructorUserId,
    string CountryCode, string CredentialType, string Identifier, string IssuingAuthority, string? JurisdictionCode,
    DateOnly? IssuedOn, DateOnly? ExpiresOn, InstructorRegulatoryCredentialSource Source, UserId ActorUserId)
    : ICommand<InstructorRegulatoryCredentialResponse>;
public sealed record VerifyInstructorRegulatoryCredentialCommand(OrganizationId OrganizationId, UserId InstructorUserId,
    InstructorRegulatoryCredentialId CredentialId, string VerificationMethod, string? Reason, UserId ActorUserId)
    : ICommand<InstructorRegulatoryCredentialResponse>;
public sealed record RejectInstructorRegulatoryCredentialCommand(OrganizationId OrganizationId, UserId InstructorUserId,
    InstructorRegulatoryCredentialId CredentialId, string Reason, UserId ActorUserId)
    : ICommand<InstructorRegulatoryCredentialResponse>;

public interface IInstructorRegulatoryCredentialService
{
    Task<IReadOnlyList<InstructorRegulatoryCredentialResponse>> GetAsync(OrganizationId organizationId, UserId instructorUserId, CancellationToken ct = default);
    Task<Result<InstructorRegulatoryCredentialResponse>> DeclareAsync(DeclareInstructorRegulatoryCredentialCommand command, CancellationToken ct = default);
    Task<Result<InstructorRegulatoryCredentialResponse>> VerifyAsync(VerifyInstructorRegulatoryCredentialCommand command, CancellationToken ct = default);
    Task<Result<InstructorRegulatoryCredentialResponse>> RejectAsync(RejectInstructorRegulatoryCredentialCommand command, CancellationToken ct = default);
}

/// <summary>Read-only bridge used by national integrations until BC-12 Workforce becomes the professional source of truth.</summary>
public interface IInstructorRegulatoryCredentialReadService
{
    Task<InstructorRegulatoryCredentialSnapshot?> ResolveCurrentAsync(OrganizationId organizationId, UserId instructorUserId,
        string countryCode, string credentialType, CancellationToken ct = default);
}
