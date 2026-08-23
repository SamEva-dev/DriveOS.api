using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Workforce.Application.JobPositions;
using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Api.Endpoints.Workforce;
internal static class JobPositionEndpoints
{
    internal static IEndpointRouteBuilder MapJobPositionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/workforce/job-positions").WithTags("Workforce - Job Positions");
        group.MapGet("/", GetAll).RequireAuthorization("Workforce.JobPositions.Read");
        group.MapGet("/{jobPositionId:guid}", GetOne).RequireAuthorization("Workforce.JobPositions.Read");
        group.MapPost("/", Create).RequireAuthorization("Workforce.JobPositions.Manage");
        group.MapPut("/{jobPositionId:guid}", Update).RequireAuthorization("Workforce.JobPositions.Manage");
        group.MapPost("/{jobPositionId:guid}/deactivate", Deactivate).RequireAuthorization("Workforce.JobPositions.Manage");
        group.MapPost("/{jobPositionId:guid}/reactivate", Reactivate).RequireAuthorization("Workforce.JobPositions.Manage");
        return app;
    }
    private static async Task<IResult> GetAll(string? status, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org) return Results.Unauthorized();
        JobPositionStatus? parsed=null; if(!string.IsNullOrWhiteSpace(status)){if(!Enum.TryParse<JobPositionStatus>(status,true,out var v))return Results.BadRequest(new { code="Workforce.JobPosition.InvalidStatus", messageKey="errors.workforce.jobPosition.invalidStatus"});parsed=v;}
        Result<IReadOnlyList<JobPositionResponse>> r=await mediator.Send(new GetJobPositionsQuery(org,parsed),ct); return r.IsSuccess?Results.Ok(r.Value):ToProblem(r.Error);
    }
    private static async Task<IResult> GetOne(Guid jobPositionId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct){if(tenant.OrganizationId is not { } org)return Results.Unauthorized();Result<JobPositionResponse> r=await mediator.Send(new GetJobPositionQuery(org,new JobPositionId(jobPositionId)),ct);return r.IsSuccess?Results.Ok(r.Value):ToProblem(r.Error);}
    private static async Task<IResult> Create(CreateJobPositionRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();if(!Enum.TryParse<ProfessionalFunction>(request.ProfessionalFunction,true,out var function))return Results.BadRequest(new{code="Workforce.JobPosition.InvalidProfessionalFunction",messageKey="errors.workforce.jobPosition.invalidProfessionalFunction"});JobPositionId id=request.JobPositionId is { } raw&&raw!=Guid.Empty?new JobPositionId(raw):JobPositionId.New();Result<JobPositionId> r=await mediator.Send(new CreateJobPositionCommand(org,id,request.Code,request.Name,request.Description,function,actor),ct);return r.IsSuccess?Results.Created($"/api/workforce/job-positions/{r.Value.Value}",new{id=r.Value.Value}):ToProblem(r.Error);}
    private static async Task<IResult> Update(Guid jobPositionId, UpdateJobPositionRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();if(!Enum.TryParse<ProfessionalFunction>(request.ProfessionalFunction,true,out var function))return Results.BadRequest(new{code="Workforce.JobPosition.InvalidProfessionalFunction",messageKey="errors.workforce.jobPosition.invalidProfessionalFunction"});Result r=await mediator.Send(new UpdateJobPositionCommand(org,new JobPositionId(jobPositionId),request.Code,request.Name,request.Description,function,actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static async Task<IResult> Deactivate(Guid jobPositionId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct){if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();Result r=await mediator.Send(new DeactivateJobPositionCommand(org,new JobPositionId(jobPositionId),actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static async Task<IResult> Reactivate(Guid jobPositionId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct){if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();Result r=await mediator.Send(new ReactivateJobPositionCommand(org,new JobPositionId(jobPositionId),actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static IResult ToProblem(Error error)=>Results.Problem(statusCode:error.Type switch{ErrorType.NotFound=>404,ErrorType.Conflict=>409,ErrorType.Validation=>400,_=>400},extensions:new Dictionary<string,object?>{{"code",error.Code},{"messageKey",error.MessageKey}});
}
public sealed record CreateJobPositionRequest(Guid? JobPositionId,string Code,string Name,string? Description,string ProfessionalFunction);
public sealed record UpdateJobPositionRequest(string Code,string Name,string? Description,string ProfessionalFunction);
