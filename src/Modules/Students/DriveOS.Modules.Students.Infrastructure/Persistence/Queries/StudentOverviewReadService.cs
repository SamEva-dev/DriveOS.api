using DriveOS.Modules.Students.Application.Students.GetStudentOverview;
using DriveOS.Modules.Students.Domain.Enrollments;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Queries;

internal sealed class StudentOverviewReadService(StudentsDbContext db) : IStudentOverviewReadService
{
    public async Task<StudentOverviewResponse?> GetAsync(
        GetStudentOverviewQuery request,
        CancellationToken cancellationToken = default
    )
    {
        StudentProfileSummary? profile = await db
            .Students.AsNoTracking()
            .Where(x => x.OrganizationId == request.OrganizationId && x.Id == request.StudentId)
            .Select(x => new StudentProfileSummary(
                x.Id.Value,
                x.FirstName,
                x.LastName,
                x.Email,
                x.Phone,
                x.Status,
                x.CreatedAtUtc
            ))
            .SingleOrDefaultAsync(cancellationToken);
        if (profile is null)
            return null;

        ActiveEnrollmentSummary? activeEnrollment = null;
        if (request.Scope.CanReadEnrollment)
        {
            activeEnrollment = await db
                .Enrollments.AsNoTracking()
                .Where(x =>
                    x.OrganizationId == request.OrganizationId
                    && x.StudentId == request.StudentId
                    && x.Status != EnrollmentStatus.Cancelled
                )
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new ActiveEnrollmentSummary(
                    x.Id.Value,
                    x.BranchId.Value,
                    x.TrainingCode,
                    x.Status,
                    x.Source,
                    x.CreatedAtUtc
                ))
                .FirstOrDefaultAsync(cancellationToken);
        }

        IReadOnlyList<OverviewSection> sections = BuildSections(profile.StudentId, request.Scope);
        IReadOnlyList<OverviewAction> actions = BuildActions(profile.StudentId, request.Scope);
        OverviewAlert[] alerts =
            activeEnrollment is null && request.Scope.CanReadEnrollment
                ?
                [
                    new OverviewAlert(
                        "NoEnrollment",
                        "warning",
                        "students.overview.alerts.noEnrollment"
                    ),
                ]
                : [];
        OverviewActivity[] activity = activeEnrollment is null
            ? []
            :
            [
                new OverviewActivity(
                    "EnrollmentCreated",
                    activeEnrollment.StartedAtUtc,
                    "students.overview.activity.enrollmentCreated"
                ),
            ];
        return new StudentOverviewResponse(
            profile,
            activeEnrollment,
            sections,
            actions,
            alerts,
            activity
        );
    }

    private static IReadOnlyList<OverviewSection> BuildSections(Guid id, StudentOverviewReadScope s)
    {
        string root = $"/app/students/{id}";
        return
        [
            Available("overview", $"{root}/overview", true),
            Pending("planning", $"{root}/planning", s.CanReadPlanning),
            Pending("pedagogy", $"{root}/pedagogy", s.CanReadPedagogy),
            Pending("theory", $"{root}/theory", s.CanReadPedagogy),
            Pending("documents", $"{root}/documents", s.CanReadDocuments),
            Pending("contracts", $"{root}/contracts", s.CanReadAdministration),
            Pending("finance", $"{root}/finance", s.CanReadFinance),
            Pending("exams", $"{root}/exams", s.CanReadExams),
            Pending("communications", $"{root}/communications", s.CanReadCommunications),
            Pending("incidents", $"{root}/incidents", s.CanReadIncidents),
            Pending("partners", $"{root}/partners", s.CanReadPartners),
            Pending("history", $"{root}/history", s.CanReadHistory),
        ];
    }

    private static IReadOnlyList<OverviewAction> BuildActions(
        Guid id,
        StudentOverviewReadScope s
    ) =>
        [
            new("Plan", $"/app/students/{id}/planning", s.CanPlan),
            new("Call", $"/app/students/{id}/communications", s.CanCommunicate),
            new("Message", $"/app/students/{id}/communications", s.CanCommunicate),
            new("AddPayment", $"/app/students/{id}/finance", s.CanAddPayment),
            new("CreateDocument", $"/app/students/{id}/documents", s.CanCreateDocument),
        ];

    private static OverviewSection Available(string code, string route, bool authorized) =>
        new(
            code,
            route,
            authorized,
            authorized,
            authorized ? null : "errors.authorization.forbidden"
        );

    private static OverviewSection Pending(string code, string route, bool authorized) =>
        new(
            code,
            route,
            authorized,
            false,
            authorized
                ? "students.overview.section.notImplemented"
                : "errors.authorization.forbidden"
        );
}
