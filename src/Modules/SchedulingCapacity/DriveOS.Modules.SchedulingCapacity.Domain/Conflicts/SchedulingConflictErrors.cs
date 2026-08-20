using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;

public static class SchedulingConflictErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("SchedulingCapacity.Conflict.InvalidIdentifier", "errors.schedulingCapacity.conflict.invalidIdentifier");
    public static readonly Error InvalidOrganization = Error.Validation("SchedulingCapacity.Conflict.InvalidOrganization", "errors.schedulingCapacity.conflict.invalidOrganization");
    public static readonly Error InvalidReason = Error.Validation("SchedulingCapacity.Conflict.InvalidReason", "errors.schedulingCapacity.conflict.invalidReason");
    public static readonly Error InvalidOverride = Error.Validation("SchedulingCapacity.Conflict.InvalidOverride", "errors.schedulingCapacity.conflict.invalidOverride");
    public static readonly Error CriticalOverrideForbidden = Error.Conflict("SchedulingCapacity.Conflict.CriticalOverrideForbidden", "errors.schedulingCapacity.conflict.criticalOverrideForbidden");
    public static readonly Error AlreadyClosed = Error.Conflict("SchedulingCapacity.Conflict.AlreadyClosed", "errors.schedulingCapacity.conflict.alreadyClosed");
}
