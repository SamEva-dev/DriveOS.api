using DomainRelay.Validation;
using DriveOS.Api;
using DriveOS.Api.Endpoints.Organizations;
using DriveOS.Api.Errors;
using DriveOS.Api.Infrastructure.Logging;
using DriveOS.Modules.Organizations.Application;
using DriveOS.Modules.Organizations.Infrastructure;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information(
        "Starting {Application}",
        LoggingConstants.ApplicationName);
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog(
        (services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(
                    builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty(
                    LoggingConstants
                        .ApplicationNameProperty,
                    LoggingConstants.ApplicationName);
        });

    builder.Services.AddOpenApi(
        "v1",
        options =>
        {
            options.AddDocumentTransformer(
                (document, context, cancellationToken) =>
                {
                    document.Info.Title =
                        "DriveOS API";

                    document.Info.Version =
                        "v1";

                    document.Info.Description =
                        "API SaaS internationale de gestion " +
                        "des auto-écoles, enseignants, " +
                        "élèves, véhicules, formations, " +
                        "paiements et conformité.";

                    return Task.CompletedTask;
                });
        });


  
    //builder.Services.AddValidatorsFromAssembly(
    //    typeof(CreateOrganizationCommandValidator).Assembly);


    builder.Services
        .AddApiServices()
    .AddOrganizationsApplication()
    .AddOrganizationsInfrastructure(
        builder.Configuration);

    builder.Services.AddDomainRelayValidation();
    builder.Services.AddExceptionHandler<
        ValidationExceptionHandler>();

    builder.Services.AddProblemDetails();

    string[] allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? [];

    builder.Services.AddCors(
        options =>
        {
            options.AddPolicy(
                "DriveOsWeb",
                policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

    var app = builder.Build();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging(
        options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} " +
                "responded {StatusCode} in " +
                "{Elapsed:0.0000} ms";

            options.GetLevel =
                (httpContext, elapsed, exception) =>
                {
                    if (exception is not null)
                    {
                        return LogEventLevel.Error;
                    }

                    return httpContext.Response.StatusCode
                        switch
                    {
                        >= 500 =>
                            LogEventLevel.Error,

                        >= 400 =>
                            LogEventLevel.Warning,

                        _ =>
                            LogEventLevel.Information
                    };
                };

            options.EnrichDiagnosticContext =
                (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set(
                        "RequestHost",
                        httpContext.Request.Host.Value);

                    diagnosticContext.Set(
                        "RequestScheme",
                        httpContext.Request.Scheme);

                    diagnosticContext.Set(
                        "ClientIp",
                        httpContext.Connection
                            .RemoteIpAddress?
                            .ToString());

                    diagnosticContext.Set(
                        "UserAgent",
                        httpContext.Request.Headers
                            .UserAgent
                            .ToString());

                    diagnosticContext.Set(
                        "TraceIdentifier",
                        httpContext.TraceIdentifier);
                };
        });
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi("/openapi/{documentName}.json");

        app.UseSwaggerUI(
            options =>
            {
                options.SwaggerEndpoint(
                    "/openapi/v1.json",
                    "DriveOS API v1");

                options.RoutePrefix =
                    "swagger";

                options.DocumentTitle =
                    "DriveOS API Documentation";

                options.DisplayRequestDuration();

                options.EnableTryItOutByDefault();

                options.EnablePersistAuthorization();

                options.DocExpansion(
                    Swashbuckle.AspNetCore
                        .SwaggerUI.DocExpansion.List);
            });
    }

    app.UseHttpsRedirection();
    app.UseCors("DriveOsWeb");

    app.MapOrganizationEndpoints();

    app.MapGet(
        "/health",
        () => Results.Ok(
            new
            {
                status = "Healthy",
                service = "DriveOS.Api"

            }));
    Log.Information(
        "{Application} started successfully",
        LoggingConstants.ApplicationName);

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(
        exception,
        "{Application} terminated unexpectedly",
        LoggingConstants.ApplicationName);
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;