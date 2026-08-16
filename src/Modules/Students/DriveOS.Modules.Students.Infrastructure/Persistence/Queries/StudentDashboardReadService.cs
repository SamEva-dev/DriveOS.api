using DriveOS.Modules.Students.Application.Dashboard.GetDashboard;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Queries;

internal sealed class StudentDashboardReadService(StudentsDbContext db)
    : IStudentDashboardReadService
{
    public async Task<StudentDashboardResponse> GetAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Enrollment> enrollments = db
            .Enrollments.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId);
        if (branchId.HasValue)
            enrollments = enrollments.Where(x => x.BranchId == branchId.Value);

        int draft = await enrollments.CountAsync(
            x => x.Status == EnrollmentStatus.Draft,
            cancellationToken
        );
        int pending = await enrollments.CountAsync(
            x => x.Status == EnrollmentStatus.PendingDocuments,
            cancellationToken
        );
        int ready = await enrollments.CountAsync(
            x => x.Status == EnrollmentStatus.ReadyForValidation,
            cancellationToken
        );
        int active = await (
            from enrollment in enrollments
            join student in db.Students.AsNoTracking() on enrollment.StudentId equals student.Id
            where student.Status == StudentStatus.Active
            select student.Id
        )
            .Distinct()
            .CountAsync(cancellationToken);

        var actions = await (
            from enrollment in enrollments
            join student in db.Students.AsNoTracking() on enrollment.StudentId equals student.Id
            where
                enrollment.Status == EnrollmentStatus.Draft
                || enrollment.Status == EnrollmentStatus.PendingDocuments
                || enrollment.Status == EnrollmentStatus.ReadyForValidation
            orderby enrollment.CreatedAtUtc
            select new StudentDashboardActionItem(
                enrollment.Id.Value,
                student.Id.Value,
                student.FirstName + " " + student.LastName,
                enrollment.TrainingCode,
                enrollment.Status.ToString(),
                enrollment.CreatedAtUtc
            )
        )
            .Take(20)
            .ToListAsync(cancellationToken);

        IQueryable<Student> recentQuery = db
            .Students.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId);
        if (branchId.HasValue)
        {
            recentQuery =
                from student in recentQuery
                join enrollment in enrollments on student.Id equals enrollment.StudentId
                select student;
        }
        var recent = await recentQuery
            .Distinct()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new RecentStudentItem(
                x.Id.Value,
                x.FirstName + " " + x.LastName,
                x.Email,
                x.Phone,
                x.CreatedAtUtc
            ))
            .Take(10)
            .ToListAsync(cancellationToken);
        return new(active, draft, pending, ready, actions, recent);
    }
}
