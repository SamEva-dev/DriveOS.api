namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;

public sealed record OrganizationLegalProfileComplianceIssue(
    string Code,
    string Message,
    OrganizationLegalProfileComplianceSeverity Severity
);

public enum OrganizationLegalProfileComplianceSeverity
{
    Warning = 1,
    Blocking = 2,
}
