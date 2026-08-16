namespace DriveOS.Modules.Students.Domain.Relationships;

public enum RelatedPartyKind
{
    Person = 1,
    Organization = 2,
}

public enum StudentRelationshipType
{
    Payer = 1,
    BillingContact = 2,
    EmergencyContact = 3,
    AuthorizedContact = 4,
    EmployerContact = 5,
    FunderContact = 6,
    Guardian = 7,
    PartnerContact = 8,
}

public enum StudentRelationshipStatus
{
    Active = 1,
    Suspended = 2,
    Revoked = 3,
    Expired = 4,
}

[Flags]
public enum StudentRelationshipPermissions
{
    None = 0,
    ReceiveInformation = 1,
    Pay = 2,
    ViewInvoices = 4,
    ReceiveDocuments = 8,
    BeContacted = 16,
}

[Flags]
public enum FinancialScope
{
    None = 0,
    Invoices = 1,
    Payments = 2,
    Refunds = 4,
    Contracts = 8,
    All = Invoices | Payments | Refunds | Contracts,
}

[Flags]
public enum CommunicationScope
{
    None = 0,
    General = 1,
    Administrative = 2,
    Financial = 4,
    Pedagogical = 8,
    Emergency = 16,
    All = General | Administrative | Financial | Pedagogical | Emergency,
}
