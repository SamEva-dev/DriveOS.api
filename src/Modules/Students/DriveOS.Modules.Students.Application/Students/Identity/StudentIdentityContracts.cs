using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Students.Identity;

public sealed record StudentIdentityResponse(
    Guid StudentId,
    string LegalFirstName,
    string LegalLastName,
    string? PreferredName,
    DateOnly? BirthDate,
    string? BirthPlace,
    string? Nationality,
    string? Email,
    string? Phone,
    string? AddressLine1,
    string? AddressLine2,
    string? PostalCode,
    string? City,
    string? CountryCode,
    string? PreferredLanguage,
    string? TimeZone,
    bool AllowEmail,
    bool AllowSms,
    bool AllowPhone,
    IdentityVerificationStatus VerificationStatus,
    DateTimeOffset? VerifiedAtUtc
);

public sealed record UpdateStudentIdentityResponse(
    StudentIdentityResponse Identity,
    bool PotentialDuplicateDetected
);

public sealed record GetStudentIdentityQuery(OrganizationId OrganizationId, PersonId StudentId)
    : IQuery<StudentIdentityResponse>;

public sealed record UpdateStudentIdentityCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    StudentIdentityData Identity,
    string? Justification,
    UserId ActorUserId
) : ICommand<UpdateStudentIdentityResponse>;

public sealed record VerifyStudentIdentityCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    IdentityVerificationStatus Status,
    string Justification,
    UserId ActorUserId
) : ICommand<StudentIdentityResponse>;

public sealed record UpdateOwnStudentContactCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    string? Email,
    string? Phone,
    string? AddressLine1,
    string? AddressLine2,
    string? PostalCode,
    string? City,
    string? CountryCode,
    string? PreferredLanguage,
    string? TimeZone,
    bool AllowEmail,
    bool AllowSms,
    bool AllowPhone,
    UserId ActorUserId
) : ICommand<UpdateStudentIdentityResponse>;

public interface IStudentIdentityService
{
    Task<StudentIdentityResponse?> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken cancellationToken = default
    );
    Task<Result<UpdateStudentIdentityResponse>> UpdateAsync(
        UpdateStudentIdentityCommand command,
        CancellationToken cancellationToken = default
    );
    Task<Result<StudentIdentityResponse>> VerifyAsync(
        VerifyStudentIdentityCommand command,
        CancellationToken cancellationToken = default
    );
    Task<Result<UpdateStudentIdentityResponse>> UpdateOwnContactAsync(
        UpdateOwnStudentContactCommand command,
        CancellationToken cancellationToken = default
    );
}

public static class StudentIdentityErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Students.Identity.NotFound",
        "errors.students.identity.notFound"
    );
}
