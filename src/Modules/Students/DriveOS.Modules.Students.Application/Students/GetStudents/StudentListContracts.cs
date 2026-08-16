using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Students.Application.Students.GetStudents;

public enum StudentSortField
{
    Name = 0,
    CreatedAt = 1,
    Status = 2,
}

public sealed record StudentListItem(
    Guid StudentId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    StudentStatus Status,
    Guid? EnrollmentId,
    Guid? BranchId,
    string? TrainingCode,
    EnrollmentStatus? EnrollmentStatus,
    DateTimeOffset CreatedAtUtc
);

public sealed record GetStudentsQuery(
    OrganizationId OrganizationId,
    int PageNumber,
    int PageSize,
    string? Search,
    BranchId? BranchId,
    StudentStatus? Status,
    EnrollmentStatus? EnrollmentStatus,
    StudentSortField SortBy,
    SortDirection SortDirection
) : DriveOS.Application.Abstractions.Messaging.IQuery<PagedResult<StudentListItem>>;

public interface IStudentReadService
{
    Task<PagedResult<StudentListItem>> GetPageAsync(
        GetStudentsQuery query,
        CancellationToken cancellationToken = default
    );
}
