using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.Readiness;

public enum OrganizationClosureRequirementSeverity { Information = 0, Warning = 1, Blocking = 2 }

public sealed record OrganizationClosureRequirement(
    string Code,
    bool IsSatisfied,
    OrganizationClosureRequirementSeverity Severity,
    string MessageKey,
    IReadOnlyDictionary<string, object?> Parameters);

public sealed record OrganizationClosureReadinessReport(
    OrganizationId OrganizationId,
    IReadOnlyList<OrganizationClosureRequirement> Requirements)
{
    public bool CanClose => Requirements.All(x => x.IsSatisfied || x.Severity != OrganizationClosureRequirementSeverity.Blocking);
    public IReadOnlyList<OrganizationClosureRequirement> BlockingRequirements =>
        Requirements.Where(x => !x.IsSatisfied && x.Severity == OrganizationClosureRequirementSeverity.Blocking).ToArray();
}
