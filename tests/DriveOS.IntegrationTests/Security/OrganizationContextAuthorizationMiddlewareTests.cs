using System.Security.Claims;
using DriveOS.Api.Security.Authentication;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.AspNetCore.Http;

namespace DriveOS.IntegrationTests.Security;

public sealed class OrganizationContextAuthorizationMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenRouteTargetsAnotherTenant_ShouldReturnForbidden()
    {
        Guid currentOrganizationId = Guid.NewGuid();
        DefaultHttpContext context = CreateContext(currentOrganizationId);
        context.Request.RouteValues["organizationId"] = Guid.NewGuid().ToString("D");
        bool nextCalled = false;
        var middleware = new OrganizationContextAuthorizationMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            }
        );

        await middleware.InvokeAsync(context, new FakeCurrentUser([]));

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderAndRouteMatchClaim_ShouldResolveContext()
    {
        Guid currentOrganizationId = Guid.NewGuid();
        DefaultHttpContext context = CreateContext(currentOrganizationId);
        context.Request.Headers["X-Organization-Id"] = currentOrganizationId.ToString("D");
        context.Request.RouteValues["organizationId"] = currentOrganizationId.ToString("D");
        bool nextCalled = false;
        var middleware = new OrganizationContextAuthorizationMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            }
        );

        await middleware.InvokeAsync(context, new FakeCurrentUser([]));

        Assert.True(nextCalled);
        Assert.Equal(
            currentOrganizationId,
            context.Items[OrganizationContextAuthorizationMiddleware.ResolvedOrganizationIdItem]
        );
    }

    [Fact]
    public async Task InvokeAsync_WithCrossTenantPermission_ShouldAllowSelectedTenant()
    {
        DefaultHttpContext context = CreateContext(Guid.NewGuid());
        Guid selectedOrganizationId = Guid.NewGuid();
        context.Request.Headers["X-Organization-Id"] = selectedOrganizationId.ToString("D");
        context.Request.RouteValues["organizationId"] = selectedOrganizationId.ToString("D");
        bool nextCalled = false;
        var middleware = new OrganizationContextAuthorizationMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            }
        );

        await middleware.InvokeAsync(
            context,
            new FakeCurrentUser([DriveOsPermissionCodes.Organizations.CrossTenantManage])
        );

        Assert.True(nextCalled);
    }

    private static DefaultHttpContext CreateContext(Guid organizationId)
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString("D")),
                    new Claim(DriveOsClaimTypes.OrganizationId, organizationId.ToString("D")),
                ],
                authenticationType: "Test"
            )
        );
        context.Response.Body = new MemoryStream();
        return context;
    }

    private sealed class FakeCurrentUser(IEnumerable<string> permissions) : ICurrentUser
    {
        private readonly IReadOnlySet<string> values = permissions.ToHashSet(StringComparer.Ordinal);
        public bool IsAuthenticated => true;
        public UserId? UserId => new(Guid.NewGuid());
        public string? Email => "security-test@driveos.test";
        public IReadOnlySet<string> Permissions => values;
        public bool HasPermission(string permission) => values.Contains(permission);
    }
}
