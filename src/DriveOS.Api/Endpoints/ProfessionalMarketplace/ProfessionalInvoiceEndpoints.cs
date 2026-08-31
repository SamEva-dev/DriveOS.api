using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.Invoices;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Invoices;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalInvoiceEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Professional Invoices");

        g.MapGet("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/invoices",ListForEngagement)
            .RequireAuthorization("ProfessionalMarketplace.Invoices.Read");

        g.MapGet("/organizations/{organizationId:guid}/invoices/{invoiceId:guid}",GetForOrganization)
            .RequireAuthorization("ProfessionalMarketplace.Invoices.Read");

        g.MapPost("/organizations/{organizationId:guid}/service-statements/{statementId:guid}/invoices",Create)
            .RequireAuthorization("ProfessionalMarketplace.Invoices.Request");

        g.MapPut("/organizations/{organizationId:guid}/invoices/{invoiceId:guid}",UpdateOrganizationDraft)
            .RequireAuthorization("ProfessionalMarketplace.Invoices.Request");

        g.MapPost("/organizations/{organizationId:guid}/invoices/{invoiceId:guid}/validate",ValidateOrganization)
            .RequireAuthorization("ProfessionalMarketplace.Invoices.Request");

        g.MapPut("/profiles/{profileId:guid}/invoices/{invoiceId:guid}",UpdateDraft)
            .RequireAuthorization("ProfessionalMarketplace.Invoices.Request");

        g.MapPost("/profiles/{profileId:guid}/invoices/{invoiceId:guid}/validate",Validate)
            .RequireAuthorization("ProfessionalMarketplace.Invoices.Request");

        g.MapPost("/organizations/{organizationId:guid}/invoices/{invoiceId:guid}/request-finance",RequestFinance)
            .RequireAuthorization("ProfessionalMarketplace.Invoices.Request");

        g.MapPost("/organizations/{organizationId:guid}/invoices/{invoiceId:guid}/sync-finance",SyncFinance)
            .RequireAuthorization("ProfessionalMarketplace.Invoices.Read");

        return app;
    }

    private static async Task<IResult> ListForEngagement(Guid organizationId,Guid engagementId,IMediator m,CancellationToken ct)
    {
        var r=await m.Send(new ListOrganizationProfessionalInvoicesQuery(new(organizationId),new(engagementId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> GetForOrganization(Guid organizationId,Guid invoiceId,IMediator m,CancellationToken ct)
    {
        var r=await m.Send(new GetOrganizationProfessionalInvoiceQuery(new(organizationId),new(invoiceId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Create(Guid organizationId,Guid statementId,CreateProfessionalInvoiceRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new ProfessionalInvoiceId(Guid.NewGuid());
        var r=await m.Send(new CreateProfessionalInvoiceCommand(id,new(organizationId),new(statementId),q.Mode,
            q.IssueDate,q.DueDate,q.TaxAmount,q.InvoiceNumber,q.BankReference,actor),ct);
        return r.IsSuccess?Results.Created($"/api/professional-marketplace/organizations/{organizationId}/invoices/{id.Value}",new{id=id.Value}):Problem(r.Error);
    }

    private static async Task<IResult> UpdateOrganizationDraft(Guid organizationId,Guid invoiceId,UpdateProfessionalInvoiceRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new UpdateOrganizationProfessionalInvoiceDraftCommand(new(invoiceId),new(organizationId),q.IssueDate,q.DueDate,q.TaxAmount,q.InvoiceNumber,q.BankReference,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> ValidateOrganization(Guid organizationId,Guid invoiceId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new ValidateOrganizationProfessionalInvoiceCommand(new(invoiceId),new(organizationId),actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> UpdateDraft(Guid profileId,Guid invoiceId,UpdateProfessionalInvoiceRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new UpdateCurrentProfessionalInvoiceDraftCommand(actor,new(invoiceId),q.IssueDate,q.DueDate,q.TaxAmount,q.InvoiceNumber,q.BankReference),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> Validate(Guid profileId,Guid invoiceId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new ValidateCurrentProfessionalInvoiceCommand(actor,new(invoiceId)),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> RequestFinance(Guid organizationId,Guid invoiceId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new RequestProfessionalInvoiceCommand(new(invoiceId),new(organizationId),actor),ct);
        return r.IsSuccess?Results.Accepted():Problem(r.Error);
    }


    private static async Task<IResult> SyncFinance(Guid organizationId,Guid invoiceId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new SyncProfessionalInvoiceFinanceStatusCommand(new(invoiceId),new(organizationId),actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})
    };
}
internal sealed record CreateProfessionalInvoiceRequest(ProfessionalInvoiceMode Mode,DateOnly IssueDate,DateOnly DueDate,decimal TaxAmount,string? InvoiceNumber,string? BankReference);
internal sealed record UpdateProfessionalInvoiceRequest(DateOnly IssueDate,DateOnly DueDate,decimal TaxAmount,string? InvoiceNumber,string? BankReference);
