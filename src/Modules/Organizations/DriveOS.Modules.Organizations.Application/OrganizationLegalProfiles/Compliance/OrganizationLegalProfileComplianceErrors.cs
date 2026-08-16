using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;

public static class OrganizationLegalProfileComplianceErrors
{
    public static Error ActivationBlocked(
        IReadOnlyCollection<OrganizationLegalProfileComplianceIssue> issues
    ) =>
        Error.Validation(
            "OrganizationLegalProfiles.Compliance.ActivationBlocked",
            string.Join(
                " | ",
                issues
                    .Where(x => x.Severity == OrganizationLegalProfileComplianceSeverity.Blocking)
                    .Select(x => x.Code)
            )
        );
}
