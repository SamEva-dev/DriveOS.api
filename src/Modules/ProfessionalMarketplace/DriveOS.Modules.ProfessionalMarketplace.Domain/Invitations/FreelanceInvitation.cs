using System.Security.Cryptography;
using System.Text;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Invitations;

/// <summary>
/// Secure invitation issued by a client organization to an external professional.
/// Only a SHA-256 hash of the bearer token is persisted. An email match never merges identities.
/// </summary>
public sealed class FreelanceInvitation:AggregateRoot<FreelanceInvitationId>,IAuditableEntity
{
    private FreelanceInvitation(){}

    private FreelanceInvitation(
        FreelanceInvitationId id,
        OrganizationId clientOrganizationId,
        BranchId? branchId,
        ProfessionalMissionId? missionId,
        ProfessionalProfileId? professionalProfileId,
        UserId? invitedUserId,
        string? email,
        string? phone,
        string? message,
        DateOnly expirationDate,
        UserId invitedByUserId):base(id)
    {
        ClientOrganizationId=clientOrganizationId;
        BranchId=branchId;
        MissionId=missionId;
        ProfessionalProfileId=professionalProfileId;
        InvitedUserId=invitedUserId;
        Email=NormalizeEmail(email);
        Phone=NormalizeOptional(phone,40);
        Message=NormalizeOptional(message,2000);
        ExpirationDate=expirationDate;
        InvitedByUserId=invitedByUserId;
        Status=FreelanceInvitationStatus.Draft;
    }

    public OrganizationId ClientOrganizationId{get;private set;}
    public BranchId? BranchId{get;private set;}
    public ProfessionalMissionId? MissionId{get;private set;}
    public ProfessionalProfileId? ProfessionalProfileId{get;private set;}
    public UserId? InvitedUserId{get;private set;}
    public string? Email{get;private set;}
    public string? Phone{get;private set;}
    public string? Message{get;private set;}
    public DateOnly ExpirationDate{get;private set;}
    public UserId InvitedByUserId{get;private set;}
    public FreelanceInvitationStatus Status{get;private set;}
    public string? TokenHash{get;private set;}
    public DateTimeOffset? SentAtUtc{get;private set;}
    public DateTimeOffset? DeliveredAtUtc{get;private set;}
    public DateTimeOffset? OpenedAtUtc{get;private set;}
    public DateTimeOffset? RespondedAtUtc{get;private set;}
    public UserId? AcceptedByUserId{get;private set;}
    public string? DeclineReason{get;private set;}

    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<FreelanceInvitation> Create(
        FreelanceInvitationId id,
        OrganizationId clientOrganizationId,
        BranchId? branchId,
        ProfessionalMissionId? missionId,
        ProfessionalProfileId? professionalProfileId,
        UserId? invitedUserId,
        string? email,
        string? phone,
        string? message,
        DateOnly expirationDate,
        DateOnly today,
        UserId invitedByUserId,
        DateTimeOffset now)
    {
        if(id.IsEmpty||clientOrganizationId.IsEmpty||invitedByUserId.IsEmpty)
            return Result.Failure<FreelanceInvitation>(FreelanceInvitationErrors.InvalidIdentifier);

        string? normalizedEmail=NormalizeEmail(email);
        string? normalizedPhone=NormalizeOptional(phone,40);
        if(invitedUserId is null&&professionalProfileId is null&&
           string.IsNullOrWhiteSpace(normalizedEmail)&&string.IsNullOrWhiteSpace(normalizedPhone))
            return Result.Failure<FreelanceInvitation>(FreelanceInvitationErrors.RecipientRequired);

        if(expirationDate<=today||expirationDate>today.AddDays(90))
            return Result.Failure<FreelanceInvitation>(FreelanceInvitationErrors.InvalidExpiration);

        var invitation=new FreelanceInvitation(id,clientOrganizationId,branchId,missionId,
            professionalProfileId,invitedUserId,normalizedEmail,normalizedPhone,message,expirationDate,invitedByUserId);
        invitation.SetCreatedAudit(now,invitedByUserId);
        return Result.Success(invitation);
    }

