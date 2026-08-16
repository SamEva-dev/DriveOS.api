namespace DriveOS.Modules.Students.Domain.Documents;

public enum StudentDocumentCategory
{
    Identity = 1,
    Residence = 2,
    Authorization = 3,
    Photograph = 4,
    RegulatoryEvidence = 5,
    Funding = 6,
    Contract = 7,
    Exam = 8,
    Certificate = 9,
    PartnerDocument = 10,
}

public enum StudentDocumentStatus
{
    Missing = 1,
    Requested = 2,
    Uploaded = 3,
    Processing = 4,
    PendingReview = 5,
    Approved = 6,
    Rejected = 7,
    Expiring = 8,
    Expired = 9,
    Replaced = 10,
    Archived = 11,
}

[Flags]
public enum StudentDocumentVisibility
{
    None = 0,
    Student = 1,
    Guardians = 2,
    AdministrativeStaff = 4,
    PedagogicalStaff = 8,
    FinanceStaff = 16,
    Partners = 32,
}

public enum StudentDocumentAccessAction
{
    Downloaded = 1,
    Shared = 2,
    ViewedSensitiveMetadata = 3,
}
