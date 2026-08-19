using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Api.Integrations.CurriculumPedagogy;

public sealed class TrainingPathStudentGateway(StudentsDbContext students) : ITrainingPathStudentGateway
{
    public Task<bool> ExistsAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default) =>
        students.Students.AsNoTracking().AnyAsync(x => x.OrganizationId == organizationId && x.Id == studentId, cancellationToken);
}
