using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;

public sealed record OrganizationActivationReadinessReport(
    OrganizationId OrganizationId,
    bool IsReady,
    IReadOnlyCollection<OrganizationActivationRequirementResult> Requirements
)
{
    public IReadOnlyCollection<OrganizationActivationRequirementResult> BlockingRequirements =>
        Requirements
            .Where(x =>
                !x.IsSatisfied
                && x.Severity
                    == Domain
                        .OrganizationActivationReadiness
                        .OrganizationActivationRequirementSeverity
                        .Blocking
            )
            .ToArray();
}
