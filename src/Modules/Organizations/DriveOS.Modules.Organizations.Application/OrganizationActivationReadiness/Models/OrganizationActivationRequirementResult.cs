using DriveOS.Modules.Organizations.Domain.OrganizationActivationReadiness;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;

public sealed record OrganizationActivationRequirementResult(
    string Code,
    bool IsSatisfied,
    OrganizationActivationRequirementSeverity Severity,
    string MessageKey,
    IReadOnlyDictionary<string, object?> Parameters
)
{
    public static OrganizationActivationRequirementResult Satisfied(
        string code,
        string messageKey
    ) =>
        new(
            code,
            true,
            OrganizationActivationRequirementSeverity.Information,
            messageKey,
            new Dictionary<string, object?>()
        );

    public static OrganizationActivationRequirementResult Missing(
        string code,
        string messageKey,
        OrganizationActivationRequirementSeverity severity =
            OrganizationActivationRequirementSeverity.Blocking,
        IReadOnlyDictionary<string, object?>? parameters = null
    ) => new(code, false, severity, messageKey, parameters ?? new Dictionary<string, object?>());
}
