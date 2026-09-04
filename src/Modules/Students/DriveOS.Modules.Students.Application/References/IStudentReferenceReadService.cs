using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Students.Application.References;

public sealed record StudentContractSourceReference(
    DraftEnrollmentId EnrollmentId,
    PersonId StudentId,
    BranchId BranchId,
    Guid? SourceLeadId,
    string TrainingCode,
    string StudentDisplayName);

public interface IStudentReferenceReadService
{
    Task<bool> ExistsAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Guid>> FindExistingActiveIdsAsync(OrganizationId organizationId, IReadOnlyCollection<Guid> studentIds, CancellationToken cancellationToken = default);
    Task<StudentContractSourceReference?> GetContractSourceAsync(OrganizationId organizationId, DraftEnrollmentId enrollmentId, CancellationToken cancellationToken = default);
}
