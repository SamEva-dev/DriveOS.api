using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Closures;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Closures;

public sealed record EnrollmentClosureResponse(
    Guid ClosureId,
    Guid EnrollmentId,
    EnrollmentClosureReason Reason,
    DateOnly ClosureDate,
    string ReasonDetail,
    EnrollmentClosureStatus Status,
    DateTimeOffset? ClosedAtUtc,
    DateTimeOffset? ArchivedAtUtc,
    DateOnly? RetainUntil,
    string? RetentionLegalBasis,
    StudentDataRetentionScope RetentionScope,
    DateTimeOffset? ReopenedAtUtc,
    string? ReopenJustification,
    IReadOnlyList<EnrollmentClosureCheckItem> Checks
);

public sealed record EnrollmentClosureCheckItem(
    EnrollmentClosureCheckType Type,
    EnrollmentClosureCheckStatus Status,
    string Detail
);

public sealed record GetEnrollmentClosuresQuery(OrganizationId OrganizationId, PersonId StudentId)
    : IQuery<IReadOnlyList<EnrollmentClosureResponse>>;

public sealed record CreateEnrollmentClosureCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid EnrollmentId,
    EnrollmentClosureReason Reason,
    DateOnly ClosureDate,
    string ReasonDetail,
    IReadOnlyList<EnrollmentClosureCheckSeed> Checks,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record ReviewEnrollmentClosureCheckCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid ClosureId,
    EnrollmentClosureCheckType Type,
    EnrollmentClosureCheckStatus Status,
    string Detail,
    UserId ActorUserId
) : ICommand;

public sealed record CloseEnrollmentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid ClosureId,
    UserId ActorUserId
) : ICommand;

public sealed record ArchiveStudentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid ClosureId,
    DateOnly RetainUntil,
    string RetentionLegalBasis,
    StudentDataRetentionScope RetentionScope,
    UserId ActorUserId
) : ICommand;

public sealed record ReopenEnrollmentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid ClosureId,
    string Justification,
    UserId ActorUserId
) : ICommand;

public interface IEnrollmentClosureService
{
    Task<IReadOnlyList<EnrollmentClosureResponse>> GetAsync(
        GetEnrollmentClosuresQuery query,
        CancellationToken ct = default
    );
    Task<Result<Guid>> CreateAsync(
        CreateEnrollmentClosureCommand command,
        CancellationToken ct = default
    );
    Task<Result> ReviewCheckAsync(
        ReviewEnrollmentClosureCheckCommand command,
        CancellationToken ct = default
    );
    Task<Result> CloseAsync(CloseEnrollmentCommand command, CancellationToken ct = default);
    Task<Result> ArchiveAsync(ArchiveStudentCommand command, CancellationToken ct = default);
    Task<Result> ReopenAsync(ReopenEnrollmentCommand command, CancellationToken ct = default);
}
