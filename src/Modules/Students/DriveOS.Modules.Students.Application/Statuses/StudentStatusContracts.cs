using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Administration;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Statuses;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Statuses;

public sealed record StudentStatusesResponse(
    Guid StudentId,
    StudentStatus StudentProfileStatus,
    EnrollmentStatus? EnrollmentStatus,
    AdministrativeStatus AdministrativeStatus,
    FinancialStatus FinancialStatus,
    PedagogicalStatus PedagogicalStatus,
    SchedulingStatus SchedulingStatus,
    ExamStatus ExamStatus,
    PortalAccessStatus PortalAccessStatus,
    StudentBlockingAction CurrentlyBlockedActions,
    IReadOnlyList<StudentBlockItem> Blocks
);

public sealed record StudentBlockItem(
    Guid Id,
    string BlockType,
    string Reason,
    string SourceDomain,
    StudentBlockingAction BlockingActions,
    StudentBlockSeverity Severity,
    DateTimeOffset AppliedAtUtc,
    Guid AppliedByUserId,
    string ExpectedResolution,
    StudentBlockStatus Status,
    StudentBlockResolutionType? ResolutionType,
    string? ResolutionReason,
    DateTimeOffset? ResolvedAtUtc,
    DateTimeOffset? OverrideUntilUtc,
    string? OverrideReason
);

public sealed record GetStudentStatusesQuery(OrganizationId OrganizationId, PersonId StudentId)
    : IQuery<StudentStatusesResponse>;

public sealed record ApplyStudentBlockCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    string BlockType,
    string Reason,
    string SourceDomain,
    StudentBlockingAction BlockingActions,
    StudentBlockSeverity Severity,
    string ExpectedResolution,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record ReleaseStudentBlockCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid BlockId,
    StudentBlockResolutionType ResolutionType,
    string Reason,
    UserId ActorUserId
) : ICommand;

public sealed record OverrideStudentBlockCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid BlockId,
    string Reason,
    DateTimeOffset UntilUtc,
    UserId ActorUserId
) : ICommand;

public interface IStudentStatusService
{
    Task<StudentStatusesResponse?> GetAsync(
        OrganizationId org,
        PersonId studentId,
        CancellationToken ct = default
    );
    Task<Result<Guid>> ApplyBlockAsync(
        ApplyStudentBlockCommand command,
        CancellationToken ct = default
    );
    Task<Result> ReleaseBlockAsync(
        ReleaseStudentBlockCommand command,
        CancellationToken ct = default
    );
    Task<Result> OverrideBlockAsync(
        OverrideStudentBlockCommand command,
        CancellationToken ct = default
    );
}

public static class StudentStatusApplicationErrors
{
    public static readonly Error StudentNotFound = Error.NotFound(
        "Students.Statuses.Student.NotFound",
        "errors.students.statuses.student.notFound"
    );
}
