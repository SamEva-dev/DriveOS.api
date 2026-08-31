using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.StudentAssignments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalStudentAssignmentEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalStudentAssignmentEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Student Assignments");

        g.MapGet("/organizations/{organizationId:guid}/missions/{missionId:guid}/student-assignments",List)
            .RequireAuthorization("ProfessionalMarketplace.StudentAssignments.Read");

        g.MapPost("/organizations/{organizationId:guid}/missions/{missionId:guid}/student-assignments",Assign)
            .RequireAuthorization("ProfessionalMarketplace.StudentAssignments.Manage");

        g.MapPost("/organizations/{organizationId:guid}/student-assignments/{assignmentId:guid}/revoke",Revoke)
            .RequireAuthorization("ProfessionalMarketplace.StudentAssignments.Revoke");

        g.MapGet("/me/student-assignments",ListMine)
            .RequireAuthorization("ProfessionalMarketplace.StudentAssignments.Read");

        g.MapGet("/me/missions/{missionId:guid}/student-assignments",ListMineForMission)
            .RequireAuthorization("ProfessionalMarketplace.StudentAssignments.Read");

        return app;
    }

    private static async Task<IResult> List(
        Guid organizationId,Guid missionId,IMediator mediator,CancellationToken ct)
    {
        Result<IReadOnlyList<ProfessionalStudentAssignmentListItem>> r=await mediator.Send(
            new GetProfessionalMissionStudentAssignmentsQuery(new(organizationId),new(missionId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> ListMine(
        IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} actor)return Results.Unauthorized();
        Result<IReadOnlyList<ProfessionalStudentAssignmentListItem>> r=await mediator.Send(
            new GetCurrentProfessionalStudentAssignmentsQuery(actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> ListMineForMission(
        Guid missionId,IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} actor)return Results.Unauthorized();
        Result<IReadOnlyList<ProfessionalStudentAssignmentListItem>> r=await mediator.Send(
            new GetCurrentProfessionalMissionStudentAssignmentsQuery(actor,new(missionId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Assign(
        Guid organizationId,Guid missionId,AssignProfessionalStudentRequest request,
        IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} actor)return Results.Unauthorized();
        var id=new ProfessionalStudentAssignmentId(Guid.NewGuid());

        var r=await mediator.Send(new AssignStudentToProfessionalMissionCommand(
            id,new(organizationId),new(missionId),new(request.StudentId),
            request.StartsOn,request.EndsOn,request.ScopeCode,request.AssignmentReason,actor),ct);

        return r.IsSuccess
            ?Results.Created($"/api/professional-marketplace/organizations/{organizationId}/student-assignments/{id.Value}",new{id=id.Value})
            :Problem(r.Error);
    }

    private static async Task<IResult> Revoke(
        Guid organizationId,Guid assignmentId,RevokeProfessionalStudentRequest request,
        IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} actor)return Results.Unauthorized();
        Result r=await mediator.Send(new RevokeProfessionalStudentAssignmentCommand(
            new(assignmentId),new(organizationId),request.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})
    };
}

internal sealed record AssignProfessionalStudentRequest(
    Guid StudentId,DateOnly StartsOn,DateOnly EndsOn,string ScopeCode,string AssignmentReason);
internal sealed record RevokeProfessionalStudentRequest(string Reason);
