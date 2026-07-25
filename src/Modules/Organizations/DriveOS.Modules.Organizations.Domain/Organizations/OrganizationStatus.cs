namespace DriveOS.Modules.Organizations.Domain.Organizations;

public enum OrganizationStatus
{
    Draft = 0,
    PendingActivation = 1,
    Active = 2,
    Restricted = 3,
    Suspended = 4,
    Closed = 5,
    Archived = 6
}