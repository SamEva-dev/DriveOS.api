using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FundingBilling.Application.CreditNotes.Create;
using DriveOS.Modules.FundingBilling.Application.CreditNotes.Issue;
using DriveOS.Modules.FundingBilling.Application.CreditNotes.Read;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Api.Endpoints.FundingBilling;
public sealed record CreateCreditNoteLineApiRequest(Guid? InvoiceLineId,string Description,decimal Quantity,string Unit,decimal UnitPrice,decimal DiscountAmount,decimal TaxRate);
public sealed record CreateCreditNoteApiRequest(string Reason,IReadOnlyCollection<CreateCreditNoteLineApiRequest> Lines);
public sealed record IssueCreditNoteApiRequest(DateOnly IssueDate);
public static class CreditNoteEndpoints
{
 public static IEndpointRouteBuilder MapCreditNoteEndpoints(this IEndpointRouteBuilder endpoints){var g=endpoints.MapGroup("/api/finance").WithTags("Funding & Billing - Credit notes");g.MapGet("/credit-notes/{creditNoteId:guid}",GetAsync).RequireAuthorization(DriveOsPermissionCodes.Finance.CreditNotesRead);g.MapGet("/invoices/{invoiceId:guid}/credit-notes",ListAsync).RequireAuthorization(DriveOsPermissionCodes.Finance.CreditNotesRead);g.MapPost("/invoices/{invoiceId:guid}/credit-notes",CreateAsync).RequireAuthorization(DriveOsPermissionCodes.Finance.CreditNotesCreate);g.MapPost("/credit-notes/{creditNoteId:guid}/issue",IssueAsync).RequireAuthorization(DriveOsPermissionCodes.Finance.CreditNotesIssue);return endpoints;}
 static async Task<IResult> GetAsync(Guid creditNoteId,IMediator m,ICurrentTenant t,CancellationToken ct){if(!t.HasTenant||t.OrganizationId is null)return Results.Problem(statusCode:401,title:"errors.currentTenant.required");var r=await m.Send(new GetCreditNoteQuery(t.OrganizationId.Value,new CreditNoteId(creditNoteId)),ct);return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);}
 static async Task<IResult> ListAsync(Guid invoiceId,IMediator m,ICurrentTenant t,CancellationToken ct){if(!t.HasTenant||t.OrganizationId is null)return Results.Problem(statusCode:401,title:"errors.currentTenant.required");var r=await m.Send(new GetInvoiceCreditNotesQuery(t.OrganizationId.Value,new InvoiceId(invoiceId)),ct);return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);}
 static async Task<IResult> CreateAsync(Guid invoiceId,CreateCreditNoteApiRequest request,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!t.HasTenant||t.OrganizationId is null)return Results.Problem(statusCode:401,title:"errors.currentTenant.required");if(u.UserId is null)return Results.Problem(statusCode:401,title:"errors.currentUser.required");var lines=request.Lines.Select(x=>new CreateCreditNoteLineRequest(x.InvoiceLineId.HasValue?new InvoiceLineId(x.InvoiceLineId.Value):null,x.Description,x.Quantity,x.Unit,x.UnitPrice,x.DiscountAmount,x.TaxRate)).ToArray();var r=await m.Send(new CreateCreditNoteCommand(t.OrganizationId.Value,new InvoiceId(invoiceId),request.Reason,lines,u.UserId.Value),ct);return r.IsSuccess?Results.Created($"/api/finance/credit-notes/{r.Value.Value}",r.Value.Value):Problem(r.Error);}
 static async Task<IResult> IssueAsync(Guid creditNoteId,IssueCreditNoteApiRequest request,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!t.HasTenant||t.OrganizationId is null)return Results.Problem(statusCode:401,title:"errors.currentTenant.required");if(u.UserId is null)return Results.Problem(statusCode:401,title:"errors.currentUser.required");var r=await m.Send(new IssueCreditNoteCommand(t.OrganizationId.Value,new CreditNoteId(creditNoteId),request.IssueDate,u.UserId.Value),ct);return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);}
 static IResult Problem(Error e)=>Results.Problem(statusCode:e.Type switch{ErrorType.NotFound=>404,ErrorType.Conflict=>409,ErrorType.Validation=>400,_=>400},title:e.Code,detail:e.MessageKey,extensions:new Dictionary<string,object?>{{"code",e.Code}});
}
