namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

/// <summary>
/// Commercial availability advertised on Marketplace. It is intentionally not a Scheduling booking
/// calendar. BC-09 remains authoritative for operational conflicts, resources and confirmed bookings.
/// </summary>
public sealed record MarketplaceAvailabilityRule(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string TimeZoneId);

public sealed record MarketplaceAvailabilityException(
    DateOnly Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    MarketplaceAvailabilityExceptionType Type,
    string? Reason);

public sealed record MarketplaceAvailabilityPolicy(
    MarketplaceAvailabilityRule[] RecurringRules,
    MarketplaceAvailabilityException[] Exceptions,
    int MinimumBookingNoticeHours,
    int MaximumDailyWorkMinutes,
    int MaximumConsecutiveWorkMinutes);

public enum MarketplaceAvailabilityExceptionType
{
    Available = 1,
    Unavailable = 2
}
