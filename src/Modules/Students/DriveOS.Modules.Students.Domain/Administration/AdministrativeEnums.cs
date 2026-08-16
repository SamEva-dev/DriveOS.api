namespace DriveOS.Modules.Students.Domain.Administration;

public enum AdministrativeStatus
{
    ToComplete = 1,
    UnderReview = 2,
    Compliant = 3,
    Blocked = 4,
}

public enum AdministrativeRequirementStatus
{
    Missing = 1,
    Submitted = 2,
    Validated = 3,
    Rejected = 4,
    Waived = 5,
    Expired = 6,
}

public enum ComplianceExceptionStatus
{
    Requested = 1,
    Approved = 2,
    Rejected = 3,
}
