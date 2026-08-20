namespace DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;

public enum WaitingListProposalStatus
{
    Proposed = 1,
    TemporarilyHeld = 2,
    Accepted = 3,
    Declined = 4,
    Expired = 5,
    Released = 6
}
