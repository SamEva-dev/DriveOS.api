using DriveOS.Modules.ProfessionalMarketplace.Domain.Invitations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;
internal sealed class FreelanceInvitationRepository(ProfessionalMarketplaceDbContext db):IFreelanceInvitationRepository
{
    public Task<FreelanceInvitation?> GetAsync(FreelanceInvitationId id,bool tracking,CancellationToken ct=default)=>
        tracking?db.FreelanceInvitations.SingleOrDefaultAsync(x=>x.Id==id,ct):
        db.FreelanceInvitations.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);

    public Task<FreelanceInvitation?> GetByTokenHashAsync(string tokenHash,bool tracking,CancellationToken ct=default)=>
        tracking?db.FreelanceInvitations.SingleOrDefaultAsync(x=>x.TokenHash==tokenHash,ct):
        db.FreelanceInvitations.AsNoTracking().SingleOrDefaultAsync(x=>x.TokenHash==tokenHash,ct);

    public Task<bool> ExistsPendingAsync(OrganizationId org,string? email,string? phone,ProfessionalMissionId? missionId,CancellationToken ct=default)
    {
        email=string.IsNullOrWhiteSpace(email)?null:email.Trim().ToLowerInvariant();
        phone=string.IsNullOrWhiteSpace(phone)?null:phone.Trim();
        return db.FreelanceInvitations.AsNoTracking().AnyAsync(x=>x.ClientOrganizationId==org&&x.MissionId==missionId&&
            (email==null||x.Email==email)&&(phone==null||x.Phone==phone)&&
            (x.Status==FreelanceInvitationStatus.Draft||x.Status==FreelanceInvitationStatus.Sent||
             x.Status==FreelanceInvitationStatus.Delivered||x.Status==FreelanceInvitationStatus.Opened),ct);
    }
    public void Add(FreelanceInvitation invitation)=>db.FreelanceInvitations.Add(invitation);
}
