using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Students.Application.Enrollments.StartDirectEnrollment;

public sealed record StartDirectEnrollmentCommand(
    OrganizationId OrganizationId,
    string IdempotencyKey,
    PersonId? ExistingStudentId,
    BranchId BranchId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string TrainingCode,
    EnrollmentSource Source,
    string RegulatoryCountryCode,
    string PreferredLanguageCode,
    bool RequiredConsentsAccepted
) : ICommand<StartDirectEnrollmentResponse>;

public sealed record StartDirectEnrollmentResponse(
    Guid StudentId,
    Guid EnrollmentId,
    bool StudentReused,
    bool IdempotentReplay
);

public interface IDirectEnrollmentService
{
    Task<DriveOS.SharedKernel.Results.Result<StartDirectEnrollmentResponse>> StartAsync(
        StartDirectEnrollmentCommand command,
        CancellationToken cancellationToken = default
    );
}
