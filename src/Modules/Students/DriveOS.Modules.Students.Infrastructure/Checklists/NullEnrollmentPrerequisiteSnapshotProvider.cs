using DriveOS.Modules.Students.Application.Checklists;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Students.Infrastructure.Checklists;

internal sealed class NullEnrollmentPrerequisiteSnapshotProvider : IEnrollmentPrerequisiteSnapshotProvider
{
    public Task<EnrollmentPrerequisiteSnapshot> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        DraftEnrollmentId enrollmentId,
        LeadId? sourceLeadId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new EnrollmentPrerequisiteSnapshot(null, null, null, null, null));
}
