using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.CalendarResources;

public sealed record CreateCalendarResourceCommand(OrganizationId OrganizationId, BranchId? BranchId, int ResourceType, Guid ExternalResourceId, string DisplayName, int Capacity, string TimeZoneId) : ICommand<CalendarResourceId>;
public sealed record UpdateCalendarResourceCommand(OrganizationId OrganizationId, CalendarResourceId Id, BranchId? BranchId, string DisplayName, int Capacity, string TimeZoneId) : ICommand;
public sealed record RestrictCalendarResourceCommand(OrganizationId OrganizationId, CalendarResourceId Id, string Reason) : ICommand;
public sealed record MarkCalendarResourceUnavailableCommand(OrganizationId OrganizationId, CalendarResourceId Id, string? Reason) : ICommand;
public sealed record ActivateCalendarResourceCommand(OrganizationId OrganizationId, CalendarResourceId Id) : ICommand;
public sealed record ArchiveCalendarResourceCommand(OrganizationId OrganizationId, CalendarResourceId Id) : ICommand;

public static class CalendarResourceApplicationErrors
{
    public static readonly Error NotFound = Error.NotFound("Scheduling.CalendarResource.NotFound", "errors.scheduling.calendarResource.notFound");
    public static readonly Error Duplicate = Error.Conflict("Scheduling.CalendarResource.Duplicate", "errors.scheduling.calendarResource.duplicate");
}
