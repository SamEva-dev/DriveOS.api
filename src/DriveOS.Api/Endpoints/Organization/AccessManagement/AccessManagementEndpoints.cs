using DomainRelay.Abstractions;
using DriveOS.Api.Security;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Organizations.Application.Branches.GetBranches;
using DriveOS.Modules.Organizations.Application.Branches.Models;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizations;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizationById;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.GetOrganizationSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.Models;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateAddress;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateContact;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateProfile;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Organization.AccessManagement;

public static class AccessManagementEndpoints
{
    public static IEndpointRouteBuilder MapAccessManagementEndpoints(
        this IEndpointRouteBuilder endpoints
    )
    {
        var group = endpoints
            .MapGroup("/api/access-management")
            .WithTags("Access Management")
            .AddEndpointFilter<AccessManagerMachineTokenEndpointFilter>();

        group.MapGet("/organizations", GetOrganizationsAsync);
        group.MapGet("/organizations/{organizationId:guid}/administration", GetOrganizationAdministrationAsync);
        group.MapPut("/organizations/{organizationId:guid}/administration", UpdateOrganizationAdministrationAsync);
        group.MapGet("/organizations/{organizationId:guid}/branches", GetBranchesAsync);
        return endpoints;
    }

    private static async Task<IResult> GetOrganizationsAsync(
        IMediator mediator,
        CancellationToken ct
    )
    {
        var query = new GetOrganizationsQuery(
            1,
            PaginationParameters.MaximumPageSize,
            null,
            OrganizationSortField.LegalName,
            SortDirection.Ascending
        );
        Result<PagedResult<OrganizationListItem>> result = await mediator.Send(query, ct);
        if (result.IsFailure)
            return Results.BadRequest(
                new { code = result.Error.Code, message = result.Error.MessageKey }
            );

        return Results.Ok(
            result.Value.Items.Select(x => new
            {
                id = x.Id.ToString("D"),
                name = x.LegalName,
                code = x.CountryCode,
                type = x.Type,
                usersCount = 0,
            })
        );
    }

    private static async Task<IResult> GetOrganizationAdministrationAsync(
        Guid organizationId,
        IMediator mediator,
        CancellationToken ct
    )
    {
        if (organizationId == Guid.Empty)
            return Results.BadRequest(new { code = "organization_id_invalid" });

        Result<OrganizationResponse> organizationResult = await mediator.Send(
            new GetOrganizationByIdQuery(new OrganizationId(organizationId)),
            ct
        );

        if (organizationResult.IsFailure)
            return Results.NotFound(new
            {
                code = organizationResult.Error.Code,
                message = organizationResult.Error.MessageKey
            });

        OrganizationSettingsResponse? settings = null;
        Result<OrganizationSettingsResponse> settingsResult = await mediator.Send(
            new GetOrganizationSettingsQuery(new OrganizationId(organizationId)),
            ct
        );

        if (!settingsResult.IsFailure)
            settings = settingsResult.Value;

        return Results.Ok(ToAdministrationResponse(organizationResult.Value, settings));
    }

    private static async Task<IResult> UpdateOrganizationAdministrationAsync(
        Guid organizationId,
        OrganizationAdministrationUpdateRequest request,
        IMediator mediator,
        CancellationToken ct
    )
    {
        if (organizationId == Guid.Empty)
            return Results.BadRequest(new { code = "organization_id_invalid" });

        // IAM-CTX-012: LegalName and Organization lifecycle are separate DriveOS use cases.
        // Do not silently map them onto OrganizationSettings.
        if (request.Name is not null)
            return Results.UnprocessableEntity(new
            {
                code = "organization_field_not_supported",
                message = "DriveOS legal name is read-only through the common administration contract."
            });

        if (request.Status is not null)
            return Results.UnprocessableEntity(new
            {
                code = "organization_field_not_supported",
                message = "DriveOS organization lifecycle is not modified through OrganizationSettings."
            });

        Result<OrganizationResponse> organizationResult = await mediator.Send(
            new GetOrganizationByIdQuery(new OrganizationId(organizationId)),
            ct
        );

        if (organizationResult.IsFailure)
            return Results.NotFound(new
            {
                code = organizationResult.Error.Code,
                message = organizationResult.Error.MessageKey
            });

        Result<OrganizationSettingsResponse> settingsResult = await mediator.Send(
            new GetOrganizationSettingsQuery(new OrganizationId(organizationId)),
            ct
        );

        if (settingsResult.IsFailure)
            return Results.UnprocessableEntity(new
            {
                code = "organization_settings_required",
                message = "DriveOS OrganizationSettings must exist before they can be administered."
            });

        OrganizationSettingsResponse current = settingsResult.Value;

        if (request.ExpectedVersion is not int expectedVersion)
            return Results.BadRequest(new
            {
                code = "organization_version_required",
                message = "expectedVersion is required for DriveOS organization updates."
            });

        if (current.Version != expectedVersion)
            return Results.Conflict(new
            {
                code = "organization_concurrent_update",
                message = "The organization settings were modified by another operation. Reload and retry."
            });

        if (request.TradeName is not null || request.RegistrationNumber is not null || request.TaxNumber is not null)
        {
            Result result = await mediator.Send(
                new UpdateOrganizationProfileCommand(
                    new OrganizationId(organizationId),
                    request.TradeName ?? current.TradeName,
                    request.RegistrationNumber ?? current.RegistrationNumber,
                    request.TaxNumber ?? current.TaxNumber,
                    current.Version),
                ct);

            if (result.IsFailure)
                return ToMutationError(result.Error);

            current = await ReloadSettingsAsync(organizationId, mediator, ct);
        }

        if (request.Email is not null || request.Phone is not null || request.Website is not null)
        {
            Result result = await mediator.Send(
                new UpdateOrganizationContactCommand(
                    new OrganizationId(organizationId),
                    request.Email ?? current.Email,
                    request.Phone ?? current.Phone,
                    request.Website ?? current.Website,
                    current.Version),
                ct);

            if (result.IsFailure)
                return ToMutationError(result.Error);

            current = await ReloadSettingsAsync(organizationId, mediator, ct);
        }

        if (request.AddressLine1 is not null || request.AddressLine2 is not null ||
            request.PostalCode is not null || request.City is not null ||
            request.Region is not null || request.CountryCode is not null)
        {
            Result result = await mediator.Send(
                new UpdateOrganizationAddressCommand(
                    new OrganizationId(organizationId),
                    request.AddressLine1 ?? current.AddressLine1,
                    request.AddressLine2 ?? current.AddressLine2,
                    request.PostalCode ?? current.PostalCode,
                    request.City ?? current.City,
                    request.Region ?? current.Region,
                    request.CountryCode ?? current.AddressCountryCode,
                    current.Version),
                ct);

            if (result.IsFailure)
                return ToMutationError(result.Error);

            current = await ReloadSettingsAsync(organizationId, mediator, ct);
        }

        return Results.Ok(ToAdministrationResponse(organizationResult.Value, current));
    }

