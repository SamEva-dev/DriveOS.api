using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.StudentAssignments;

public interface IProfessionalStudentAssignmentRepository
{
    Task<ProfessionalStudentAssignment?> GetAsync(
        ProfessionalStudentAssignmentId id,bool tracking,CancellationToken ct=default);
    Task<bool> ExistsActiveAsync(
        ProfessionalMissionId missionId,PersonId studentId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalStudentAssignment>> ListActiveByEngagementAsync(
        ProfessionalEngagementId engagementId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalStudentAssignment>> ListByMissionAsync(
        ProfessionalMissionId missionId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalStudentAssignment>> ListActiveByProfileAsync(
        ProfessionalProfileId profileId,CancellationToken ct=default);
    void Add(ProfessionalStudentAssignment assignment);
}
