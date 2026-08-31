using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Disputes;

/// <summary>
/// Formal dispute dossier for one ServiceEntry. Evidence and discussion are preserved as immutable history.
/// The disputed ServiceEntry remains blocked until an explicit resolution approves or rejects it.
/// </summary>
public sealed class ServiceDispute:AggregateRoot<ServiceDisputeId>,IAuditableEntity
{
    private ServiceDispute(){}

    private ServiceDispute(
        ServiceDisputeId id,
        ServiceEntryId serviceEntryId,
        ProfessionalEngagementId engagementId,
        ProfessionalProfileId professionalProfileId,
        OrganizationId clientOrganizationId,
        Guid raisedByOrganizationId,
        ServiceDisputeReason reason,
        string description,
        ServiceDisputeEvidence[] evidence):base(id)
    {
        ServiceEntryId=serviceEntryId;
        EngagementId=engagementId;
        ProfessionalProfileId=professionalProfileId;
        ClientOrganizationId=clientOrganizationId;
        RaisedByOrganizationId=raisedByOrganizationId;
        Reason=reason;
        Description=description.Trim();
        Evidence=evidence;
        Status=ServiceDisputeStatus.Open;
    }

    public ServiceEntryId ServiceEntryId{get;private set;}
    public ProfessionalEngagementId EngagementId{get;private set;}
    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public OrganizationId ClientOrganizationId{get;private set;}
    public Guid RaisedByOrganizationId{get;private set;}
    public ServiceDisputeReason Reason{get;private set;}
    public string Description{get;private set;}=string.Empty;
    public ServiceDisputeEvidence[] Evidence{get;private set;}=[];
    public ServiceDisputeMessage[] Discussion{get;private set;}=[];
    public ServiceDisputeStatus Status{get;private set;}
    public ServiceDisputeResolutionOutcome? ResolutionOutcome{get;private set;}
    public string? Resolution{get;private set;}
    public DateTimeOffset? ResolvedAtUtc{get;private set;}
    public UserId? ResolvedByUserId{get;private set;}
    public DateTimeOffset? EscalatedAtUtc{get;private set;}
    public UserId? EscalatedByUserId{get;private set;}
    public string? EscalationReason{get;private set;}

    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<ServiceDispute> Open(
        ServiceDisputeId id,
        ServiceEntryId serviceEntryId,
        ProfessionalEngagementId engagementId,
        ProfessionalProfileId professionalProfileId,
        OrganizationId clientOrganizationId,
        Guid raisedByOrganizationId,
        ServiceDisputeReason reason,
        string description,
        IEnumerable<ServiceDisputeEvidence>? evidence,
        DateTimeOffset now,
        UserId actor)
    {
        if(id.IsEmpty||serviceEntryId.IsEmpty||engagementId.IsEmpty||professionalProfileId.IsEmpty||
           clientOrganizationId.IsEmpty||raisedByOrganizationId==Guid.Empty)
            return Result.Failure<ServiceDispute>(ServiceDisputeErrors.InvalidIdentifier);

        description=(description??string.Empty).Trim();
        if(description.Length is <2 or >3000)
            return Result.Failure<ServiceDispute>(ServiceDisputeErrors.InvalidDescription);

        ServiceDisputeEvidence[] items=(evidence??[]).ToArray();
        if(items.Length>20||items.Any(x=>x.DocumentReferenceId==Guid.Empty||string.IsNullOrWhiteSpace(x.Label)))
            return Result.Failure<ServiceDispute>(ServiceDisputeErrors.InvalidEvidence);

        var dispute=new ServiceDispute(id,serviceEntryId,engagementId,professionalProfileId,
            clientOrganizationId,raisedByOrganizationId,reason,description,items);
        dispute.SetCreatedAudit(now,actor);
        return Result.Success(dispute);
    }