    private static async Task<OrganizationSettingsResponse> ReloadSettingsAsync(
        Guid organizationId,
        IMediator mediator,
        CancellationToken ct)
    {
        Result<OrganizationSettingsResponse> result = await mediator.Send(
            new GetOrganizationSettingsQuery(new OrganizationId(organizationId)),
            ct);

        if (result.IsFailure)
            throw new InvalidOperationException(
                $"Organization settings disappeared during update. OrganizationId={organizationId:D}");

        return result.Value;
    }

    private static IResult ToMutationError(Error error) => error.Type switch
    {
        ErrorType.NotFound => Results.NotFound(new { code = error.Code, message = error.MessageKey }),
        ErrorType.Conflict => Results.Conflict(new { code = error.Code, message = error.MessageKey }),
        ErrorType.Validation => Results.BadRequest(new { code = error.Code, message = error.MessageKey }),
        _ => Results.BadRequest(new { code = error.Code, message = error.MessageKey })
    };

    private static OrganizationAdministrationResponse ToAdministrationResponse(
        OrganizationResponse organization,
        OrganizationSettingsResponse? settings)
        => new(
            organization.Id,
            organization.LegalName,
            organization.CountryCode,
            organization.Type,
            organization.Status,
            new OrganizationAdministrationProfileResponse(
                settings?.TradeName,
                settings?.RegistrationNumber,
                settings?.TaxNumber),
            new OrganizationAdministrationContactResponse(
                settings?.Email,
                settings?.Phone,
                settings?.Website),
            settings is null
                ? null
                : new OrganizationAdministrationAddressResponse(
                    settings.AddressLine1,
                    settings.AddressLine2,
                    settings.PostalCode,
                    settings.City,
                    settings.Region,
                    settings.AddressCountryCode),
            settings?.Version);

    private static async Task<IResult> GetBranchesAsync(
        Guid organizationId,
        IMediator mediator,
        CancellationToken ct
    )
    {
        if (organizationId == Guid.Empty)
            return Results.BadRequest();
        var query = new GetBranchesQuery(
            new OrganizationId(organizationId),
            1,
            PaginationParameters.MaximumPageSize,
            null,
            BranchSortField.Name,
            SortDirection.Ascending
        );

        Result<PagedResult<BranchListItem>> result = await mediator.Send(query, ct);
        if (result.IsFailure)
            return Results.NotFound();

        return Results.Ok(
            result.Value.Items.Select(x => new
            {
                id = x.Id,
                name = x.Name,
                code = x.Code,
                status = x.Status,
                isPrimary = x.IsPrimary,
            })
        );
    }

    public sealed record OrganizationAdministrationUpdateRequest(
        string? Name = null,
        string? Status = null,
        string? TradeName = null,
        string? RegistrationNumber = null,
        string? TaxNumber = null,
        string? Email = null,
        string? Phone = null,
        string? Website = null,
        string? AddressLine1 = null,
        string? AddressLine2 = null,
        string? PostalCode = null,
        string? City = null,
        string? Region = null,
        string? CountryCode = null,
        int? ExpectedVersion = null);

    public sealed record OrganizationAdministrationResponse(
        Guid OrganizationId,
        string Name,
        string? Code,
        string? Type,
        string? Status,
        OrganizationAdministrationProfileResponse Profile,
        OrganizationAdministrationContactResponse Contact,
        OrganizationAdministrationAddressResponse? Address,
        int? Version);

    public sealed record OrganizationAdministrationProfileResponse(
        string? TradeName,
        string? RegistrationNumber,
        string? TaxNumber);

    public sealed record OrganizationAdministrationContactResponse(
        string? Email,
        string? Phone,
        string? Website);

    public sealed record OrganizationAdministrationAddressResponse(
        string? AddressLine1,
        string? AddressLine2,
        string? PostalCode,
        string? City,
        string? Region,
        string? CountryCode);

}
