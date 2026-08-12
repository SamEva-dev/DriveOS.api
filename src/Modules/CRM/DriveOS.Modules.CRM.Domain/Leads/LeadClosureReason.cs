namespace DriveOS.Modules.CRM.Domain.Leads;

public enum LeadClosureReason
{
    PriceTooHigh = 1,
    FinancingRejected = 2,
    DelayTooLong = 3,
    TrainingUnavailable = 4,
    AreaNotCovered = 5,
    CompetitorChosen = 6,
    Unavailable = 7,
    ProjectPostponed = 8,
    NoResponse = 9,
    EligibilityConditionNotMet = 10,
    Duplicate = 11,
    PartnerReferral = 12,
    CancelledByLead = 13,
    ConvertedElsewhere = 14,
    Other = 15
}
