using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Workforce.Application.LeaveRequests;
using DriveOS.Modules.Workforce.Domain.LeaveRequests;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Api.Endpoints.Workforce;
internal static class LeaveRequestEndpoints
{
    internal static IEndpointRouteBuilder MapLeaveRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/workforce/leave-requests").WithTags("Workforce - Leave Requests");
        g.MapGet("/", GetAll).RequireAuthorization("Workforce.LeaveRequests.Read");
        g.MapGet("/{id:guid}", GetOne).RequireAuthorization("Workforce.LeaveRequests.Read");
        g.MapPost("/", Create).RequireAuthorization("Workforce.LeaveRequests.Manage");
        g.MapPut("/{id:guid}", Update).RequireAuthorization("Workforce.LeaveRequests.Manage");
        g.MapPost("/{id:guid}/submit", Submit).RequireAuthorization("Workforce.LeaveRequests.Submit");
        g.MapPost("/{id:guid}/approve", Approve).RequireAuthorization("Workforce.LeaveRequests.Approve");
        g.MapPost("/{id:guid}/reject", Reject).RequireAuthorization("Workforce.LeaveRequests.Approve");
        g.MapPost("/{id:guid}/cancel", Cancel).RequireAuthorization("Workforce.LeaveRequests.Manage");
        return app;
    }
    static async Task<IResult> GetAll(Guid? employeeId,string? status,DateOnly? from,DateOnly? to,IMediator m,ICurrentTenant t,CancellationToken ct)
    { if(t.OrganizationId is not{}o)return Results.Unauthorized();LeaveRequestStatus? s=null;if(!string.IsNullOrWhiteSpace(status)){if(!Enum.TryParse<LeaveRequestStatus>(status,true,out var x))return Results.BadRequest(new{code="Workforce.LeaveRequest.InvalidStatus",messageKey="errors.workforce.leaveRequest.invalidStatus"});s=x;}var r=await m.Send(new GetLeaveRequestsQuery(o,employeeId is{}e?new EmployeeId(e):null,s,from,to),ct);return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error); }
    static async Task<IResult> GetOne(Guid id,IMediator m,ICurrentTenant t,CancellationToken ct){if(t.OrganizationId is not{}o)return Results.Unauthorized();var r=await m.Send(new GetLeaveRequestQuery(o,new LeaveRequestId(id)),ct);return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);}
    static async Task<IResult> Create(LeaveRequestRequest q,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{}o||u.UserId is not{}a)return Results.Unauthorized();if(!TryPortion(q.StartPortion,out var sp)||!TryPortion(q.EndPortion,out var ep))return InvalidPortion();var id=q.Id is{}x&&x!=Guid.Empty?new LeaveRequestId(x):LeaveRequestId.New();var r=await m.Send(new CreateLeaveRequestCommand(o,id,new EmployeeId(q.EmployeeId),new LeavePolicyId(q.LeavePolicyId),q.StartDate,q.EndDate,sp,ep,q.Reason,q.EvidenceDocumentId is{}d?new DocumentId(d):null,a),ct);return r.IsSuccess?Results.Created($"/api/workforce/leave-requests/{r.Value.Value}",new{id=r.Value.Value}):Problem(r.Error);}
    static async Task<IResult> Update(Guid id,LeaveRequestUpdateRequest q,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{}o||u.UserId is not{}a)return Results.Unauthorized();if(!TryPortion(q.StartPortion,out var sp)||!TryPortion(q.EndPortion,out var ep))return InvalidPortion();var r=await m.Send(new UpdateLeaveRequestCommand(o,new LeaveRequestId(id),q.StartDate,q.EndDate,sp,ep,q.Reason,q.EvidenceDocumentId is{}d?new DocumentId(d):null,a),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    static async Task<IResult> Submit(Guid id,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{}o||u.UserId is not{}a)return Results.Unauthorized();var r=await m.Send(new SubmitLeaveRequestCommand(o,new LeaveRequestId(id),a),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    static async Task<IResult> Approve(Guid id,LeaveDecisionRequest q,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{}o||u.UserId is not{}a)return Results.Unauthorized();var r=await m.Send(new ApproveLeaveRequestCommand(o,new LeaveRequestId(id),q.Reason,a),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    static async Task<IResult> Reject(Guid id,LeaveDecisionRequest q,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{}o||u.UserId is not{}a)return Results.Unauthorized();var r=await m.Send(new RejectLeaveRequestCommand(o,new LeaveRequestId(id),q.Reason??string.Empty,a),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    static async Task<IResult> Cancel(Guid id,LeaveDecisionRequest q,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{}o||u.UserId is not{}a)return Results.Unauthorized();var r=await m.Send(new CancelLeaveRequestCommand(o,new LeaveRequestId(id),q.Reason,a),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    static bool TryPortion(string value,out LeaveDayPortion portion)=>Enum.TryParse(value,true,out portion);
    static IResult InvalidPortion()=>Results.BadRequest(new{code="Workforce.LeaveRequest.InvalidDayPortion",messageKey="errors.workforce.leaveRequest.invalidDayPortion"});
    static IResult Problem(Error e)=>Results.Problem(statusCode:e.Type switch{ErrorType.NotFound=>404,ErrorType.Conflict=>409,ErrorType.Validation=>400,_=>400},extensions:new Dictionary<string,object?>{{"code",e.Code},{"messageKey",e.MessageKey}});
}
public sealed record LeaveRequestRequest(Guid? Id,Guid EmployeeId,Guid LeavePolicyId,DateOnly StartDate,DateOnly EndDate,string StartPortion,string EndPortion,string? Reason,Guid? EvidenceDocumentId);
public sealed record LeaveRequestUpdateRequest(DateOnly StartDate,DateOnly EndDate,string StartPortion,string EndPortion,string? Reason,Guid? EvidenceDocumentId);
public sealed record LeaveDecisionRequest(string? Reason);
