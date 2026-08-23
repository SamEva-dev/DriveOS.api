namespace DriveOS.Modules.Workforce.Domain.Employees;

/// <summary>Lifecycle of the employment relationship. WFR-002 owns the transitions.</summary>
public enum EmploymentStatus
{
    Draft = 0,
    Onboarding = 1,
    Active = 2,
    Suspended = 3,
    OnLeave = 4,
    Ending = 5,
    Ended = 6
}
