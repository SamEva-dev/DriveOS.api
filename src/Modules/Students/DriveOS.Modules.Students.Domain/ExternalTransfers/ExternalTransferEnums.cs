namespace DriveOS.Modules.Students.Domain.ExternalTransfers;

public enum ExternalTransferType
{
    FullTransfer = 1,
    PartialTrainingTransfer = 2,
    TemporaryTransfer = 3,
    EmergencyTransfer = 4,
    PartnerExecution = 5,
}

[Flags]
public enum ExternalTransferDataScope
{
    None = 0,
    Identity = 1,
    ContactDetails = 2,
    SelectedDocuments = 4,
    TrainingHistory = 8,
    Assessments = 16,
    CompletedHours = 32,
    Exams = 64,
    RelevantContracts = 128,
    Credits = 256,
    AuthorizedFinance = 512,
    AuthorizedSpecialNeeds = 1024,
    All = 2047,
}

public enum ExternalTransferStatus
{
    Draft = 1,
    ConsentPending = 2,
    TargetReview = 3,
    Accepted = 4,
    Rejected = 5,
    Scheduled = 6,
    InProgress = 7,
    Completed = 8,
    Cancelled = 9,
    Disputed = 10,
}

public enum TransferConsentStatus
{
    Pending = 1,
    Verified = 2,
    Withdrawn = 3,
}

public enum TransferFinancialStatus
{
    Pending = 1,
    Cleared = 2,
    ResolutionRequired = 3,
    Resolved = 4,
}

public enum TargetRelationshipStatus
{
    Active = 1,
    InvitationRequested = 2,
    Missing = 3,
}