    public Result AddEvidence(ServiceDisputeEvidence evidence,DateTimeOffset now,UserId actor)
    {
        if(IsClosed)return Result.Failure(ServiceDisputeErrors.Closed);
        if(evidence.DocumentReferenceId==Guid.Empty||string.IsNullOrWhiteSpace(evidence.Label)||Evidence.Length>=50)
            return Result.Failure(ServiceDisputeErrors.InvalidEvidence);
        if(Evidence.Any(x=>x.DocumentReferenceId==evidence.DocumentReferenceId))return Result.Success();
        Evidence=[..Evidence,evidence];
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result AddMessage(ServiceDisputeParty party,string message,DateTimeOffset now,UserId actor)
    {
        if(IsClosed)return Result.Failure(ServiceDisputeErrors.Closed);
        message=(message??string.Empty).Trim();
        if(message.Length is <1 or >4000)return Result.Failure(ServiceDisputeErrors.InvalidMessage);
        Discussion=[..Discussion,new ServiceDisputeMessage(Guid.NewGuid(),party,message,now.ToUniversalTime(),actor)];
        if(Status==ServiceDisputeStatus.Open)Status=ServiceDisputeStatus.UnderDiscussion;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result WaitFor(ServiceDisputeParty party,DateTimeOffset now,UserId actor)
    {
        if(IsClosed)return Result.Failure(ServiceDisputeErrors.Closed);
        Status=party switch
        {
            ServiceDisputeParty.Freelance=>ServiceDisputeStatus.WaitingForFreelance,
            ServiceDisputeParty.School=>ServiceDisputeStatus.WaitingForSchool,
            _=>ServiceDisputeStatus.UnderDiscussion
        };
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result Resolve(ServiceDisputeResolutionOutcome outcome,string resolution,DateTimeOffset now,UserId actor)
    {
        if(IsClosed)return Result.Failure(ServiceDisputeErrors.Closed);
        resolution=(resolution??string.Empty).Trim();
        if(resolution.Length is <2 or >3000)return Result.Failure(ServiceDisputeErrors.ResolutionRequired);

        ResolutionOutcome=outcome;
        Resolution=resolution;
        ResolvedAtUtc=now.ToUniversalTime();
        ResolvedByUserId=actor;
        Status=outcome==ServiceDisputeResolutionOutcome.Rejected
            ?ServiceDisputeStatus.Rejected
            :ServiceDisputeStatus.Resolved;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result Escalate(string reason,DateTimeOffset now,UserId actor)
    {
        if(IsClosed)return Result.Failure(ServiceDisputeErrors.Closed);
        reason=(reason??string.Empty).Trim();
        if(reason.Length is <2 or >1000)return Result.Failure(ServiceDisputeErrors.EscalationReasonRequired);
        Status=ServiceDisputeStatus.Escalated;
        EscalationReason=reason;
        EscalatedAtUtc=now.ToUniversalTime();
        EscalatedByUserId=actor;
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public bool IsClosed=>Status is ServiceDisputeStatus.Resolved or ServiceDisputeStatus.Rejected;
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
}

public sealed record ServiceDisputeEvidence(Guid DocumentReferenceId,string Label,string? Note);
public sealed record ServiceDisputeMessage(Guid Id,ServiceDisputeParty Party,string Message,DateTimeOffset CreatedAtUtc,UserId CreatedByUserId);

public enum ServiceDisputeStatus{Open=1,UnderDiscussion=2,WaitingForFreelance=3,WaitingForSchool=4,Resolved=5,Rejected=6,Escalated=7}
public enum ServiceDisputeParty{School=1,Freelance=2,Mediator=3}
public enum ServiceDisputeResolutionOutcome{ApproveServiceEntry=1,RejectServiceEntry=2,Rejected=3}
public enum ServiceDisputeReason{Duration=1,Rate=2,Absence=3,Expenses=4,ServiceQuality=5,ServiceNotPerformed=6,Duplicate=7,IncorrectStudent=8,NonCompliantVehicle=9,Other=10}
