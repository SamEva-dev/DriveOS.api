namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public sealed record BookingCancellationPolicyResolutionSnapshot(
    string PolicyCode,
    int PolicyVersion,
    string ExplanationKey,
    BookingCreditDecision CreditDecision,
    BookingFeeDecision FeeDecision,
    bool ReplacementRequired);
