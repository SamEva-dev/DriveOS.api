using System.Security.Cryptography;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Invitations;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Invitations;

public sealed class CreateFreelanceInvitationCommandHandler(
    IFreelanceInvitationRepository invitations,
    IProfessionalMissionRepository missions,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<CreateFreelanceInvitationCommand,FreelanceInvitationId>
{
    public async Task<Result<FreelanceInvitationId>> Handle(CreateFreelanceInvitationCommand c,CancellationToken ct)
    {
        if(c.MissionId is ProfessionalMissionId missionId)
        {
            ProfessionalMission? mission=await missions.GetAsync(missionId,false,ct);
            if(mission is null||mission.OrganizationId!=c.OrganizationId)
                return Result.Failure<FreelanceInvitationId>(ProfessionalMissionErrors.NotFound);
        }

        if(await invitations.ExistsPendingAsync(c.OrganizationId,c.Email,c.Phone,c.MissionId,ct))
            return Result.Failure<FreelanceInvitationId>(Error.Conflict(
                "ProfessionalMarketplace.Invitations.DuplicatePending","errors.professionalMarketplace.invitations.duplicatePending"));

        var created=FreelanceInvitation.Create(c.Id,c.OrganizationId,c.BranchId,c.MissionId,c.ProfessionalProfileId,
            c.InvitedUserId,c.Email,c.Phone,c.Message,c.ExpirationDate,DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),
            c.ActorUserId,clock.UtcNow);
        if(created.IsFailure)return Result.Failure<FreelanceInvitationId>(created.Error);

        invitations.Add(created.Value);
        await uow.CommitAsync(ct);
        return Result.Success(created.Value.Id);
    }
}

public sealed class SendFreelanceInvitationCommandHandler(
    IFreelanceInvitationRepository invitations,
    IFreelanceInvitationDeliveryGateway delivery,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<SendFreelanceInvitationCommand,FreelanceInvitationSendResponse>
{
    public async Task<Result<FreelanceInvitationSendResponse>> Handle(SendFreelanceInvitationCommand c,CancellationToken ct)
    {
        FreelanceInvitation? invitation=await invitations.GetAsync(c.Id,true,ct);
        if(invitation is null||invitation.ClientOrganizationId!=c.OrganizationId)
            return Result.Failure<FreelanceInvitationSendResponse>(FreelanceInvitationErrors.NotFound);

        string token=Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace('+','-').Replace('/','_').TrimEnd('=');
        var sent=invitation.Send(token,clock.UtcNow,c.ActorUserId);
        if(sent.IsFailure)return Result.Failure<FreelanceInvitationSendResponse>(sent.Error);

        await uow.CommitAsync(ct);

        string baseUrl=(c.PublicBaseUrl??string.Empty).TrimEnd('/');
        string secureUrl=$"{baseUrl}/invite/freelance?token={Uri.EscapeDataString(token)}";
        await delivery.TrySendAsync(new(invitation.Email,invitation.Phone,secureUrl,invitation.Message,
            invitation.ExpirationDate,invitation.Id.Value,invitation.ClientOrganizationId.Value),ct);

        return Result.Success(new FreelanceInvitationSendResponse(invitation.Id.Value,secureUrl,invitation.ExpirationDate));
    }
}

public sealed class OpenFreelanceInvitationCommandHandler(
    IFreelanceInvitationRepository invitations,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<OpenFreelanceInvitationCommand,PublicFreelanceInvitationResponse>
{
    public async Task<Result<PublicFreelanceInvitationResponse>> Handle(OpenFreelanceInvitationCommand c,CancellationToken ct)
    {
        string hash=FreelanceInvitation.HashToken(c.Token);
        FreelanceInvitation? invitation=await invitations.GetByTokenHashAsync(hash,true,ct);
        if(invitation is null||!invitation.TokenMatches(c.Token))
            return Result.Failure<PublicFreelanceInvitationResponse>(FreelanceInvitationErrors.NotFound);

        var opened=invitation.MarkOpened(DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),clock.UtcNow);
        if(opened.IsFailure&&opened.Error!=FreelanceInvitationErrors.Expired)
            return Result.Failure<PublicFreelanceInvitationResponse>(opened.Error);
        await uow.CommitAsync(ct);

        return Result.Success(new PublicFreelanceInvitationResponse(invitation.Id.Value,
            invitation.ClientOrganizationId.Value,invitation.BranchId?.Value,invitation.MissionId?.Value,
            invitation.Message,invitation.ExpirationDate,invitation.Status.ToString(),true));
    }
}

public sealed class AcceptFreelanceInvitationCommandHandler(
    IFreelanceInvitationRepository invitations,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<AcceptFreelanceInvitationCommand,FreelanceInvitationAcceptanceResponse>
{
    public async Task<Result<FreelanceInvitationAcceptanceResponse>> Handle(AcceptFreelanceInvitationCommand c,CancellationToken ct)
    {
        string hash=FreelanceInvitation.HashToken(c.Token);
        FreelanceInvitation? invitation=await invitations.GetByTokenHashAsync(hash,true,ct);
        if(invitation is null)return Result.Failure<FreelanceInvitationAcceptanceResponse>(FreelanceInvitationErrors.NotFound);

        var accepted=invitation.Accept(c.Token,c.AuthenticatedUserId,DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),clock.UtcNow);
        if(accepted.IsFailure)return Result.Failure<FreelanceInvitationAcceptanceResponse>(accepted.Error);
        await uow.CommitAsync(ct);

        return Result.Success(new FreelanceInvitationAcceptanceResponse(invitation.Id.Value,c.AuthenticatedUserId.Value,
            invitation.ProfessionalProfileId?.Value,invitation.ProfessionalProfileId is null));
    }
}

public sealed class DeclineFreelanceInvitationCommandHandler(
    IFreelanceInvitationRepository invitations,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<DeclineFreelanceInvitationCommand>
{
    public async Task<Result> Handle(DeclineFreelanceInvitationCommand c,CancellationToken ct)
    {
        string hash=FreelanceInvitation.HashToken(c.Token);
        FreelanceInvitation? invitation=await invitations.GetByTokenHashAsync(hash,true,ct);
        if(invitation is null)return Result.Failure(FreelanceInvitationErrors.NotFound);
        var declined=invitation.Decline(c.Token,c.Reason,DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),clock.UtcNow);
        if(declined.IsFailure)return declined;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class CancelFreelanceInvitationCommandHandler(
    IFreelanceInvitationRepository invitations,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<CancelFreelanceInvitationCommand>
{
    public async Task<Result> Handle(CancelFreelanceInvitationCommand c,CancellationToken ct)
    {
        FreelanceInvitation? invitation=await invitations.GetAsync(c.Id,true,ct);
        if(invitation is null||invitation.ClientOrganizationId!=c.OrganizationId)
            return Result.Failure(FreelanceInvitationErrors.NotFound);
        var cancelled=invitation.Cancel(clock.UtcNow,c.ActorUserId);
        if(cancelled.IsFailure)return cancelled;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
