using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalServiceContractEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalServiceContractEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace/organizations/{organizationId:guid}/engagements/{engagementId:guid}")
            .WithTags("Professional Marketplace - Contracts");

        g.MapGet("/contract",Get)
            .RequireAuthorization("ProfessionalMarketplace.Contracts.Read");
        g.MapPost("/contract",Create)
            .RequireAuthorization("ProfessionalMarketplace.Contracts.Manage");
        g.MapPost("/contract/generate",Generate)
            .RequireAuthorization("ProfessionalMarketplace.Contracts.Generate");
        g.MapPost("/contract/revise",Revise)
            .RequireAuthorization("ProfessionalMarketplace.Contracts.Generate");
        g.MapPost("/contract/send-for-signature",Send)
            .RequireAuthorization("ProfessionalMarketplace.Contracts.SendForSignature");
        g.MapPost("/contract/signatures",RecordSignature)
            .RequireAuthorization("ProfessionalMarketplace.Contracts.Manage");
        g.MapPost("/contract/terminate",Terminate)
            .RequireAuthorization("ProfessionalMarketplace.Contracts.Terminate");

        return app;
    }


    private static async Task<IResult> Get(
        Guid organizationId,Guid engagementId,IMediator mediator,CancellationToken ct)
    {
        Result<ProfessionalEngagementView> engagement=await mediator.Send(
            new GetProfessionalEngagementQuery(new(organizationId),new(engagementId)),ct);
        if(engagement.IsFailure)return Problem(engagement.Error);
        return engagement.Value.Contract is null?Results.NotFound():Results.Ok(engagement.Value.Contract);
    }

    private static async Task<IResult> Create(
        Guid organizationId,Guid engagementId,CreateProfessionalServiceContractRequest q,
        IMediator mediator,ICurrentUser user,CancellationToken ct)
    {
        if(user.UserId is not{} actor)return Results.Unauthorized();
        var id=new ProfessionalServiceContractId(Guid.NewGuid());
        var signatories=(q.Signatories??[])
            .Select(x=>new ProfessionalContractSignatoryInput(
                new PersonId(x.PersonId),x.Role,x.SigningOrder,x.IsRequired))
            .ToArray();

        var r=await mediator.Send(new CreateProfessionalServiceContractCommand(
            id,new(organizationId),new(engagementId),q.ContractNumber,q.ContractType,q.SignatureOrder,signatories,actor),ct);

        return r.IsSuccess
            ?Results.Created($"/api/professional-marketplace/organizations/{organizationId}/engagements/{engagementId}/contract",r.Value)
            :Problem(r.Error);
    }

    private static async Task<IResult> Generate(
        Guid organizationId,Guid engagementId,GenerateProfessionalServiceContractRequest q,
        IMediator mediator,ICurrentUser user,CancellationToken ct)
    {
        if(user.UserId is not{} actor)return Results.Unauthorized();
        var r=await mediator.Send(new GenerateProfessionalServiceContractCommand(
            new(organizationId),new(engagementId),q.DocumentReference,q.DocumentSha256,actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Revise(
        Guid organizationId,Guid engagementId,ReviseProfessionalServiceContractRequest q,
        IMediator mediator,ICurrentUser user,CancellationToken ct)
    {
        if(user.UserId is not{} actor)return Results.Unauthorized();
        var r=await mediator.Send(new ReviseProfessionalServiceContractCommand(
            new(organizationId),new(engagementId),q.DocumentReference,q.DocumentSha256,q.Reason,actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Send(
        Guid organizationId,Guid engagementId,IMediator mediator,ICurrentUser user,CancellationToken ct)
    {
        if(user.UserId is not{} actor)return Results.Unauthorized();
        var r=await mediator.Send(new SendProfessionalServiceContractForSignatureCommand(
            new(organizationId),new(engagementId),actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> RecordSignature(
        Guid organizationId,Guid engagementId,RecordProfessionalServiceContractSignatureRequest q,
        IMediator mediator,ICurrentUser user,HttpContext http,CancellationToken ct)
    {
        if(user.UserId is not{} actor)return Results.Unauthorized();
        string? ip=q.IpAddress??http.Connection.RemoteIpAddress?.ToString();
        var r=await mediator.Send(new RecordProfessionalServiceContractSignatureCommand(
            new(organizationId),new(engagementId),new PersonId(q.SignatoryPersonId),
            q.DocumentSha256,q.SignatureMethod,q.AuthenticationMethod,q.Provider,
            q.ProviderReference,q.CertificateReference,ip,q.SignedAtUtc,actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Terminate(
        Guid organizationId,Guid engagementId,TerminateProfessionalServiceContractRequest q,
        IMediator mediator,ICurrentUser user,CancellationToken ct)
    {
        if(user.UserId is not{} actor)return Results.Unauthorized();
        Result r=await mediator.Send(new TerminateProfessionalServiceContractCommand(
            new(organizationId),new(engagementId),q.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})
    };
}

internal sealed record CreateProfessionalServiceContractRequest(
    string ContractNumber,string ContractType,ProfessionalContractSignatureOrder SignatureOrder,
    ProfessionalContractSignatoryRequest[] Signatories);
internal sealed record ProfessionalContractSignatoryRequest(Guid PersonId,string Role,int SigningOrder,bool IsRequired);
internal sealed record GenerateProfessionalServiceContractRequest(string DocumentReference,string DocumentSha256);
internal sealed record ReviseProfessionalServiceContractRequest(string DocumentReference,string DocumentSha256,string Reason);
internal sealed record RecordProfessionalServiceContractSignatureRequest(
    Guid SignatoryPersonId,string DocumentSha256,string SignatureMethod,string AuthenticationMethod,
    string Provider,string ProviderReference,string? CertificateReference,string? IpAddress,DateTimeOffset SignedAtUtc);
internal sealed record TerminateProfessionalServiceContractRequest(string Reason);
