using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness;

public static class OrganizationActivationReadinessErrors
{
    public static Error RequirementsNotMet(
        IReadOnlyCollection<OrganizationActivationRequirementResult> blockingRequirements) =>
        Error.Conflict(
            code: "Organizations.Activation.RequirementsNotMet",
            messageKey: "errors.organizations.activation.requirementsNotMet",
            parameters: new Dictionary<string, object?>
            {
                ["requirements"] = blockingRequirements
                    .Select(x => new
                    {
                        x.Code,
                        x.MessageKey,
                        x.Parameters
                    })
                    .ToArray()
            });
}
