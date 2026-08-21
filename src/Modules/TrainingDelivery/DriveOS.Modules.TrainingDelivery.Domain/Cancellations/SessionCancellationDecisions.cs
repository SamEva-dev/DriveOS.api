namespace DriveOS.Modules.TrainingDelivery.Domain.Cancellations;

public enum SessionCancellationBillingDecision
{
    NoCharge = 1,
    BillDeliveredTime = 2,
    BillPlannedDuration = 3,
    ManualReview = 4
}

public enum SessionCancellationCreditDecision
{
    NotApplicable = 1,
    ReleaseAll = 2,
    ConsumeAll = 3,
    ConsumePartial = 4,
    ManualReview = 5
}

public enum SessionCancellationProviderCompensationDecision
{
    NotApplicable = 1,
    NoCompensation = 2,
    PayDeliveredTime = 3,
    PayPlannedDuration = 4,
    ManualReview = 5
}
