using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Availability;

public sealed record AvailabilityRuleResponse(
    Guid Id,
    string DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int Capacity,
    string Type,
    string Source,
    int Priority,
    Guid? BranchId,
    string? TrainingCategory,
    string? ServiceArea);

public sealed record AvailabilityExceptionResponse(
    Guid Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Type,
    string Source,
    int Priority,
    int? Capacity,
    string? Reason);

public sealed record AvailabilityPreferencesResponse(
    string? PreferredMeetingPoint,
    decimal? MaximumTravelDistanceKm,
    int? MinimumNoticeMinutes,
    int? TrainingFrequencyPerWeek,
    Guid? PreferredInstructorId,
    bool IntensiveRhythm,
    bool OneTimeGeolocationAllowed);

public sealed record ImpactedBookingResponse(
    Guid BookingId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    string Status);

public sealed record AddAvailabilityExceptionResult(
    Guid AvailabilityExceptionId,
    string Source,
    IReadOnlyCollection<ImpactedBookingResponse> ImpactedBookings);

public sealed record AvailabilityPlanResponse(
    Guid Id,
    Guid CalendarResourceId,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string Status,
    IReadOnlyCollection<AvailabilityRuleResponse> Rules,
    IReadOnlyCollection<AvailabilityExceptionResponse> Exceptions,
    AvailabilityPreferencesResponse Preferences,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);

public interface IAvailabilityPlanReadService
{
    Task<IReadOnlyCollection<AvailabilityPlanResponse>> ListForResourceAsync(
        OrganizationId organizationId,
        CalendarResourceId resourceId,
        CancellationToken cancellationToken = default);

    Task<AvailabilityPlanResponse?> GetAsync(
        OrganizationId organizationId,
        AvailabilityPlanId id,
        CancellationToken cancellationToken = default);
}

public interface IAvailabilityImpactAssessmentService
{
    Task<IReadOnlyCollection<ImpactedBookingResponse>> FindImpactedBookingsAsync(
        OrganizationId organizationId,
        CalendarResourceId resourceId,
        DateOnly localDate,
        TimeOnly localStart,
        TimeOnly localEnd,
        string timeZoneId,
        CancellationToken cancellationToken = default);
}
