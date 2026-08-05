using System.Net.Http.Json;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationRepresentatives.AccessSynchronization;

internal sealed class AuthGateOrganizationRepresentativeAccessSynchronizer(
    HttpClient httpClient,
    IOptions<AuthGateRepresentativeAccessOptions> options,
    ILogger<AuthGateOrganizationRepresentativeAccessSynchronizer> logger)
    : IOrganizationRepresentativeAccessSynchronizer
{
    private readonly AuthGateRepresentativeAccessOptions _options = options.Value;

    public async Task SynchronizeAsync(OrganizationRepresentativeAccessSnapshot representative, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            logger.LogDebug("AuthGate representative synchronization is disabled for {RepresentativeId}.", representative.RepresentativeId.Value);
            return;
        }

        using HttpRequestMessage request = new(HttpMethod.Put, _options.SynchronizePath)
        {
            Content = JsonContent.Create(representative)
        };
        request.Headers.TryAddWithoutValidation("Idempotency-Key", $"org-representative:{representative.RepresentativeId.Value}:r{representative.Revision}");

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RevokeAsync(OrganizationRepresentativeAccessSnapshot representative, string reason, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return;

        var payload = new { Representative = representative, Reason = reason };
        using HttpRequestMessage request = new(HttpMethod.Post, _options.RevokePath)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.TryAddWithoutValidation("Idempotency-Key", $"org-representative-revoke:{representative.RepresentativeId.Value}:r{representative.Revision}");

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
