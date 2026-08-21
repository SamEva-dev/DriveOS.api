using DriveOS.Modules.TrainingDelivery.Application;
using DriveOS.Modules.TrainingDelivery.Infrastructure;
using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.Modules.TrainingDelivery.Application.GroupSessions;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using DriveOS.Modules.TrainingDelivery.Application.CancellationConsequences;
using DriveOS.Api.Integrations.TrainingDelivery;
using DriveOS.Api.Endpoints.TrainingDelivery;
using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.Replacements;
using DriveOS.Modules.SchedulingCapacity.Application.Travel;
using DriveOS.Modules.SchedulingCapacity.Application.SlotSearch;
using DomainRelay.Validation;
using DriveOS.Api;
using DriveOS.Api.Configuration;
using DriveOS.Api.Endpoints.Crm;
using DriveOS.Api.Endpoints.Contracts;
using DriveOS.Api.Endpoints.CurriculumPedagogy;
using DriveOS.Api.Endpoints.FundingBilling;
using DriveOS.Api.Endpoints.Organization.AccessManagement;
using DriveOS.Api.Endpoints.Organization.BranchAssignments;
using DriveOS.Api.Endpoints.Organization.BranchConfigurationOverrides;
using DriveOS.Api.Endpoints.Organization.Branches;
using DriveOS.Api.Endpoints.Organization.Networks;
using DriveOS.Api.Endpoints.Organization.OrganizationConfigurations;
using DriveOS.Api.Endpoints.Organization.OrganizationLegalProfiles;
using DriveOS.Api.Endpoints.Organization.OrganizationRepresentatives;
using DriveOS.Api.Endpoints.Organization.Organizations;
using DriveOS.Api.Endpoints.Organization.OrganizationSequences;
using DriveOS.Api.Endpoints.Organization.OrganizationSettings;
using DriveOS.Api.Endpoints.Organization.OrganizationSubscriptions;
using DriveOS.Api.Endpoints.Provisioning;
using DriveOS.Api.Endpoints.Students;
using DriveOS.Api.Errors;
using DriveOS.Api.Infrastructure.Logging;
using DriveOS.Api.Integrations.Students;
using DriveOS.Api.Integrations.SchedulingCapacity;
using DriveOS.Api.Integrations.Contracts;
using DriveOS.Api.Integrations.FundingBilling;
using DriveOS.Api.Integrations.CurriculumPedagogy;
using DriveOS.Modules.CRM.Application;
using DriveOS.Modules.CRM.Application.Leads.ConvertLead;
using DriveOS.Modules.CRM.Infrastructure;
using DriveOS.Modules.Contracts.Application;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Create;
using DriveOS.Modules.Contracts.Infrastructure;
using DriveOS.Modules.CurriculumPedagogy.Application;
using DriveOS.Modules.CurriculumPedagogy.Infrastructure;
using DriveOS.Modules.SchedulingCapacity.Application;
using DriveOS.Modules.SchedulingCapacity.Infrastructure;
using DriveOS.Api.Endpoints.SchedulingCapacity;
using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.Modules.FundingBilling.Application;
using DriveOS.Modules.FundingBilling.Application.BillingAccounts.Create;
using DriveOS.Modules.FundingBilling.Application.Invoices.Issue;
using DriveOS.Modules.FundingBilling.Application.CreditNotes.Issue;
using DriveOS.Modules.FundingBilling.Infrastructure;
using DriveOS.Modules.Organizations.Application;
using DriveOS.Modules.Organizations.Infrastructure;
using DriveOS.Modules.Students.Application;
using DriveOS.Modules.Students.Infrastructure;
using DriveOS.Api.Integrations.FundingBilling.Notifications;
using DriveOS.Api.Integrations.CurriculumPedagogy.Notifications;
using DriveOS.Modules.CurriculumPedagogy.Application.Notifications;
using DriveOS.Modules.FundingBilling.Application.Notifications;
using Itech.Emailing.Registration;
using Itech.Emailing.Webhooks;
using Itech.Emailing.Workers;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting {Application}", LoggingConstants.ApplicationName);
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSingleton(TimeProvider.System);

    builder.Services.AddSerilog(
        (services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty(
                    LoggingConstants.ApplicationNameProperty,
                    LoggingConstants.ApplicationName
                );
        }
    );

    builder.Services.AddOpenApi(
        "v1",
        options =>
        {
            options.AddDocumentTransformer(
                (document, context, cancellationToken) =>
                {
                    document.Info.Title = "DriveOS API";

                    document.Info.Version = "v1";

                    document.Info.Description =
                        "API SaaS internationale de gestion "
                        + "des auto-écoles, enseignants, "
                        + "élèves, véhicules, formations, "
                        + "paiements et conformité.";

                    return Task.CompletedTask;
                }
            );
        }
    );

    //builder.Services.AddValidatorsFromAssembly(
    //    typeof(CreateOrganizationCommandValidator).Assembly);

    builder
        .Services.AddApiServices(builder.Configuration)
        .AddOrganizationsApplication()
        .AddOrganizationsInfrastructure(builder.Configuration)
        .AddCrmApplication()
        .AddCrmInfrastructure(builder.Configuration)
        .AddStudentsApplication()
        .AddStudentsInfrastructure(builder.Configuration)
        .AddContractsApplication()
        .AddContractsInfrastructure(builder.Configuration)
        .AddFundingBillingApplication()
        .AddFundingBillingInfrastructure(builder.Configuration)
        .AddCurriculumPedagogyApplication()
        .AddCurriculumPedagogyInfrastructure(builder.Configuration)
        .AddSchedulingCapacityApplication()
        .AddSchedulingCapacityInfrastructure(builder.Configuration)
        .AddTrainingDeliveryApplication()
        .AddTrainingDeliveryInfrastructure(builder.Configuration);

    builder.Services.AddScoped<IStudentProvisioningGateway, StudentProvisioningGateway>();
    builder.Services.AddScoped<ITrainingContractSourceGateway, TrainingContractSourceGateway>();
    builder.Services.AddScoped<IBillingAccountStudentGateway, BillingAccountStudentGateway>();
    builder.Services.AddScoped<ITrainingPathStudentGateway, TrainingPathStudentGateway>();
    builder.Services.AddScoped<IBookingReferenceValidationGateway, BookingReferenceValidationGateway>();
    builder.Services.AddScoped<IBookingCreditReservationGateway, BookingCreditReservationGateway>();
    builder.Services.AddScoped<IBookingCancellationPolicyGateway, BookingCancellationPolicyGateway>();
    builder.Services.AddScoped<IInstructorReplacementEligibilityGateway, InstructorReplacementEligibilityGateway>();
    builder.Services.AddScoped<ISlotSearchInstructorContextGateway, SlotSearchInstructorContextGateway>();
    builder.Services.AddScoped<IVehicleReplacementEligibilityGateway, VehicleReplacementEligibilityGateway>();
    builder.Services.AddScoped<ITravelRoutingGateway, TravelRoutingGateway>();
    builder.Services.AddScoped<IConfirmedBookingSessionSourceGateway, ConfirmedBookingSessionSourceGateway>();
    builder.Services.AddScoped<IConfirmedGroupBookingSourceGateway, ConfirmedGroupBookingSourceGateway>();
    builder.Services.AddScoped<ITrainingSessionExecutionReadinessGateway, TrainingSessionExecutionReadinessGateway>();
    builder.Services.AddScoped<ITrainingSessionVehicleComplianceGateway, TrainingSessionVehicleComplianceGateway>();
    builder.Services.AddScoped<ITrainingDeliveryDashboardReadService, TrainingDeliveryDashboardReadService>();
    builder.Services.AddScoped<ITrainingDeliveryPendingReportsReadService, TrainingDeliveryPendingReportsReadService>();
    builder.Services.AddScoped<ITrainingSessionPedagogyGateway, TrainingSessionPedagogyGateway>();
    builder.Services.AddScoped<ITrainingSessionCompletionConsequenceGateway, TrainingSessionCompletionConsequenceGateway>();
    builder.Services.AddScoped<ITrainingSessionCancellationConsequenceGateway, TrainingSessionCancellationConsequenceGateway>();
    builder.Services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();
    builder.Services.AddScoped<ICreditNoteNumberGenerator, CreditNoteNumberGenerator>();
    builder.Services.AddScoped<IFinancialNotificationGateway, LocaGuestFinancialNotificationGateway>();
    builder.Services.AddScoped<IPedagogicalNotificationGateway, ItechPedagogicalNotificationGateway>();

    string driveOsConnectionString = builder.Configuration.GetConnectionString("DriveOS")
        ?? throw new InvalidOperationException("The DriveOS database connection string is missing.");
    builder.Services.AddItechEmailing(builder.Configuration, emailing =>
        emailing.UsePostgres(driveOsConnectionString, typeof(LocaGuestFinancialNotificationGateway).Assembly.GetName().Name));
    builder.Services.AddHostedService<EmailDispatcherWorker>();

    builder.Services.AddDomainRelayValidation();
    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

    builder.Services.AddProblemDetails();

    string[] allowedOrigins =
        builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? throw new InvalidOperationException(
            "The CORS allowed origins configuration is missing."
        );

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.ConfigureDriveOsEnums();
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(
            "DriveOsWeb",
            policy =>
            {
                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
            }
        );
    });

    var app = builder.Build();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} "
            + "responded {StatusCode} in "
            + "{Elapsed:0.0000} ms";

        options.GetLevel = (httpContext, elapsed, exception) =>
        {
            if (exception is not null)
            {
                return LogEventLevel.Error;
            }

            return httpContext.Response.StatusCode switch
            {
                >= 500 => LogEventLevel.Error,

                >= 400 => LogEventLevel.Warning,

                _ => LogEventLevel.Information,
            };
        };

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);

            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);

            diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());

            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

            diagnosticContext.Set("TraceIdentifier", httpContext.TraceIdentifier);
        };
    });
    app.UseExceptionHandler();
    app.ApplyMigrations();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi("/openapi/{documentName}.json");

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "DriveOS API v1");

            options.RoutePrefix = "swagger";

            options.DocumentTitle = "DriveOS API Documentation";

            options.DisplayRequestDuration();

            options.EnableTryItOutByDefault();

            options.EnablePersistAuthorization();

            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("DriveOsWeb");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapBrevoTransactionalWebhook("/api/webhooks/brevo/transactional");

    app.MapOrganizationEndpoints();
    app.MapOrganizationSettingsEndpoints();
    app.MapOrganizationSubscriptionEndpoints();
    app.MapBranchEndpoints();
    app.MapBranchUserAssignmentEndpoints();
    app.MapProvisioningEndpoints();
    app.MapAccessManagementEndpoints();
    app.MapBranchConfigurationOverrideEndpoints();
    app.MapOrganizationConfigurationEndpoints();
    app.MapOrganizationSequenceEndpoints();
    app.MapOrganizationRepresentativeEndpoints();
    app.MapOrganizationLegalProfileEndpoints();
    app.MapLeadEndpoints();
    app.MapCrmTaskEndpoints();
    app.MapCrmActivityEndpoints();
    app.MapCrmDashboardEndpoints();
    app.MapNetworkMembershipEndpoints();
    app.MapAssessmentAppointmentEndpoints();
    app.MapCommercialOfferEndpoints();
    app.MapStudentDashboardEndpoints();
    app.MapTrainingContractEndpoints();
    app.MapBillingAccountEndpoints();
    app.MapInvoiceEndpoints();
    app.MapPaymentInstallmentEndpoints();
    app.MapPaymentEndpoints();
    app.MapRefundEndpoints();
    app.MapCreditNoteEndpoints();
    app.MapCollectionEndpoints();
    app.MapFundingPlanEndpoints();
    app.MapBillingPartyEndpoints();
    app.MapTrainingCreditAccountEndpoints();
    app.MapStudentFinancialOverviewEndpoints();
    app.MapFinancialAuditEndpoints();
    app.MapCurriculumPedagogyEndpoints();
    app.MapSchedulingCapacityEndpoints();
    app.MapTrainingDeliveryEndpoints();
app.MapGroupTrainingSessionEndpoints();

    app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "DriveOS.Api" }));
    Log.Information("{Application} started successfully", LoggingConstants.ApplicationName);

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "{Application} terminated unexpectedly", LoggingConstants.ApplicationName);
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
