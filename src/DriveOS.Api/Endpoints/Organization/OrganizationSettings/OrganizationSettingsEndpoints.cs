using DomainRelay.Abstractions;
using DomainRelay.Mapping.Abstractions.Services;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.CreateOrganizationSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.GetOrganizationSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.Models;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateAddress;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateContact;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateOperationalSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateProfile;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateRegionalSettings;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Organization.OrganizationSettings;

public static class OrganizationSettingsEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationSettingsEndpoints(
        this IEndpointRouteBuilder endpoints
    )
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/organizations/{organizationId:guid}/settings")
            .WithTags("Organization settings");

        group
            .MapGet("/", GetAsync)
            .WithName("GetOrganizationSettings")
            .Produces<OrganizationSettingsResponseContract>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSettings.Read);

        group
            .MapPost("/", CreateAsync)
            .WithName("CreateOrganizationSettings")
            .Accepts<CreateOrganizationSettingsRequest>("application/json")
            .Produces(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSettings.Create);

        group
            .MapPut("/profile", UpdateProfileAsync)
            .WithName("UpdateOrganizationSettingsProfile")
            .Accepts<UpdateOrganizationProfileRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSettings.Update);

        group
            .MapPut("/contact", UpdateContactAsync)
            .WithName("UpdateOrganizationSettingsContact")
            .Accepts<UpdateOrganizationContactRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSettings.Update);

        group
            .MapPut("/address", UpdateAddressAsync)
            .WithName("UpdateOrganizationSettingsAddress")
            .Accepts<UpdateOrganizationAddressRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSettings.Update);

        group
            .MapPut("/regional", UpdateRegionalAsync)
            .WithName("UpdateOrganizationRegionalSettings")
            .Accepts<UpdateOrganizationRegionalSettingsRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSettings.Update);

        group
            .MapPut("/operational", UpdateOperationalAsync)
            .WithName("UpdateOrganizationOperationalSettings")
            .Accepts<UpdateOrganizationOperationalSettingsRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSettings.Update);

        return endpoints;
    }

    private static async Task<IResult> GetAsync(
        Guid organizationId,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (!TryGetOrganizationId(organizationId, out OrganizationId id, out Error? error))
            return error!.ToHttpResult(httpContext);

        IResult? scopeError = EnsureTenantScope(id, currentTenant, httpContext);
        if (scopeError is not null)
            return scopeError;

        Result<OrganizationSettingsResponse> result = await mediator.Send(
            new GetOrganizationSettingsQuery(id),
            cancellationToken
        );

        if (result.IsFailure)
            return result.Error.ToHttpResult(httpContext);

        OrganizationSettingsResponseContract response = mapper.Map<
            OrganizationSettingsResponse,
            OrganizationSettingsResponseContract
        >(result.Value);

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateAsync(
        Guid organizationId,
        CreateOrganizationSettingsRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (!TryGetOrganizationId(organizationId, out OrganizationId id, out Error? error))
            return error!.ToHttpResult(httpContext);

        IResult? scopeError = EnsureTenantScope(id, currentTenant, httpContext);
        if (scopeError is not null)
            return scopeError;

        var model = new CreateOrganizationSettingsApiModel(
            id,
            request.TradeName,
            request.RegistrationNumber,
            request.TaxNumber,
            request.Email,
            request.Phone,
            request.Website,
            request.AddressLine1,
            request.AddressLine2,
            request.PostalCode,
            request.City,
            request.Region,
            request.AddressCountryCode,
            request.DefaultLanguage,
            request.SupportedLanguages,
            request.TimeZoneId,
            request.CurrencyCode,
            request.DateFormat,
            request.TimeFormat,
            request.FirstDayOfWeek,
            request.MeasurementSystem,
            request.DefaultSessionDurationMinutes,
            request.DefaultBookingLeadTimeMinutes,
            request.DefaultCancellationDelayHours,
            request.AllowStudentSelfBooking,
            request.RequireBranchForOperations,
            request.DefaultBranchId is Guid branchId ? new BranchId(branchId) : null
        );

        CreateOrganizationSettingsCommand command = mapper.Map<
            CreateOrganizationSettingsApiModel,
            CreateOrganizationSettingsCommand
        >(model);

        Result<OrganizationSettingsId> result = await mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToHttpResult(httpContext);

        return Results.Created(
            $"/api/organizations/{organizationId}/settings",
            new { id = result.Value.Value }
        );
    }

    private static async Task<IResult> UpdateProfileAsync(
        Guid organizationId,
        UpdateOrganizationProfileRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryGetScopedOrganizationId(
                organizationId,
                currentTenant,
                httpContext,
                out OrganizationId id,
                out IResult? failure
            )
        )
            return failure!;

        var model = new UpdateOrganizationProfileApiModel(
            id,
            request.TradeName,
            request.RegistrationNumber,
            request.TaxNumber,
            request.ExpectedVersion
        );

        UpdateOrganizationProfileCommand command = mapper.Map<
            UpdateOrganizationProfileApiModel,
            UpdateOrganizationProfileCommand
        >(model);

        Result result = await mediator.Send(command, cancellationToken);
        return ToUpdateResult(result, httpContext);
    }

    private static async Task<IResult> UpdateContactAsync(
        Guid organizationId,
        UpdateOrganizationContactRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryGetScopedOrganizationId(
                organizationId,
                currentTenant,
                httpContext,
                out OrganizationId id,
                out IResult? failure
            )
        )
            return failure!;

        var model = new UpdateOrganizationContactApiModel(
            id,
            request.Email,
            request.Phone,
            request.Website,
            request.ExpectedVersion
        );

        UpdateOrganizationContactCommand command = mapper.Map<
            UpdateOrganizationContactApiModel,
            UpdateOrganizationContactCommand
        >(model);

        Result result = await mediator.Send(command, cancellationToken);
        return ToUpdateResult(result, httpContext);
    }

    private static async Task<IResult> UpdateAddressAsync(
        Guid organizationId,
        UpdateOrganizationAddressRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryGetScopedOrganizationId(
                organizationId,
                currentTenant,
                httpContext,
                out OrganizationId id,
                out IResult? failure
            )
        )
            return failure!;

        var model = new UpdateOrganizationAddressApiModel(
            id,
            request.AddressLine1,
            request.AddressLine2,
            request.PostalCode,
            request.City,
            request.Region,
            request.AddressCountryCode,
            request.ExpectedVersion
        );

        UpdateOrganizationAddressCommand command = mapper.Map<
            UpdateOrganizationAddressApiModel,
            UpdateOrganizationAddressCommand
        >(model);

        Result result = await mediator.Send(command, cancellationToken);
        return ToUpdateResult(result, httpContext);
    }

    private static async Task<IResult> UpdateRegionalAsync(
        Guid organizationId,
        UpdateOrganizationRegionalSettingsRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryGetScopedOrganizationId(
                organizationId,
                currentTenant,
                httpContext,
                out OrganizationId id,
                out IResult? failure
            )
        )
            return failure!;

        var model = new UpdateOrganizationRegionalSettingsApiModel(
            id,
            request.DefaultLanguage,
            request.SupportedLanguages,
            request.TimeZoneId,
            request.CurrencyCode,
            request.DateFormat,
            request.TimeFormat,
            request.FirstDayOfWeek,
            request.MeasurementSystem,
            request.ExpectedVersion
        );

        UpdateOrganizationRegionalSettingsCommand command = mapper.Map<
            UpdateOrganizationRegionalSettingsApiModel,
            UpdateOrganizationRegionalSettingsCommand
        >(model);

        Result result = await mediator.Send(command, cancellationToken);
        return ToUpdateResult(result, httpContext);
    }

    private static async Task<IResult> UpdateOperationalAsync(
        Guid organizationId,
        UpdateOrganizationOperationalSettingsRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryGetScopedOrganizationId(
                organizationId,
                currentTenant,
                httpContext,
                out OrganizationId id,
                out IResult? failure
            )
        )
            return failure!;

        var model = new UpdateOrganizationOperationalSettingsApiModel(
            id,
            request.DefaultSessionDurationMinutes,
            request.DefaultBookingLeadTimeMinutes,
            request.DefaultCancellationDelayHours,
            request.AllowStudentSelfBooking,
            request.RequireBranchForOperations,
            request.DefaultBranchId is Guid branchId ? new BranchId(branchId) : null,
            request.ExpectedVersion
        );

        UpdateOrganizationOperationalSettingsCommand command = mapper.Map<
            UpdateOrganizationOperationalSettingsApiModel,
            UpdateOrganizationOperationalSettingsCommand
        >(model);

        Result result = await mediator.Send(command, cancellationToken);
        return ToUpdateResult(result, httpContext);
    }

    private static bool TryGetScopedOrganizationId(
        Guid rawOrganizationId,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        out OrganizationId organizationId,
        out IResult? failure
    )
    {
        if (!TryGetOrganizationId(rawOrganizationId, out organizationId, out Error? error))
        {
            failure = error!.ToHttpResult(httpContext);
            return false;
        }

        failure = EnsureTenantScope(organizationId, currentTenant, httpContext);

        return failure is null;
    }

    private static IResult ToUpdateResult(Result result, HttpContext httpContext) =>
        result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(httpContext);

    private static bool TryGetOrganizationId(
        Guid value,
        out OrganizationId organizationId,
        out Error? error
    )
    {
        organizationId = new OrganizationId(value);
        error = null;

        if (value != Guid.Empty)
            return true;

        error = OrganizationSettingsEndpointErrors.InvalidOrganizationId;
        return false;
    }

    private static IResult? EnsureTenantScope(
        OrganizationId requestedOrganizationId,
        ICurrentTenant currentTenant,
        HttpContext httpContext
    )
    {
        if (!currentTenant.HasTenant)
            return null;

        return currentTenant.OrganizationId == requestedOrganizationId
            ? null
            : OrganizationSettingsEndpointErrors.TenantScopeMismatch.ToHttpResult(httpContext);
    }
}
