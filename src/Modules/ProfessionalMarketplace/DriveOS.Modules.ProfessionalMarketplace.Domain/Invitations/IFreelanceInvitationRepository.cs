using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Invitations;
public interface IFreelanceInvitationRepository
{
    Task<FreelanceInvitation?> GetAsync(FreelanceInvitationId id,bool tracking,CancellationToken ct=default);
    Task<FreelanceInvitation?> GetByTokenHashAsync(string tokenHash,bool tracking,CancellationToken ct=default);
    Task<bool> ExistsPendingAsync(OrganizationId organizationId,string? email,string? phone,ProfessionalMissionId? missionId,CancellationToken ct=default);
    void Add(FreelanceInvitation invitation);
}
