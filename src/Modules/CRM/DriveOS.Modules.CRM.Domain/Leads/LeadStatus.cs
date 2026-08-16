namespace DriveOS.Modules.CRM.Domain.Leads;

public enum LeadStatus
{
    New = 1,
    Contacted = 2,
    Qualified = 3,
    AssessmentScheduled = 4,
    OfferSent = 5,
    Negotiation = 6,
    Won = 7,
    Lost = 8,
    Dormant = 9,
    NotEligible = 10,
    OutOfScope = 11,
    Duplicate = 12,
    TransferredToPartner = 13,
    NoResponse = 14,
    CancelledByLead = 15,
    ConvertedElsewhere = 16,
}
