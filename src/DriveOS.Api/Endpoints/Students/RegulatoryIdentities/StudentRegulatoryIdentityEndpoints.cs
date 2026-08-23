using DomainRelay.Abstractions;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Students.Application.RegulatoryIdentities;
using DriveOS.Modules.Students.Domain.RegulatoryIdentities;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Students.RegulatoryIdentities;

public sealed record DeclareStudentRegulatoryIdentityRequest(
    string CountryCode,
    string IdentifierType,
    string Value,
    StudentRegulatoryIdentitySource Source = StudentRegulatoryIdentitySource.Manual);

public sealed record VerifyStudentRegulatoryIdentityRequest(string VerificationMethod, string? Reason);
public sealed record RejectStudentRegulatoryIdentityRequest(string Reason);

public static class StudentRegulatoryIdentityEndpoints
{
    public static IEndpointRouteBuilder MapStudentRegulatoryIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/students/{studentId:guid}/regulatory-identities")
            .WithTags("Students - Regulatory identities");

        group.MapGet("/", GetAsync)
            .RequireAuthorization(DriveOsPermissionCodes.Students.RegulatoryIdentityRead);
        group.MapPost("/", DeclareAsync)
            .RequireAuthorization(DriveOsPermissionCodes.Students.RegulatoryIdentityManage);
        group.MapPost("/{identityId:guid}/verify", VerifyAsync)
            .RequireAuthorization(DriveOsPermissionCodes.Students.RegulatoryIdentityVerify);
        group.MapPost("/{identityId:guid}/reject", RejectAsync)
            .RequireAuthorization(DriveOsPermissionCodes.Students.RegulatoryIdentityVerify);
        return endpoints;
    }

    private static async Task<IResult> GetAsync(
        Guid studentId, IMediator mediator, ICurrentTenant tenant, HttpContext http, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Forbid();
        Result<IReadOnlyList<StudentRegulatoryIdentityResponse>> result = await mediator.Send(
            new GetStudentRegulatoryIdentitiesQuery(organizationId, new PersonId(studentId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToHttpResult(http);
    }

    private static async Task<IResult> DeclareAsync(
        Guid studentId, DeclareStudentRegulatoryIdentityRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, HttpContext http, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Forbid();
        Result<StudentRegulatoryIdentityResponse> result = await mediator.Send(
            new DeclareStudentRegulatoryIdentityCommand(
                organizationId, new PersonId(studentId), request.CountryCode,
                request.IdentifierType, request.Value, request.Source, actorUserId), ct);
        return result.IsSuccess
            ? Results.Created($"/api/students/{studentId}/regulatory-identities/{result.Value.Id}", result.Value)
            : result.Error.ToHttpResult(http);
    }

    private static async Task<IResult> VerifyAsync(
        Guid studentId, Guid identityId, VerifyStudentRegulatoryIdentityRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, HttpContext http, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Forbid();
        Result<StudentRegulatoryIdentityResponse> result = await mediator.Send(
            new VerifyStudentRegulatoryIdentityCommand(
                organizationId, new PersonId(studentId), new StudentRegulatoryIdentityId(identityId),
                request.VerificationMethod, request.Reason, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToHttpResult(http);
    }

    private static async Task<IResult> RejectAsync(
        Guid studentId, Guid identityId, RejectStudentRegulatoryIdentityRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, HttpContext http, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Forbid();
        Result<StudentRegulatoryIdentityResponse> result = await mediator.Send(
            new RejectStudentRegulatoryIdentityCommand(
                organizationId, new PersonId(studentId), new StudentRegulatoryIdentityId(identityId),
                request.Reason, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToHttpResult(http);
    }
}
