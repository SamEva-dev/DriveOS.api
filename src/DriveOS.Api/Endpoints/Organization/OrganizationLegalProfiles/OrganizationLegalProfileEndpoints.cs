using DomainRelay.Abstractions;
using DomainRelay.Mapping.Abstractions.Services;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Activate;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Archive;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Create;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.GetByOrganization;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Models;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Update;
using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Organization.OrganizationLegalProfiles;

public static class OrganizationLegalProfileEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationLegalProfileEndpoints(
        this IEndpointRouteBuilder endpoints
    )
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/organizations/{organizationId:guid}/legal-profile")
            .WithTags("Organization legal profile");

        group
            .MapGet("/", GetAsync)
            .WithName("GetOrganizationLegalProfile")
            .Produces<OrganizationLegalProfileResponseContract>()
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationLegalProfiles.Read);

        group
            .MapPost("/", CreateAsync)
            .WithName("CreateOrganizationLegalProfile")
            .Produces(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationLegalProfiles.Create);

        group
            .MapPut("/", UpdateAsync)
            .WithName("UpdateOrganizationLegalProfile")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationLegalProfiles.Update);

        group
            .MapPost("/activate", ActivateAsync)
            .WithName("ActivateOrganizationLegalProfile")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationLegalProfiles.Activate);

        group
            .MapPost("/archive", ArchiveAsync)
            .WithName("ArchiveOrganizationLegalProfile")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationLegalProfiles.Archive);

        return endpoints;
    }

    private static async Task<IResult> GetAsync(
        Guid organizationId,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryOrganization(
                organizationId,
                tenant,
                context,
                out OrganizationId organization,
                out IResult? failure
            )
        )
            return failure!;

        Result<OrganizationLegalProfileResponse> result = await mediator.Send(
            new GetOrganizationLegalProfileQuery(organization),
            cancellationToken
        );

        if (result.IsFailure)
            return result.Error.ToHttpResult(context);

        OrganizationLegalProfileResponse source = result.Value;

        var response = new OrganizationLegalProfileResponseContract(
            source.Id.Value,
            source.OrganizationId.Value,
            source.LegalForm.ToString(),
            source.RegistrationNumber,
            source.TaxNumber,
            source.TradeName,
            source.IncorporationDate,
            source.AddressLine1,
            source.AddressLine2,
            source.PostalCode,
            source.City,
            source.Region,
            source.CountryCode,
            source.Status.ToString(),
            source.Revision,
            source.CreatedAtUtc,
            source.CreatedByUserId?.Value,
            source.LastModifiedAtUtc,
            source.LastModifiedByUserId?.Value
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateAsync(
        Guid organizationId,
        CreateOrganizationLegalProfileRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryOrganization(
                organizationId,
                tenant,
                context,
                out OrganizationId organization,
                out IResult? failure
            )
        )
            return failure!;

        var model = new CreateOrganizationLegalProfileApiModel(
            organization,
            request.LegalForm,
            request.RegistrationNumber,
            request.TaxNumber,
            request.TradeName,
            request.IncorporationDate,
            request.AddressLine1,
            request.AddressLine2,
            request.PostalCode,
            request.City,
            request.Region,
            request.CountryCode,
            request.ActivateImmediately
        );

        Result<OrganizationLegalProfileId> result = await mediator.Send(
            mapper.Map<
                CreateOrganizationLegalProfileApiModel,
                CreateOrganizationLegalProfileCommand
            >(model),
            cancellationToken
        );

        return result.IsSuccess
            ? Results.Created(
                $"/api/organizations/{organizationId}/legal-profile",
                new { id = result.Value.Value }
            )
            : result.Error.ToHttpResult(context);
    }

    private static async Task<IResult> UpdateAsync(
        Guid organizationId,
        UpdateOrganizationLegalProfileRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryOrganization(
                organizationId,
                tenant,
                context,
                out OrganizationId organization,
                out IResult? failure
            )
        )
            return failure!;

        var model = new UpdateOrganizationLegalProfileApiModel(
            organization,
            request.LegalForm,
            request.RegistrationNumber,
            request.TaxNumber,
            request.TradeName,
            request.IncorporationDate,
            request.AddressLine1,
            request.AddressLine2,
            request.PostalCode,
            request.City,
            request.Region,
            request.CountryCode,
            request.ExpectedRevision
        );

        Result result = await mediator.Send(
            mapper.Map<
                UpdateOrganizationLegalProfileApiModel,
                UpdateOrganizationLegalProfileCommand
            >(model),
            cancellationToken
        );

        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(context);
    }

    private static Task<IResult> ActivateAsync(
        Guid organizationId,
        ChangeOrganizationLegalProfileStatusRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken cancellationToken
    ) =>
        ChangeStatusAsync(
            organizationId,
            request,
            mediator,
            mapper,
            tenant,
            context,
            cancellationToken,
            archive: false
        );

    private static Task<IResult> ArchiveAsync(
        Guid organizationId,
        ChangeOrganizationLegalProfileStatusRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken cancellationToken
    ) =>
        ChangeStatusAsync(
            organizationId,
            request,
            mediator,
            mapper,
            tenant,
            context,
            cancellationToken,
            archive: true
        );

    private static async Task<IResult> ChangeStatusAsync(
        Guid rawOrganizationId,
        ChangeOrganizationLegalProfileStatusRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken cancellationToken,
        bool archive
    )
    {
        if (
            !TryOrganization(
                rawOrganizationId,
                tenant,
                context,
                out OrganizationId organization,
                out IResult? failure
            )
        )
            return failure!;

        var model = new ChangeOrganizationLegalProfileStatusApiModel(
            organization,
            request.ExpectedRevision
        );

        Result result = archive
            ? await mediator.Send(
                mapper.Map<
                    ChangeOrganizationLegalProfileStatusApiModel,
                    ArchiveOrganizationLegalProfileCommand
                >(model),
                cancellationToken
            )
            : await mediator.Send(
                mapper.Map<
                    ChangeOrganizationLegalProfileStatusApiModel,
                    ActivateOrganizationLegalProfileCommand
                >(model),
                cancellationToken
            );

        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(context);
    }

    private static bool TryOrganization(
        Guid rawOrganizationId,
        ICurrentTenant tenant,
        HttpContext context,
        out OrganizationId organizationId,
        out IResult? failure
    )
    {
        organizationId = new OrganizationId(rawOrganizationId);
        failure = null;

        if (rawOrganizationId == Guid.Empty)
        {
            failure = Error
                .Validation(
                    "OrganizationLegalProfiles.Identifier.Invalid",
                    "errors.organizationLegalProfile.identifier.invalid",
                    new Dictionary<string, object?> { ["parameter"] = "organizationId" }
                )
                .ToHttpResult(context);
            return false;
        }

        if (
            !tenant.HasTenant
            || tenant.OrganizationId is null
            || tenant.OrganizationId.Value != organizationId
        )
        {
            failure = Results.Forbid();
            return false;
        }

        return true;
    }
}
