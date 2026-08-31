using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Invitations;

public sealed record CreateFreelanceInvitationCommand(
    FreelanceInvitationId Id,OrganizationId OrganizationId,BranchId? BranchId,ProfessionalMissionId? MissionId,
    ProfessionalProfileId? ProfessionalProfileId,UserId? InvitedUserId,string? Email,string? Phone,string? Message,
    DateOnly ExpirationDate,UserId ActorUserId):ICommand<FreelanceInvitationId>;

public sealed record SendFreelanceInvitationCommand(
    FreelanceInvitationId Id,OrganizationId OrganizationId,string PublicBaseUrl,UserId ActorUserId)
    :ICommand<FreelanceInvitationSendResponse>;

public sealed record FreelanceInvitationSendResponse(Guid InvitationId,string SecureUrl,DateOnly ExpirationDate);

public sealed record OpenFreelanceInvitationCommand(string Token):ICommand<PublicFreelanceInvitationResponse>;
public sealed record AcceptFreelanceInvitationCommand(string Token,UserId AuthenticatedUserId):ICommand<FreelanceInvitationAcceptanceResponse>;
public sealed record DeclineFreelanceInvitationCommand(string Token,string? Reason):ICommand;
public sealed record CancelFreelanceInvitationCommand(FreelanceInvitationId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;

public sealed record PublicFreelanceInvitationResponse(
    Guid InvitationId,Guid OrganizationId,Guid? BranchId,Guid? MissionId,string? Message,DateOnly ExpirationDate,string Status,
    bool AuthenticationRequired);

public sealed record FreelanceInvitationAcceptanceResponse(
    Guid InvitationId,Guid AcceptedByUserId,Guid? ProfessionalProfileId,bool ProfessionalProfileRequired);

public sealed record FreelanceInvitationDeliveryRequest(
    string? Email,string? Phone,string SecureUrl,string? Message,DateOnly ExpirationDate,Guid InvitationId,Guid OrganizationId);

public interface IFreelanceInvitationDeliveryGateway
{
    Task TrySendAsync(FreelanceInvitationDeliveryRequest request,CancellationToken ct=default);
}
