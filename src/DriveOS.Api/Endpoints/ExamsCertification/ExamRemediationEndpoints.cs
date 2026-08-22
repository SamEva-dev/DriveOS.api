using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Remediation;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamRemediationEndpoints
{
    internal static IEndpointRouteBuilder MapExamRemediationEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams/remediations").WithTags("Exams & Certification");
        group.MapGet("/{requestId:guid}", Get).RequireAuthorization(DriveOsPermissionCodes.Exams.RemediationRead);
        group.MapGet("/result/{resultId:guid}/revision/{revision:int}", GetByResult).RequireAuthorization(DriveOsPermissionCodes.Exams.RemediationRead);
        group.MapPost("/result/{resultId:guid}/revision/{revision:int}", Create).RequireAuthorization(DriveOsPermissionCodes.Exams.RemediationManage);
        group.MapPut("/{requestId:guid}/configuration", Configure).RequireAuthorization(DriveOsPermissionCodes.Exams.RemediationManage);
        group.MapPost("/{requestId:guid}/provision", Provision).RequireAuthorization(DriveOsPermissionCodes.Exams.RemediationManage);
        group.MapPost("/{requestId:guid}/refresh", Refresh).RequireAuthorization(DriveOsPermissionCodes.Exams.RemediationManage);
        group.MapPost("/{requestId:guid}/validate-representation", Validate).RequireAuthorization(DriveOsPermissionCodes.Exams.RemediationManage);
        group.MapPost("/{requestId:guid}/cancel", Cancel).RequireAuthorization(DriveOsPermissionCodes.Exams.RemediationManage);
        app.MapGet("/api/exams/students/{studentId:guid}/remediations", ListStudent).WithTags("Exams & Certification")
            .RequireAuthorization(DriveOsPermissionCodes.Exams.RemediationRead);
        return app;
    }

    private static async Task<IResult> Get(Guid requestId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    { if(tenant.OrganizationId is not {} org)return Results.Unauthorized(); var r=await mediator.Send(new GetExamRemediationRequestQuery(org,new ExamRemediationRequestId(requestId)),ct); return ToResult(r); }
    private static async Task<IResult> GetByResult(Guid resultId,int revision,IMediator mediator,ICurrentTenant tenant,CancellationToken ct)
    { if(tenant.OrganizationId is not {} org)return Results.Unauthorized(); var r=await mediator.Send(new GetExamRemediationByResultQuery(org,new ExamResultId(resultId),revision),ct); return ToResult(r); }
    private static async Task<IResult> Create(Guid resultId,int revision,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    { if(tenant.OrganizationId is not {} org||user.UserId is not {} uid)return Results.Unauthorized(); var r=await mediator.Send(new CreateExamRemediationRequestCommand(org,new ExamResultId(resultId),revision,uid),ct); return ToResult(r); }
    private sealed record ConfigureRequest(Guid TrainingPathId, Guid ResponsibleUserId, DateOnly ReviewDate, DateOnly? TargetDate,
        bool MockExamRequired, bool FundingReviewRequired, int? RecommendedHours);
    private static async Task<IResult> Configure(Guid requestId,ConfigureRequest body,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    { if(tenant.OrganizationId is not {} org||user.UserId is not {} uid)return Results.Unauthorized(); var r=await mediator.Send(new ConfigureExamRemediationRequestCommand(org,new ExamRemediationRequestId(requestId),new TrainingPathId(body.TrainingPathId),new UserId(body.ResponsibleUserId), body.ReviewDate, body.TargetDate, body.MockExamRequired, body.FundingReviewRequired, body.RecommendedHours, uid),ct); return ToResult(r); }
    private static async Task<IResult> Provision(Guid requestId,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    { if(tenant.OrganizationId is not {} org||user.UserId is not {} uid)return Results.Unauthorized(); var r=await mediator.Send(new ProvisionExamRemediationPlanCommand(org,new ExamRemediationRequestId(requestId),uid),ct); return ToResult(r); }
    private static async Task<IResult> Refresh(Guid requestId,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    { if(tenant.OrganizationId is not {} org||user.UserId is not {} uid)return Results.Unauthorized(); var r=await mediator.Send(new RefreshExamRemediationRequestCommand(org,new ExamRemediationRequestId(requestId),uid),ct); return ToResult(r); }
    private static async Task<IResult> Validate(Guid requestId,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    { if(tenant.OrganizationId is not {} org||user.UserId is not {} uid)return Results.Unauthorized(); var r=await mediator.Send(new ValidateExamRemediationForRePresentationCommand(org,new ExamRemediationRequestId(requestId),uid),ct); return ToResult(r); }
    private sealed record CancelRequest(string Reason);
    private static async Task<IResult> Cancel(Guid requestId,CancelRequest body,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    { if(tenant.OrganizationId is not {} org||user.UserId is not {} uid)return Results.Unauthorized(); var r=await mediator.Send(new CancelExamRemediationRequestCommand(org,new ExamRemediationRequestId(requestId),body.Reason,uid),ct); return ToResult(r); }
    private static async Task<IResult> ListStudent(Guid studentId,IMediator mediator,ICurrentTenant tenant,CancellationToken ct)
    { if(tenant.OrganizationId is not {} org)return Results.Unauthorized(); var r=await mediator.Send(new GetStudentExamRemediationsQuery(org,new PersonId(studentId)),ct); return r.IsSuccess?Results.Ok(r.Value):Failure(r.Error); }
    private static IResult ToResult(Result<ExamRemediationRequestResponse> r)=>r.IsSuccess?Results.Ok(r.Value):Failure(r.Error);
    private static IResult Failure(Error e)=>Results.Problem(statusCode:e.Type switch{ErrorType.NotFound=>404,ErrorType.Conflict=>409,ErrorType.Validation=>400,_=>400},title:e.Code,extensions:new Dictionary<string,object?>{{"code",e.Code},{"messageKey",e.MessageKey}});
}
