using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Availability;

public static class AvailabilityPlanErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.Id.Invalid",
        "errors.schedulingCapacity.availabilityPlan.id.invalid");

    public static readonly Error InvalidOrganization = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.Organization.Invalid",
        "errors.schedulingCapacity.availabilityPlan.organization.invalid");

    public static readonly Error InvalidResource = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.Resource.Invalid",
        "errors.schedulingCapacity.availabilityPlan.resource.invalid");

    public static readonly Error InvalidEffectivePeriod = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.EffectivePeriod.Invalid",
        "errors.schedulingCapacity.availabilityPlan.effectivePeriod.invalid");

    public static readonly Error InvalidTimeRange = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.TimeRange.Invalid",
        "errors.schedulingCapacity.availabilityPlan.timeRange.invalid");

    public static readonly Error InvalidCapacity = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.Capacity.Invalid",
        "errors.schedulingCapacity.availabilityPlan.capacity.invalid");


    public static readonly Error InvalidRuleType = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.RuleType.Invalid",
        "errors.schedulingCapacity.availabilityPlan.ruleType.invalid");

    public static readonly Error InvalidSource = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.Source.Invalid",
        "errors.schedulingCapacity.availabilityPlan.source.invalid");

    public static readonly Error InvalidPriority = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.Priority.Invalid",
        "errors.schedulingCapacity.availabilityPlan.priority.invalid");

    public static readonly Error InvalidTrainingCategory = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.TrainingCategory.Invalid",
        "errors.schedulingCapacity.availabilityPlan.trainingCategory.invalid");

    public static readonly Error InvalidServiceArea = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.ServiceArea.Invalid",
        "errors.schedulingCapacity.availabilityPlan.serviceArea.invalid");

    public static readonly Error InvalidTravelDistance = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.TravelDistance.Invalid",
        "errors.schedulingCapacity.availabilityPlan.travelDistance.invalid");

    public static readonly Error InvalidMinimumNotice = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.MinimumNotice.Invalid",
        "errors.schedulingCapacity.availabilityPlan.minimumNotice.invalid");

    public static readonly Error InvalidTrainingFrequency = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.TrainingFrequency.Invalid",
        "errors.schedulingCapacity.availabilityPlan.trainingFrequency.invalid");

    public static readonly Error InvalidMeetingPoint = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.MeetingPoint.Invalid",
        "errors.schedulingCapacity.availabilityPlan.meetingPoint.invalid");

    public static readonly Error InvalidExceptionType = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.ExceptionType.Invalid",
        "errors.schedulingCapacity.availabilityPlan.exceptionType.invalid");

    public static readonly Error InvalidReason = Error.Validation(
        "SchedulingCapacity.AvailabilityPlan.Reason.Invalid",
        "errors.schedulingCapacity.availabilityPlan.reason.invalid");

    public static readonly Error RuleOverlap = Error.Conflict(
        "SchedulingCapacity.AvailabilityPlan.Rule.Overlap",
        "errors.schedulingCapacity.availabilityPlan.rule.overlap");

    public static readonly Error ExceptionOverlap = Error.Conflict(
        "SchedulingCapacity.AvailabilityPlan.Exception.Overlap",
        "errors.schedulingCapacity.availabilityPlan.exception.overlap");

    public static readonly Error RuleNotFound = Error.NotFound(
        "SchedulingCapacity.AvailabilityPlan.Rule.NotFound",
        "errors.schedulingCapacity.availabilityPlan.rule.notFound");

    public static readonly Error ExceptionNotFound = Error.NotFound(
        "SchedulingCapacity.AvailabilityPlan.Exception.NotFound",
        "errors.schedulingCapacity.availabilityPlan.exception.notFound");

    public static readonly Error ActivationNotAllowed = Error.Conflict(
        "SchedulingCapacity.AvailabilityPlan.Activation.NotAllowed",
        "errors.schedulingCapacity.availabilityPlan.activation.notAllowed");

    public static readonly Error ModificationNotAllowed = Error.Conflict(
        "SchedulingCapacity.AvailabilityPlan.Modification.NotAllowed",
        "errors.schedulingCapacity.availabilityPlan.modification.notAllowed");

    public static readonly Error ArchiveNotAllowed = Error.Conflict(
        "SchedulingCapacity.AvailabilityPlan.Archive.NotAllowed",
        "errors.schedulingCapacity.availabilityPlan.archive.notAllowed");
}
