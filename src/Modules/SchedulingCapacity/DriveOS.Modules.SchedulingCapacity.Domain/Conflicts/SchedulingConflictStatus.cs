namespace DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;

public enum SchedulingConflictStatus
{
    Open = 1,
    ResolutionRequested = 2,
    Overridden = 3,
    Resolved = 4,
    Obsolete = 5
}
