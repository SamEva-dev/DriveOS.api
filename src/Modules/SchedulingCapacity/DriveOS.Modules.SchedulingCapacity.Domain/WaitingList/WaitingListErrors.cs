using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;

public static class WaitingListErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("SchedulingCapacity.WaitingList.InvalidIdentifier", "errors.schedulingCapacity.waitingList.invalidIdentifier");
    public static readonly Error InvalidPeriod = Error.Validation("SchedulingCapacity.WaitingList.InvalidPeriod", "errors.schedulingCapacity.waitingList.invalidPeriod");
    public static readonly Error InvalidDuration = Error.Validation("SchedulingCapacity.WaitingList.InvalidDuration", "errors.schedulingCapacity.waitingList.invalidDuration");
    public static readonly Error InvalidPriority = Error.Validation("SchedulingCapacity.WaitingList.InvalidPriority", "errors.schedulingCapacity.waitingList.invalidPriority");
    public static readonly Error InvalidReason = Error.Validation("SchedulingCapacity.WaitingList.InvalidReason", "errors.schedulingCapacity.waitingList.invalidReason");
    public static readonly Error InvalidExpiration = Error.Validation("SchedulingCapacity.WaitingList.InvalidExpiration", "errors.schedulingCapacity.waitingList.invalidExpiration");
    public static readonly Error NotWaiting = Error.Conflict("SchedulingCapacity.WaitingList.NotWaiting", "errors.schedulingCapacity.waitingList.notWaiting");
    public static readonly Error ProposalNotFound = Error.NotFound("SchedulingCapacity.WaitingList.ProposalNotFound", "errors.schedulingCapacity.waitingList.proposalNotFound");
    public static readonly Error ProposalClosed = Error.Conflict("SchedulingCapacity.WaitingList.ProposalClosed", "errors.schedulingCapacity.waitingList.proposalClosed");
    public static readonly Error FulfillmentMismatch = Error.Conflict("SchedulingCapacity.WaitingList.FulfillmentMismatch", "errors.schedulingCapacity.waitingList.fulfillmentMismatch");
    public static readonly Error HoldExpired = Error.Conflict("SchedulingCapacity.WaitingList.HoldExpired", "errors.schedulingCapacity.waitingList.holdExpired");
}
