using DriveOS.Modules.Organizations.Application.OrganizationClosures.Readiness;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.Models;

public sealed record OrganizationClosureRequirementModel(
    string Code,
    bool IsSatisfied,
    string Severity,
    string MessageKey,
    IReadOnlyDictionary<string, object?> Parameters
);

public sealed record OrganizationClosureReadinessModel(
    OrganizationId OrganizationId,
    bool CanClose,
    IReadOnlyList<OrganizationClosureRequirementModel> Requirements,
    IReadOnlyList<OrganizationClosureRequirementModel> BlockingRequirements
)
{
    public static OrganizationClosureReadinessModel FromReport(
        OrganizationClosureReadinessReport report
    )
    {
        OrganizationClosureRequirementModel Map(OrganizationClosureRequirement requirement) =>
            new(
                requirement.Code,
                requirement.IsSatisfied,
                requirement.Severity.ToString(),
                requirement.MessageKey,
                requirement.Parameters
            );

        return new(
            report.OrganizationId,
            report.CanClose,
            report.Requirements.Select(Map).ToArray(),
            report.BlockingRequirements.Select(Map).ToArray()
        );
    }
}
