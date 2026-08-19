using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.CurriculumPedagogy.Application.StudentOverview;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Endpoints.CurriculumPedagogy;

internal static class StudentPedagogyOverviewEndpoints
{
    internal static RouteGroupBuilder MapStudentPedagogyOverviewEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/students/{studentId:guid}/overview", GetOverview)
            .RequireAuthorization("Pedagogy.Summary.Read");
        return group;
    }

    private static async Task<IResult> GetOverview(
        Guid studentId,
        Guid? trainingPathId,
        IStudentPedagogyOverviewReadService readService,
        ICurrentTenant currentTenant,
        CancellationToken cancellationToken)
    {
        if (currentTenant.OrganizationId is not { } organizationId)
            return Results.Unauthorized();

        var result = await readService.GetAsync(
            organizationId,
            new PersonId(studentId),
            trainingPathId.HasValue ? new TrainingPathId(trainingPathId.Value) : null,
            cancellationToken);

        return result is null ? Results.NotFound() : Results.Ok(result);
    }
}
