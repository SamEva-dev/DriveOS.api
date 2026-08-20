namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class SchedulingTravelOptions
{
    public const string SectionName = "Scheduling:Travel";
    public int DefaultSafetyBufferMinutes { get; init; } = 10;
    public bool AllowContinuousTracking { get; init; } = false;
}
