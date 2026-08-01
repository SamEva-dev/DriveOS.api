using Microsoft.AspNetCore.Authorization;

namespace DriveOS.Api.Security.Authorization;

internal sealed record PermissionRequirement(string Permission)
    : IAuthorizationRequirement;
