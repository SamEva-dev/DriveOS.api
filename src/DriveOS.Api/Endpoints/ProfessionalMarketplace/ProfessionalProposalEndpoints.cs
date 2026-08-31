using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.Proposals;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalProposalEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalProposalEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Proposals");
        g.MapGet("/organizations/{organizationId:guid}/profiles/{profileId:guid}/proposals",List).RequireAuthorization("ProfessionalMarketplace.Proposals.Read");
        g.MapPost("/organizations/{organizationId:guid}/profiles/{profileId:guid}/proposals",Create).RequireAuthorization("ProfessionalMarketplace.Proposals.Create");
        g.MapPost("/profiles/{profileId:guid}/proposals/{proposalId:guid}/accept",Accept).RequireAuthorization("ProfessionalMarketplace.Proposals.Accept");
        g.MapPost("/profiles/{profileId:guid}/proposals/{proposalId:guid}/reject",Reject).RequireAuthorization("ProfessionalMarketplace.Proposals.Reject");
        g.MapPost("/profiles/{profileId:guid}/proposals/{proposalId:guid}/counter",Counter).RequireAuthorization("ProfessionalMarketplace.Proposals.Counter");
        g.MapPost("/organizations/{organizationId:guid}/proposals/{proposalId:guid}/withdraw",Withdraw).RequireAuthorization("ProfessionalMarketplace.Proposals.Withdraw");
        return app;
    }


    private static async Task<IResult> List(Guid organizationId,Guid profileId,Guid? opportunityId,IMediator m,CancellationToken ct)
    {
        ProfessionalOpportunityId opportunity;
        if (opportunityId is Guid o)
        {
            opportunity = new ProfessionalOpportunityId(o);
        }
        else
        {
            opportunity = new ProfessionalOpportunityId(Guid.Empty);
        }

        var r=await m.Send(new ListProfessionalProposalsQuery(new OrganizationId(organizationId),new ProfessionalProfileId(profileId),opportunity),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Create(Guid organizationId,Guid profileId,CreateProposalRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new ProfessionalProposalId(Guid.NewGuid());
        BranchId? branch=q.BranchId is Guid b?new BranchId(b):null;
        ProfessionalOpportunityId? opportunity=q.OpportunityId is Guid o?new ProfessionalOpportunityId(o):null;
        var r=await m.Send(new CreateProfessionalProposalCommand(
            id,new OrganizationId(organizationId),branch,new ProfessionalProfileId(profileId),opportunity,
            q.Subject,q.Message,q.StartsOn,q.EndsOn,q.TeachingCategoryCodes??[],q.EngagementType,
            q.VehicleProvisionMode,q.ProposedRate,q.Currency,q.RateUnit,q.Negotiable,q.ExpiresAtUtc,actor),ct);
        return r.IsSuccess?Results.Created($"/api/professional-marketplace/proposals/{id.Value}",new{id=id.Value}):Problem(r.Error);
    }

    private static async Task<IResult> Accept(Guid profileId,Guid proposalId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new AcceptProfessionalProposalCommand(new(proposalId),new(profileId),actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> Reject(Guid profileId,Guid proposalId,RejectProposalRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new RejectProfessionalProposalCommand(new(proposalId),new(profileId),q.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> Counter(Guid profileId,Guid proposalId,CounterProposalRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new CounterProfessionalProposalCommand(new(proposalId),new(profileId),q.ProposedRate,q.Currency,q.RateUnit,q.Negotiable,q.Message,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> Withdraw(Guid organizationId,Guid proposalId,WithdrawProposalRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new WithdrawProfessionalProposalCommand(new(proposalId),new(organizationId),q.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.MessageKey}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.MessageKey}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.MessageKey})
    };
}

internal sealed record CreateProposalRequest(
    Guid? BranchId,Guid? OpportunityId,string Subject,string Message,DateOnly StartsOn,DateOnly EndsOn,
    string[]? TeachingCategoryCodes,ProfessionalEngagementType EngagementType,ProfessionalVehicleProvisionMode VehicleProvisionMode,
    decimal? ProposedRate,string? Currency,ProfessionalRateUnit? RateUnit,bool Negotiable,DateTimeOffset ExpiresAtUtc);
internal sealed record RejectProposalRequest(string? Reason);
internal sealed record CounterProposalRequest(decimal ProposedRate,string Currency,ProfessionalRateUnit RateUnit,bool Negotiable,string? Message);
internal sealed record WithdrawProposalRequest(string? Reason);
