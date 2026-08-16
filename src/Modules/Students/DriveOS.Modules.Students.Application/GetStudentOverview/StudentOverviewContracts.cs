using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Students.Application.Students.GetStudentOverview;

public sealed record GetStudentOverviewQuery(
    OrganizationId OrganizationId,
    PersonId StudentId,
    StudentOverviewReadScope Scope
) : IQuery<StudentOverviewResponse>;

public sealed record StudentOverviewReadScope(
    bool CanReadEnrollment,
    bool CanReadAdministration,
    bool CanReadFinance,
    bool CanReadPedagogy,
    bool CanReadPlanning,
    bool CanReadExams,
    bool CanReadDocuments,
    bool CanReadCommunications,
    bool CanReadIncidents,
    bool CanReadPartners,
    bool CanReadHistory,
    bool CanPlan,
    bool CanAddPayment,
    bool CanCreateDocument,
    bool CanCommunicate
);

public sealed record StudentOverviewResponse(
    StudentProfileSummary Profile,
    ActiveEnrollmentSummary? ActiveEnrollment,
    IReadOnlyList<OverviewSection> Sections,
    IReadOnlyList<OverviewAction> Actions,
    IReadOnlyList<OverviewAlert> Alerts,
    IReadOnlyList<OverviewActivity> RecentActivity
);

public sealed record StudentProfileSummary(
    Guid StudentId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    StudentStatus Status,
    DateTimeOffset CreatedAtUtc
);

public sealed record ActiveEnrollmentSummary(
    Guid EnrollmentId,
    Guid BranchId,
    string TrainingCode,
    EnrollmentStatus Status,
    EnrollmentSource Source,
    DateTimeOffset StartedAtUtc
);

public sealed record OverviewSection(
    string Code,
    string Route,
    bool IsAuthorized,
    bool IsAvailable,
    string? UnavailableReasonKey
);

public sealed record OverviewAction(string Code, string Route, bool IsEnabled);

public sealed record OverviewAlert(string Code, string Severity, string MessageKey);

public sealed record OverviewActivity(string Type, DateTimeOffset OccurredAtUtc, string LabelKey);

public interface IStudentOverviewReadService
{
    Task<StudentOverviewResponse?> GetAsync(
        GetStudentOverviewQuery query,
        CancellationToken cancellationToken = default
    );
}