    public Result<string> Send(string rawToken,DateTimeOffset now,UserId actor)
    {
        if(Status!=FreelanceInvitationStatus.Draft)
            return Result.Failure<string>(FreelanceInvitationErrors.InvalidTransition);
        if(string.IsNullOrWhiteSpace(rawToken)||rawToken.Length<32)
            return Result.Failure<string>(FreelanceInvitationErrors.InvalidToken);

        TokenHash=HashToken(rawToken);
        Status=FreelanceInvitationStatus.Sent;
        SentAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);
        return Result.Success(rawToken);
    }

    public Result MarkDelivered(DateTimeOffset now)
    {
        if(Status is not FreelanceInvitationStatus.Sent and not FreelanceInvitationStatus.Opened)
            return Result.Failure(FreelanceInvitationErrors.InvalidTransition);
        if(Status==FreelanceInvitationStatus.Sent)Status=FreelanceInvitationStatus.Delivered;
        DeliveredAtUtc??=now.ToUniversalTime();
        SetModifiedAudit(now,null);
        return Result.Success();
    }

    public Result MarkOpened(DateOnly today,DateTimeOffset now)
    {
        if(ExpireIfNeeded(today,now))return Result.Failure(FreelanceInvitationErrors.Expired);
        if(Status is not FreelanceInvitationStatus.Sent and not FreelanceInvitationStatus.Delivered and not FreelanceInvitationStatus.Opened)
            return Result.Failure(FreelanceInvitationErrors.InvalidTransition);
        Status=FreelanceInvitationStatus.Opened;
        OpenedAtUtc??=now.ToUniversalTime();
        SetModifiedAudit(now,null);
        return Result.Success();
    }

    public Result Accept(string rawToken,UserId authenticatedUserId,DateOnly today,DateTimeOffset now)
    {
        if(authenticatedUserId.IsEmpty)return Result.Failure(FreelanceInvitationErrors.AuthenticationRequired);
        if(!TokenMatches(rawToken))return Result.Failure(FreelanceInvitationErrors.InvalidToken);
        if(ExpireIfNeeded(today,now))return Result.Failure(FreelanceInvitationErrors.Expired);
        if(Status is not FreelanceInvitationStatus.Sent and not FreelanceInvitationStatus.Delivered and not FreelanceInvitationStatus.Opened)
            return Result.Failure(FreelanceInvitationErrors.InvalidTransition);
        if(InvitedUserId is UserId expected&&!expected.IsEmpty&&expected!=authenticatedUserId)
            return Result.Failure(FreelanceInvitationErrors.InvitedUserMismatch);

        Status=FreelanceInvitationStatus.Accepted;
        AcceptedByUserId=authenticatedUserId;
        RespondedAtUtc=now.ToUniversalTime();
        TokenHash=null;
        SetModifiedAudit(now,authenticatedUserId);
        return Result.Success();
    }

    public Result Decline(string rawToken,string? reason,DateOnly today,DateTimeOffset now)
    {
        if(!TokenMatches(rawToken))return Result.Failure(FreelanceInvitationErrors.InvalidToken);
        if(ExpireIfNeeded(today,now))return Result.Failure(FreelanceInvitationErrors.Expired);
        if(Status is not FreelanceInvitationStatus.Sent and not FreelanceInvitationStatus.Delivered and not FreelanceInvitationStatus.Opened)
            return Result.Failure(FreelanceInvitationErrors.InvalidTransition);

        Status=FreelanceInvitationStatus.Declined;
        DeclineReason=NormalizeOptional(reason,512);
        RespondedAtUtc=now.ToUniversalTime();
        TokenHash=null;
        SetModifiedAudit(now,null);
        return Result.Success();
    }

    public Result Cancel(DateTimeOffset now,UserId actor)
    {
        if(Status is FreelanceInvitationStatus.Accepted or FreelanceInvitationStatus.Declined or FreelanceInvitationStatus.Expired or FreelanceInvitationStatus.Cancelled)
            return Result.Failure(FreelanceInvitationErrors.InvalidTransition);
        Status=FreelanceInvitationStatus.Cancelled;
        TokenHash=null;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public bool TokenMatches(string? rawToken)=>
        !string.IsNullOrWhiteSpace(TokenHash)&&!string.IsNullOrWhiteSpace(rawToken)&&
        CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(TokenHash),
            Convert.FromHexString(HashToken(rawToken)));

    public bool ExpireIfNeeded(DateOnly today,DateTimeOffset now)
    {
        if(Status is FreelanceInvitationStatus.Accepted or FreelanceInvitationStatus.Declined or FreelanceInvitationStatus.Cancelled or FreelanceInvitationStatus.Expired)
            return Status==FreelanceInvitationStatus.Expired;
        if(today<=ExpirationDate)return false;
        Status=FreelanceInvitationStatus.Expired;
        TokenHash=null;
        SetModifiedAudit(now,null);
        return true;
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}

    public static string HashToken(string token)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim())));
    private static string? NormalizeEmail(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim().ToLowerInvariant();
    private static string? NormalizeOptional(string? value,int max){if(string.IsNullOrWhiteSpace(value))return null;var s=value.Trim();return s.Length<=max?s:s[..max];}
}

public enum FreelanceInvitationStatus
{
    Draft=1,Sent=2,Delivered=3,Opened=4,Accepted=5,Declined=6,Expired=7,Cancelled=8
}
