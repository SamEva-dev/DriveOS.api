using DriveOS.Modules.SchedulingCapacity.Application.Availability;
using DriveOS.Modules.SchedulingCapacity.Application.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Modules.SchedulingCapacity.Domain.Availability;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;
using DriveOS.Modules.SchedulingCapacity.Application.Recurrences;
using DriveOS.Modules.SchedulingCapacity.Application.Conflicts;
using DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;
using DriveOS.Modules.SchedulingCapacity.Application.WaitingList;
using DriveOS.Modules.SchedulingCapacity.Application.Replacements;
using DriveOS.Modules.SchedulingCapacity.Application.Travel;
using DriveOS.Modules.SchedulingCapacity.Application.Capacity;
using DriveOS.Modules.SchedulingCapacity.Application.SlotSearch;
using DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Repositories;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Interceptors;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSchedulingCapacityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SchedulingTransitionOptions>(configuration.GetSection(SchedulingTransitionOptions.SectionName));
        services.Configure<SchedulingTravelOptions>(configuration.GetSection(SchedulingTravelOptions.SectionName));
        string connectionString = configuration.GetConnectionString("DriveOS") ?? throw new InvalidOperationException("The DriveOS database connection string is missing.");
        services.AddScoped<SchedulingCapacityAuditInterceptor>();
        services.AddDbContext<SchedulingCapacityDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", SchedulingCapacitySchema.Name));
            options.AddInterceptors(sp.GetRequiredService<SchedulingCapacityAuditInterceptor>());
        });
        services.AddScoped<ISchedulingCapacityUnitOfWork>(sp => sp.GetRequiredService<SchedulingCapacityDbContext>());
        services.AddScoped<ICalendarResourceRepository, CalendarResourceRepository>();
        services.AddScoped<IAvailabilityPlanRepository, AvailabilityPlanRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IRecurrenceSeriesRepository, RecurrenceSeriesRepository>();
        services.AddScoped<ISchedulingConflictRepository, SchedulingConflictRepository>();
        services.AddScoped<IWaitingListEntryRepository, WaitingListEntryRepository>();
        services.AddScoped<ICalendarResourceReadService, CalendarResourceReadService>();
        services.AddScoped<IAvailabilityPlanReadService, AvailabilityPlanReadService>();
        services.AddScoped<IAvailabilityImpactAssessmentService, AvailabilityImpactAssessmentService>();
        services.AddScoped<IBookingReadService, BookingReadService>();
        services.AddScoped<IRecurrenceSeriesReadService, RecurrenceSeriesReadService>();
        services.AddScoped<IRecurrencePreviewService, RecurrencePreviewService>();
        services.AddScoped<ISchedulingConflictReadService, SchedulingConflictReadService>();
        services.AddScoped<ISchedulingConflictInboxService, SchedulingConflictInboxService>();
        services.AddScoped<IWaitingListReadService, WaitingListReadService>();
        services.AddScoped<IWaitingListMatchingService, WaitingListMatchingService>();
        services.AddScoped<IWaitingListSlotLock, WaitingListSlotLock>();
        services.AddScoped<IBookingConflictAssessmentService, BookingConflictAssessmentService>();
        services.AddScoped<IBookingCapacityLock, BookingCapacityLock>();
        services.AddScoped<IBookingCreationIdempotencyLock, BookingCreationIdempotencyLock>();
        services.AddScoped<IInstructorReplacementService, InstructorReplacementService>();
        services.AddScoped<IVehicleReplacementService, VehicleReplacementService>();
        services.AddScoped<ITravelPlanningService, TravelPlanningService>();
        services.AddScoped<ICapacityForecastService, CapacityForecastService>();
        services.AddScoped<ISlotSearchService, SlotSearchService>();
        return services;
    }
}
