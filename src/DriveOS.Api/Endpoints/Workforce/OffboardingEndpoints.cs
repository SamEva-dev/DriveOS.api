using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Workforce.Application.Offboarding;
using DriveOS.Modules.Workforce.Domain.Offboarding;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Api.Endpoints.Workforce;
internal static class OffboardingEndpoints
{
    internal static IEndpointRouteBuilder MapOffboardingEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/workforce/employees/{employeeId:guid}/offboarding").WithTags("Workforce - Offboarding");
        g.MapGet("/",Get).RequireAuthorization("Workforce.Offboarding.Read");
        g.MapPost("/refresh",Refresh).RequireAuthorization("Workforce.Offboarding.Manage");
        g.MapPost("/items/{kind}/complete",CompleteItem).RequireAuthorization("Workforce.Offboarding.Manage");
        g.MapPost("/items/{kind}/waive",WaiveItem).RequireAuthorization("Workforce.Offboarding.Waive");
        g.MapPost("/access/revoke",RevokeAccess).RequireAuthorization("Workforce.Offboarding.Manage");
        g.MapPost("/complete",Complete).RequireAuthorization("Workforce.Offboarding.Complete");
        return app;
    }
    private static async Task<IResult> Get(Guid employeeId,IMediator mediator,ICurrentTenant tenant,CancellationToken ct)
    { if(tenant.OrganizationId is not{} org)return Results.Unauthorized();var r=await mediator.Send(new GetEmployeeOffboardingQuery(org,new EmployeeId(employeeId)),ct);return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error); }
    private static async Task<IResult> Refresh(Guid employeeId,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    { if(tenant.OrganizationId is not{} org||user.UserId is not{} actor)return Results.Unauthorized();var r=await mediator.Send(new RefreshOffboardingCommand(org,new EmployeeId(employeeId),actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error); }
    private static async Task<IResult> CompleteItem(Guid employeeId,string kind,ItemRequest request,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    { if(tenant.OrganizationId is not{} org||user.UserId is not{} actor)return Results.Unauthorized();if(!Enum.TryParse<OffboardingChecklistItemKind>(kind,true,out var parsed))return Results.BadRequest();var r=await mediator.Send(new CompleteOffboardingChecklistItemCommand(org,new EmployeeId(employeeId),parsed,request.Note,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error); }
    private static async Task<IResult> RevokeAccess(Guid employeeId,AccessRevocationRequest request,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    { if(tenant.OrganizationId is not{} org||user.UserId is not{} actor)return Results.Unauthorized();var r=await mediator.Send(new RevokeOffboardingAccessCommand(org,new EmployeeId(employeeId),request.Reason,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error); }
    private static async Task<IResult> WaiveItem(Guid employeeId,string kind,WaiverRequest request,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    { if(tenant.OrganizationId is not{} org||user.UserId is not{} actor)return Results.Unauthorized();if(!Enum.TryParse<OffboardingChecklistItemKind>(kind,true,out var parsed))return Results.BadRequest();var r=await mediator.Send(new WaiveOffboardingChecklistItemCommand(org,new EmployeeId(employeeId),parsed,request.Reason,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error); }
    private static async Task<IResult> Complete(Guid employeeId,CompletionRequest request,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    { if(tenant.OrganizationId is not{} org||user.UserId is not{} actor)return Results.Unauthorized();var r=await mediator.Send(new CompleteOffboardingCommand(org,new EmployeeId(employeeId),request.Reason,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error); }
    private static IResult Problem(Error e)=>Results.Problem(statusCode:e.Type switch{ErrorType.NotFound=>404,ErrorType.Conflict=>409,ErrorType.Validation=>400,_=>400},extensions:new Dictionary<string,object?>{{"code",e.Code},{"messageKey",e.MessageKey}});
    public sealed record ItemRequest(string? Note);
    public sealed record WaiverRequest(string Reason);
    public sealed record CompletionRequest(string Reason);
    public sealed record AccessRevocationRequest(string Reason);
}
