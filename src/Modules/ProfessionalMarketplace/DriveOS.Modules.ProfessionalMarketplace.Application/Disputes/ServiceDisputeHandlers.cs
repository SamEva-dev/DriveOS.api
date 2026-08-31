using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Notifications;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Disputes;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Disputes;

public sealed class OpenServiceDisputeCommandHandler(
    IServiceDisputeRepository disputes,
    IServiceEntryRepository entries,
    IProfessionalProfileRepository profiles,
    IMarketplaceNotificationGateway notifications,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<OpenServiceDisputeCommand,ServiceDisputeId>
{
    public async Task<Result<ServiceDisputeId>> Handle(OpenServiceDisputeCommand c,CancellationToken ct)
    {
        ServiceEntry? entry=await entries.GetAsync(c.ServiceEntryId,true,ct);
        if(entry is null||entry.OrganizationId!=c.ClientOrganizationId)
            return Result.Failure<ServiceDisputeId>(ServiceEntryErrors.NotFound);

        if(entry.Status!=ServiceEntryStatus.Submitted)
            return Result.Failure<ServiceDisputeId>(ServiceEntryErrors.InvalidTransition);

        if(await disputes.HasOpenDisputeAsync(entry.Id,ct))
            return Result.Failure<ServiceDisputeId>(ServiceDisputeErrors.DuplicateOpenDispute);

        Guid raisedByOrganizationId=c.RaisedByOrganizationId;
        if(c.RaisedByParty==ServiceDisputeParty.Freelance)
        {
            if(c.RaisedByProfessionalProfileId is not ProfessionalProfileId expectedProfile||
               expectedProfile!=entry.ProfessionalProfileId)
                return Result.Failure<ServiceDisputeId>(ServiceEntryErrors.NotFound);

            ProfessionalProfile? raisingProfile=await profiles.GetByIdAsync(expectedProfile,ct);
            if(raisingProfile is null)
                return Result.Failure<ServiceDisputeId>(ServiceEntryErrors.NotFound);
            raisedByOrganizationId=raisingProfile.ProviderOrganizationId.Value;
        }

        var opened=ServiceDispute.Open(c.Id,entry.Id,entry.EngagementId,entry.ProfessionalProfileId,
            entry.OrganizationId,raisedByOrganizationId,c.Reason,c.Description,
            c.Evidence.Select(x=>new ServiceDisputeEvidence(x.DocumentReferenceId,x.Label,x.Note)),
            clock.UtcNow,c.ActorUserId);
        if(opened.IsFailure)return Result.Failure<ServiceDisputeId>(opened.Error);

        Result blocked=entry.OpenDispute(c.Description,clock.UtcNow,c.ActorUserId);
        if(blocked.IsFailure)return Result.Failure<ServiceDisputeId>(blocked.Error);

        disputes.Add(opened.Value);
        await uow.CommitAsync(ct);

        ProfessionalProfile? profile=await profiles.GetByIdAsync(entry.ProfessionalProfileId,ct);
        if(profile is not null&&profile.UserId is UserId professionalUserId&&!professionalUserId.IsEmpty&&c.RaisedByParty==ServiceDisputeParty.School)
        {
            await notifications.TryEnqueueAsync(new(
                "User",professionalUserId.Value,entry.OrganizationId,"DISPUTE",
                "professionalMarketplace.notifications.disputeOpened",
                $"service-dispute-opened:{opened.Value.Id.Value}",
                new Dictionary<string,string?>
                {
                    ["disputeId"]=opened.Value.Id.Value.ToString(),
                    ["serviceEntryId"]=entry.Id.Value.ToString(),
                    ["reason"]=c.Reason.ToString()
                },
                "SERVICE_DISPUTE",opened.Value.Id.Value,
                profile.ProfessionalEmail,
                profile.Languages.FirstOrDefault(x=>x.StartsWith("fr",StringComparison.OrdinalIgnoreCase))??"en",
                c.ActorUserId),ct);
        }
        else if(c.RaisedByParty==ServiceDisputeParty.Freelance)
        {
            await notifications.TryEnqueueAsync(new(
                "Organization",entry.OrganizationId.Value,entry.OrganizationId,"DISPUTE",
                "professionalMarketplace.notifications.disputeOpenedForSchool",
                $"service-dispute-school:{opened.Value.Id.Value}",
                new Dictionary<string,string?>
                {
                    ["disputeId"]=opened.Value.Id.Value.ToString(),
                    ["serviceEntryId"]=entry.Id.Value.ToString(),
                    ["reason"]=c.Reason.ToString()
                },
                "SERVICE_DISPUTE",opened.Value.Id.Value,null,null,c.ActorUserId),ct);
        }

        return Result.Success(opened.Value.Id);
    }
}

public abstract class ServiceDisputeMutation
{
    protected static async Task<Result> Run(
        ServiceDisputeId id,OrganizationId organizationId,Func<ServiceDispute,Result> action,
        IServiceDisputeRepository disputes,IProfessionalMarketplaceUnitOfWork uow,CancellationToken ct)
    {
        ServiceDispute? dispute=await disputes.GetAsync(id,true,ct);
        if(dispute is null||dispute.ClientOrganizationId!=organizationId)
            return Result.Failure(ServiceDisputeErrors.NotFound);
        Result result=action(dispute);if(result.IsFailure)return result;
        await uow.CommitAsync(ct);return Result.Success();
    }
}

public sealed class AddServiceDisputeMessageCommandHandler(
    IServiceDisputeRepository disputes,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ServiceDisputeMutation,ICommandHandler<AddServiceDisputeMessageCommand>
{
    public Task<Result> Handle(AddServiceDisputeMessageCommand c,CancellationToken ct)=>
        Run(c.Id,c.ClientOrganizationId,x=>x.AddMessage(c.Party,c.Message,clock.UtcNow,c.ActorUserId),disputes,uow,ct);
}

public sealed class AddServiceDisputeEvidenceCommandHandler(
    IServiceDisputeRepository disputes,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ServiceDisputeMutation,ICommandHandler<AddServiceDisputeEvidenceCommand>
{
    public Task<Result> Handle(AddServiceDisputeEvidenceCommand c,CancellationToken ct)=>
        Run(c.Id,c.ClientOrganizationId,x=>x.AddEvidence(
            new ServiceDisputeEvidence(c.Evidence.DocumentReferenceId,c.Evidence.Label,c.Evidence.Note),
            clock.UtcNow,c.ActorUserId),disputes,uow,ct);
}

public sealed class WaitServiceDisputeForCommandHandler(
    IServiceDisputeRepository disputes,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ServiceDisputeMutation,ICommandHandler<WaitServiceDisputeForCommand>
{
    public Task<Result> Handle(WaitServiceDisputeForCommand c,CancellationToken ct)=>
        Run(c.Id,c.ClientOrganizationId,x=>x.WaitFor(c.WaitingFor,clock.UtcNow,c.ActorUserId),disputes,uow,ct);
}

public sealed class EscalateServiceDisputeCommandHandler(
    IServiceDisputeRepository disputes,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ServiceDisputeMutation,ICommandHandler<EscalateServiceDisputeCommand>
{
    public Task<Result> Handle(EscalateServiceDisputeCommand c,CancellationToken ct)=>
        Run(c.Id,c.ClientOrganizationId,x=>x.Escalate(c.Reason,clock.UtcNow,c.ActorUserId),disputes,uow,ct);
}

public sealed class ResolveServiceDisputeCommandHandler(
    IServiceDisputeRepository disputes,
    IServiceEntryRepository entries,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<ResolveServiceDisputeCommand>
{
    public async Task<Result> Handle(ResolveServiceDisputeCommand c,CancellationToken ct)
    {
        ServiceDispute? dispute=await disputes.GetAsync(c.Id,true,ct);
        if(dispute is null||dispute.ClientOrganizationId!=c.ClientOrganizationId)
            return Result.Failure(ServiceDisputeErrors.NotFound);

        ServiceEntry? entry=await entries.GetAsync(dispute.ServiceEntryId,true,ct);
        if(entry is null)return Result.Failure(ServiceEntryErrors.NotFound);

        Result resolution=dispute.Resolve(c.Outcome,c.Resolution,clock.UtcNow,c.ActorUserId);
        if(resolution.IsFailure)return resolution;

        Result entryResult=c.Outcome switch
        {
            ServiceDisputeResolutionOutcome.ApproveServiceEntry=>entry.Approve(clock.UtcNow,c.ActorUserId),
            ServiceDisputeResolutionOutcome.RejectServiceEntry=>entry.ResolveDisputeRejected(c.Resolution,clock.UtcNow,c.ActorUserId),
            ServiceDisputeResolutionOutcome.Rejected=>entry.Approve(clock.UtcNow,c.ActorUserId),
            _=>Result.Failure(ServiceEntryErrors.InvalidTransition)
        };
        if(entryResult.IsFailure)return entryResult;

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class GetServiceDisputeQueryHandler(IServiceDisputeRepository disputes)
    :IQueryHandler<GetServiceDisputeQuery,ServiceDisputeResponse>
{
    public async Task<Result<ServiceDisputeResponse>> Handle(GetServiceDisputeQuery q,CancellationToken ct)
    {
        ServiceDispute? x=await disputes.GetAsync(q.Id,false,ct);
        if(x is null)return Result.Failure<ServiceDisputeResponse>(ServiceDisputeErrors.NotFound);
        if(q.OrganizationId is OrganizationId org&&x.ClientOrganizationId!=org)
            return Result.Failure<ServiceDisputeResponse>(ServiceDisputeErrors.NotFound);
        if(q.ProfessionalProfileId is ProfessionalProfileId profile&&x.ProfessionalProfileId!=profile)
            return Result.Failure<ServiceDisputeResponse>(ServiceDisputeErrors.NotFound);

        return Result.Success(new ServiceDisputeResponse(
            x.Id.Value,x.ServiceEntryId.Value,x.EngagementId.Value,x.ProfessionalProfileId.Value,
            x.ClientOrganizationId.Value,x.RaisedByOrganizationId,x.Reason.ToString(),x.Description,
            x.Status.ToString(),x.ResolutionOutcome?.ToString(),x.Resolution,x.Evidence,x.Discussion,
            x.CreatedAtUtc,x.ResolvedAtUtc,x.EscalatedAtUtc,x.EscalatedByUserId?.Value,x.EscalationReason));
    }
}


public sealed class ListOrganizationServiceDisputesQueryHandler(IServiceDisputeRepository disputes)
    :IQueryHandler<ListOrganizationServiceDisputesQuery,IReadOnlyList<ServiceDisputeResponse>>
{
    public async Task<Result<IReadOnlyList<ServiceDisputeResponse>>> Handle(ListOrganizationServiceDisputesQuery q,CancellationToken ct)
    {
        var items=await disputes.ListByOrganizationAsync(q.OrganizationId,ct);
        return Result.Success<IReadOnlyList<ServiceDisputeResponse>>(items.Select(Map).ToArray());
    }

    internal static ServiceDisputeResponse Map(ServiceDispute x)=>new(
        x.Id.Value,x.ServiceEntryId.Value,x.EngagementId.Value,x.ProfessionalProfileId.Value,
        x.ClientOrganizationId.Value,x.RaisedByOrganizationId,x.Reason.ToString(),x.Description,
        x.Status.ToString(),x.ResolutionOutcome?.ToString(),x.Resolution,x.Evidence,x.Discussion,
        x.CreatedAtUtc,x.ResolvedAtUtc,x.EscalatedAtUtc,x.EscalatedByUserId?.Value,x.EscalationReason);
}

public sealed class ListProfessionalServiceDisputesQueryHandler(IServiceDisputeRepository disputes)
    :IQueryHandler<ListProfessionalServiceDisputesQuery,IReadOnlyList<ServiceDisputeResponse>>
{
    public async Task<Result<IReadOnlyList<ServiceDisputeResponse>>> Handle(ListProfessionalServiceDisputesQuery q,CancellationToken ct)
    {
        var items=await disputes.ListByProfessionalAsync(q.ProfessionalProfileId,ct);
        return Result.Success<IReadOnlyList<ServiceDisputeResponse>>(
            items.Select(ListOrganizationServiceDisputesQueryHandler.Map).ToArray());
    }
}
