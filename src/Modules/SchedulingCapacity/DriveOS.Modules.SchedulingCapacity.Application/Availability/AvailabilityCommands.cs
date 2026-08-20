using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.Availability;

public sealed record CreateAvailabilityPlanCommand(OrganizationId OrganizationId, CalendarResourceId CalendarResourceId, DateOnly EffectiveFrom, DateOnly? EffectiveTo) : ICommand<AvailabilityPlanId>;
public sealed record AddAvailabilityRuleCommand(
    OrganizationId OrganizationId,
    AvailabilityPlanId PlanId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int Capacity,
    int Type,
    int Source,
    int Priority,
    BranchId? BranchId,
    string? TrainingCategory,
    string? ServiceArea) : ICommand<AvailabilityRuleId>;
public sealed record RemoveAvailabilityRuleCommand(OrganizationId OrganizationId, AvailabilityPlanId PlanId, AvailabilityRuleId RuleId) : ICommand;
public sealed record AddAvailabilityExceptionCommand(
    OrganizationId OrganizationId,
    AvailabilityPlanId PlanId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int Type,
    int? Capacity,
    string? Reason,
    int? Source,
    int? Priority) : ICommand<AddAvailabilityExceptionResult>;
public sealed record RemoveAvailabilityExceptionCommand(OrganizationId OrganizationId, AvailabilityPlanId PlanId, AvailabilityExceptionId ExceptionId) : ICommand;
public sealed record UpdateAvailabilityPreferencesCommand(
    OrganizationId OrganizationId,
    AvailabilityPlanId PlanId,
    string? PreferredMeetingPoint,
    decimal? MaximumTravelDistanceKm,
    int? MinimumNoticeMinutes,
    int? TrainingFrequencyPerWeek,
    UserId? PreferredInstructorId,
    bool IntensiveRhythm,
    bool OneTimeGeolocationAllowed) : ICommand;
public sealed record ActivateAvailabilityPlanCommand(OrganizationId OrganizationId, AvailabilityPlanId PlanId) : ICommand;
public sealed record ArchiveAvailabilityPlanCommand(OrganizationId OrganizationId, AvailabilityPlanId PlanId) : ICommand;

public static class AvailabilityApplicationErrors
{
    public static readonly Error ResourceNotFound = Error.NotFound("Scheduling.Availability.ResourceNotFound", "errors.scheduling.availability.resourceNotFound");
    public static readonly Error PlanNotFound = Error.NotFound("Scheduling.Availability.PlanNotFound", "errors.scheduling.availability.planNotFound");
}
