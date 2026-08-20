namespace DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;

public enum WaitingListStatus
{
    Waiting = 1,
    Proposed = 2,
    TemporarilyHeld = 3,
    Accepted = 4,
    Declined = 5,
    Expired = 6,
    Cancelled = 7,
    Fulfilled = 8
}
