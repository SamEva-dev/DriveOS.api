using DriveOS.Modules.Students.Application.References;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Queries;

internal sealed class StudentReferenceReadService(StudentsDbContext database) : IStudentReferenceReadService
{
    public Task<bool> ExistsAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default) =>
        database.Students.AsNoTracking().AnyAsync(x => x.OrganizationId == organizationId && x.Id == studentId && x.Status != StudentStatus.Archived, cancellationToken);

    public async Task<IReadOnlyCollection<Guid>> FindExistingActiveIdsAsync(OrganizationId organizationId, IReadOnlyCollection<Guid> studentIds, CancellationToken cancellationToken = default)
    {
        if (studentIds.Count == 0) return [];
        PersonId[] typed = studentIds.Select(x => new PersonId(x)).ToArray();
        return await database.Students.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.Status != StudentStatus.Archived && typed.Contains(x.Id))
            .Select(x => x.Id.Value)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<StudentContractSourceReference?> GetContractSourceAsync(OrganizationId organizationId, DraftEnrollmentId enrollmentId, CancellationToken cancellationToken = default)
    {
        var row = await database.Enrollments.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.Id == enrollmentId)
            .Join(database.Students.AsNoTracking(), enrollment => enrollment.StudentId, student => student.Id, (enrollment, student) => new
            {
                enrollment.Id,
                enrollment.StudentId,
                enrollment.BranchId,
                enrollment.SourceLeadId,
                enrollment.TrainingCode,
                student.FirstName,
                student.LastName
            })
            .SingleOrDefaultAsync(cancellationToken);
        return row is null ? null : new StudentContractSourceReference(row.Id, row.StudentId, row.BranchId, row.SourceLeadId?.Value, row.TrainingCode, $"{row.FirstName} {row.LastName}".Trim());
    }
}
