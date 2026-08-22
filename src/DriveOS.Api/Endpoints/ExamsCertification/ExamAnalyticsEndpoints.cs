using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Analytics;
using DriveOS.Security.Contracts;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamAnalyticsEndpoints
{
    internal static IEndpointRouteBuilder MapExamAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/exams/analytics/results", GetResultsAnalytics)
            .WithTags("Exams & Certification")
            .RequireAuthorization(DriveOsPermissionCodes.Exams.AnalyticsRead);
        return app;
    }

    private static async Task<IResult> GetResultsAnalytics(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? examType,
        string? licenseCategory,
        Guid? examCenterId,
        Guid? instructorId,
        Guid? branchId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId)
            return Results.Unauthorized();

        var filter = new ExamAnalyticsFilter(
            fromUtc,
            toUtc,
            examType,
            licenseCategory,
            examCenterId,
            instructorId,
            branchId);

        var result = await mediator.Send(new GetExamAnalyticsQuery(organizationId, filter), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(
            statusCode: 400,
            title: result.Error.Code,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = result.Error.Code,
                ["messageKey"] = result.Error.MessageKey
            });
    }
}
