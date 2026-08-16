using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.Commands;

public interface IOrganizationClosureOrchestrator
{
    Task<OrganizationClosureExecutionResult> ExecuteAsync(
        OrganizationClosure closure,
        UserId actorUserId,
        CancellationToken cancellationToken
    );
    Task<OrganizationClosureExecutionResult> ReopenAsync(
        OrganizationId organizationId,
        string justification,
        UserId actorUserId,
        CancellationToken cancellationToken
    );
}

public sealed record OrganizationClosureExecutionResult(
    bool Succeeded,
    IReadOnlyList<OrganizationClosureStepResult> Steps
);

public sealed record OrganizationClosureStepResult(
    string Step,
    bool Succeeded,
    string? ErrorCode = null
);

public static class OrganizationClosureSteps
{
    public const string BlockNewOperations = "block-new-operations";
    public const string CloseBranches = "close-branches";
    public const string FreezeSequences = "freeze-sequences";
    public const string EndRepresentatives = "end-representatives";
    public const string RevokeAccess = "revoke-access";
    public const string TerminateSubscription = "terminate-subscription";
    public const string DisableIntegrations = "disable-integrations";
    public const string ArchiveData = "archive-data";
    public const string FinalizeOrganization = "finalize-organization";
}
