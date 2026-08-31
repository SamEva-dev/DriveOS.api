using DriveOS.Modules.ProfessionalMarketplace.Domain.StudentAssignments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;

internal sealed class ProfessionalStudentAssignmentRepository(
    ProfessionalMarketplaceDbContext db):IProfessionalStudentAssignmentRepository
{
    public Task<ProfessionalStudentAssignment?> GetAsync(
        ProfessionalStudentAssignmentId id,bool tracking,CancellationToken ct=default)=>
        tracking
            ?db.ProfessionalStudentAssignments.SingleOrDefaultAsync(x=>x.Id==id,ct)
            :db.ProfessionalStudentAssignments.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);

    public Task<bool> ExistsActiveAsync(
        ProfessionalMissionId missionId,PersonId studentId,CancellationToken ct=default)=>
        db.ProfessionalStudentAssignments.AsNoTracking().AnyAsync(x=>
            x.MissionId==missionId&&x.StudentId==studentId&&
            x.Status==ProfessionalStudentAssignmentStatus.Active,ct);


    public async Task<IReadOnlyList<ProfessionalStudentAssignment>> ListActiveByEngagementAsync(
        ProfessionalEngagementId engagementId,CancellationToken ct=default)=>
        await db.ProfessionalStudentAssignments.AsNoTracking()
            .Where(x=>x.EngagementId==engagementId&&x.Status==ProfessionalStudentAssignmentStatus.Active)
            .OrderBy(x=>x.EndsOn)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProfessionalStudentAssignment>> ListByMissionAsync(
        ProfessionalMissionId missionId,CancellationToken ct=default)=>
        await db.ProfessionalStudentAssignments.AsNoTracking()
            .Where(x=>x.MissionId==missionId)
            .OrderByDescending(x=>x.Status==ProfessionalStudentAssignmentStatus.Active)
            .ThenBy(x=>x.EndsOn)
            .ToListAsync(ct);


    public async Task<IReadOnlyList<ProfessionalStudentAssignment>> ListActiveByProfileAsync(
        ProfessionalProfileId profileId,CancellationToken ct=default)=>
        await db.ProfessionalStudentAssignments.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId&&x.Status==ProfessionalStudentAssignmentStatus.Active)
            .OrderBy(x=>x.EndsOn)
            .ThenBy(x=>x.StartsOn)
            .ToListAsync(ct);

    public void Add(ProfessionalStudentAssignment assignment)=>db.ProfessionalStudentAssignments.Add(assignment);
}
