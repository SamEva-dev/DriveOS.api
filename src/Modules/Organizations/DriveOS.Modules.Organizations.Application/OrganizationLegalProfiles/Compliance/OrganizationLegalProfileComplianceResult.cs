namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;

public sealed record OrganizationLegalProfileComplianceResult(
    IReadOnlyCollection<OrganizationLegalProfileComplianceIssue> Issues
)
{
    public bool IsCompliant =>
        Issues.All(x => x.Severity != OrganizationLegalProfileComplianceSeverity.Blocking);

    public static OrganizationLegalProfileComplianceResult Success() =>
        new(Array.Empty<OrganizationLegalProfileComplianceIssue>());
}
