using System.Security.Claims;
using DriveOS.Api.Security.Authorization;
using DriveOS.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace DriveOS.Api.Security.Authentication;

internal static class DriveOsAuthenticationExtensions
{
    public static IServiceCollection AddDriveOsAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<AuthGateJwtOptions>()
            .Bind(configuration.GetSection(AuthGateJwtOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Issuer),
                "Authentication:AuthGate:Issuer is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Audience),
                "Authentication:AuthGate:Audience is required."
            )
            .Validate(
                options => Uri.TryCreate(options.JwksUrl, UriKind.Absolute, out _),
                "Authentication:AuthGate:JwksUrl must be an absolute URL."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.RequiredClientId),
                "Authentication:AuthGate:RequiredClientId is required."
            )
            .Validate(
                options => options.ClockSkewSeconds is >= 0 and <= 300,
                "Authentication:AuthGate:ClockSkewSeconds must be between 0 and 300."
            )
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddScoped<ICurrentTenant, HttpContextCurrentTenant>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                AuthGateJwtOptions settings =
                    configuration
                        .GetSection(AuthGateJwtOptions.SectionName)
                        .Get<AuthGateJwtOptions>()
                    ?? throw new InvalidOperationException(
                        "The AuthGate JWT configuration is missing."
                    );

                var documentRetriever = new HttpDocumentRetriever
                {
                    RequireHttps = settings.RequireHttpsMetadata,
                };

                options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    settings.JwksUrl,
                    new JwksOnlyConfigurationRetriever(),
                    documentRetriever
                );

                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromSeconds(settings.ClockSkewSeconds),
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role,
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        string? clientId =
                            context.Principal?.FindFirstValue(DriveOsClaimTypes.ClientId)
                            ?? context.Principal?.FindFirstValue("app");

                        if (
                            !string.Equals(
                                clientId,
                                settings.RequiredClientId,
                                StringComparison.Ordinal
                            )
                        )
                        {
                            context.Fail("The access token was not issued for DriveOS.Web.");
                            return;
                        }

                        var bearer = context.Request.Headers.Authorization.FirstOrDefault();
                        var accessToken = !string.IsNullOrWhiteSpace(bearer) && bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                            ? bearer[7..].Trim()
                            : string.Empty;
                        var sessionValidator = context.HttpContext.RequestServices.GetRequiredService<AuthGateSessionValidator>();
                        if (!await sessionValidator.IsActiveAsync(accessToken, context.HttpContext.RequestAborted))
                            context.Fail("AuthGate session is no longer active.");
                    },
                };
            });

        services.AddDriveOsAuthorization();

        return services;
    }
}
