using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Organization.OrganizationSettings;

internal static class OrganizationSettingsEndpointErrors
{
    public static readonly Error InvalidOrganizationId = Error.Validation(
        "OrganizationSettings.OrganizationId.Invalid",
        "errors.organizationSettings.organizationId.invalid");

    public static readonly Error TenantScopeMismatch = Error.Forbidden(
        "OrganizationSettings.TenantScopeMismatch",
        "errors.organizationSettings.tenantScopeMismatch");
}
