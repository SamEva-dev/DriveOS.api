using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Suspensions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Suspensions;

public sealed record EnrollmentSuspensionResponse(
    Guid SuspensionId,
    Guid StudentId,
    Guid EnrollmentId,
    EnrollmentSuspensionReason Reason,
    EnrollmentSuspensionScope Scope,
    DateOnly StartDate,
    DateOnly ExpectedEndDate,
    string ImmediateActions,
    ExistingBookingsDecision BookingsDecision,
    int FutureBookingsCount,
    string CreditDecision,
    string NotificationPlan,
    DateOnly ReviewDate,
    EnrollmentSuspensionStatus Status,
    SuspensionNotificationStatus NotificationStatus,
    Guid? OperationalBlockId,
    IReadOnlyList<EnrollmentSuspensionHistoryItem> History
);

public sealed record EnrollmentSuspensionHistoryItem(
    string Action,
    string Detail,
    Guid ActorUserId,
    DateTimeOffset OccurredAtUtc
);

public sealed record GetEnrollmentSuspensionsQuery(
    OrganizationId OrganizationId,
    PersonId StudentId
) : IQuery<IReadOnlyList<EnrollmentSuspensionResponse>>;

public sealed record SuspendEnrollmentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    EnrollmentSuspensionReason Reason,
    EnrollmentSuspensionScope Scope,
    DateOnly StartDate,
    DateOnly ExpectedEndDate,
    string ImmediateActions,
    ExistingBookingsDecision BookingsDecision,
    int FutureBookingsCount,
    string CreditDecision,
    string NotificationPlan,
    DateOnly ReviewDate,
    UserId ActorUserId
) : ICommand<Guid>;

public interface IEnrollmentSuspensionService
{
    Task<IReadOnlyList<EnrollmentSuspensionResponse>> GetAsync(
        GetEnrollmentSuspensionsQuery query,
        CancellationToken ct = default
    );
    Task<Result<Guid>> SuspendAsync(
        SuspendEnrollmentCommand command,
        CancellationToken ct = default
    );
    Task ActivateDueAsync(CancellationToken ct = default);
}

public static class EnrollmentSuspensionApplicationErrors
{
    public static readonly Error StudentNotFound = Error.NotFound(
        "Students.Suspension.Student.NotFound",
        "errors.students.suspension.student.notFound"
    );
    public static readonly Error ActiveEnrollmentNotFound = Error.NotFound(
        "Students.Suspension.Enrollment.NotFound",
        "errors.students.suspension.enrollment.notFound"
    );
}
