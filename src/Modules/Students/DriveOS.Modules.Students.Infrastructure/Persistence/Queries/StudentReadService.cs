using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Students.Application.Students.GetStudents;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Students;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Queries;

internal sealed class StudentReadService(StudentsDbContext db) : IStudentReadService
{
    public async Task<PagedResult<StudentListItem>> GetPageAsync(
        GetStudentsQuery request,
        CancellationToken cancellationToken = default
    )
    {
        int pageNumber = Math.Max(1, request.PageNumber);
        int pageSize = Math.Clamp(request.PageSize, 10, 100);
        IQueryable<Student> query = db
            .Students.AsNoTracking()
            .Where(x => x.OrganizationId == request.OrganizationId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string pattern = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.FirstName, pattern)
                || EF.Functions.ILike(x.LastName, pattern)
                || x.Email != null && EF.Functions.ILike(x.Email, pattern)
                || x.Phone != null && EF.Functions.ILike(x.Phone, pattern)
            );
        }
        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);
        if (request.BranchId.HasValue || request.EnrollmentStatus.HasValue)
        {
            query = query.Where(student =>
                db.Enrollments.AsNoTracking()
                    .Any(enrollment =>
                        enrollment.OrganizationId == request.OrganizationId
                        && enrollment.StudentId == student.Id
                        && (
                            !request.BranchId.HasValue
                            || enrollment.BranchId == request.BranchId.Value
                        )
                        && (
                            !request.EnrollmentStatus.HasValue
                            || enrollment.Status == request.EnrollmentStatus.Value
                        )
                    )
            );
        }

        long totalCount = await query.LongCountAsync(cancellationToken);
        query = ApplySorting(query, request.SortBy, request.SortDirection);
        IQueryable<Student> pageQuery = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        var students = await pageQuery
            .Select(x => new
            {
                Id = x.Id,
                IdValue = x.Id.Value,
                x.FirstName,
                x.LastName,
                x.Email,
                x.Phone,
                x.Status,
                x.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var enrollments = await (
            from enrollment in db.Enrollments.AsNoTracking()
            join student in pageQuery on enrollment.StudentId equals student.Id
            orderby enrollment.CreatedAtUtc descending
            select new
            {
                StudentId = student.Id,
                EnrollmentId = enrollment.Id.Value,
                BranchId = enrollment.BranchId.Value,
                enrollment.TrainingCode,
                enrollment.Status,
            }
        ).ToListAsync(cancellationToken);
        var latestEnrollmentByStudent = enrollments
            .GroupBy(x => x.StudentId)
            .ToDictionary(x => x.Key, x => x.First());

        StudentListItem[] items = students
            .Select(student =>
            {
                latestEnrollmentByStudent.TryGetValue(student.Id, out var enrollment);
                return new StudentListItem(
                    student.IdValue,
                    student.FirstName,
                    student.LastName,
                    student.Email,
                    student.Phone,
                    student.Status,
                    enrollment?.EnrollmentId,
                    enrollment?.BranchId,
                    enrollment?.TrainingCode,
                    enrollment?.Status,
                    student.CreatedAtUtc
                );
            })
            .ToArray();
        return new PagedResult<StudentListItem>(items, pageNumber, pageSize, totalCount);
    }

    private static IQueryable<Student> ApplySorting(
        IQueryable<Student> query,
        StudentSortField field,
        SortDirection direction
    ) =>
        (field, direction) switch
        {
            (StudentSortField.CreatedAt, SortDirection.Ascending) => query.OrderBy(x =>
                x.CreatedAtUtc
            ),
            (StudentSortField.CreatedAt, _) => query.OrderByDescending(x => x.CreatedAtUtc),
            (StudentSortField.Status, SortDirection.Ascending) => query
                .OrderBy(x => x.Status)
                .ThenBy(x => x.LastName),
            (StudentSortField.Status, _) => query
                .OrderByDescending(x => x.Status)
                .ThenBy(x => x.LastName),
            (StudentSortField.Name, SortDirection.Descending) => query
                .OrderByDescending(x => x.LastName)
                .ThenByDescending(x => x.FirstName),
            _ => query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName),
        };
}
