using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FundingBilling.Application.StudentFinance.Read;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.FundingBilling;

public static class StudentFinancialOverviewEndpoints
{
    public static IEndpointRouteBuilder MapStudentFinancialOverviewEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/finance/students")
            .WithTags("Funding & Billing - Student Finance");

        group.MapGet("/{studentId:guid}/overview", GetOverviewAsync)
            .Produces<StudentFinancialOverviewResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Finance.SummaryRead);

        return endpoints;
    }

    private static async Task<IResult> GetOverviewAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "errors.currentTenant.required");

        Result<StudentFinancialOverviewResponse> result = await mediator.Send(
            new GetStudentFinancialOverviewQuery(
                tenant.OrganizationId.Value,
                new PersonId(studentId),
                DateOnly.FromDateTime(DateTime.UtcNow)),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static IResult Problem(Error error) => Results.Problem(
        statusCode: error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        },
        title: error.Code,
        detail: error.MessageKey,
        extensions: new Dictionary<string, object?> { ["code"] = error.Code });
}
