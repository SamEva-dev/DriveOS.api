using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.Modules.Students.Application.References;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.CurriculumPedagogy;

public sealed class TrainingPathStudentGateway(IStudentReferenceReadService students) : ITrainingPathStudentGateway
{
    public Task<bool> ExistsAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default) =>
        students.ExistsAsync(organizationId, studentId, cancellationToken);
}
