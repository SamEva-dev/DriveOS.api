using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace DriveOS.Api.Security.Authentication;

internal sealed class JwksOnlyConfigurationRetriever
    : IConfigurationRetriever<OpenIdConnectConfiguration>
{
    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
        string address,
        IDocumentRetriever retriever,
        CancellationToken cancel
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentNullException.ThrowIfNull(retriever);

        string document = await retriever.GetDocumentAsync(address, cancel);

        var jsonWebKeySet = new JsonWebKeySet(document);
        var configuration = new OpenIdConnectConfiguration();

        foreach (SecurityKey signingKey in jsonWebKeySet.GetSigningKeys())
        {
            configuration.SigningKeys.Add(signingKey);
        }

        return configuration;
    }
}
