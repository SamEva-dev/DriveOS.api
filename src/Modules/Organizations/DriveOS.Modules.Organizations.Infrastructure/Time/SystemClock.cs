using DriveOS.Application.Abstractions.Time;

namespace DriveOS.Modules.Organizations.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
