using DriveOS.Modules.Workforce.Application.Offboarding;
using DriveOS.Modules.Workforce.Domain.Offboarding;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Workforce.Infrastructure.AccessRevocation;

internal sealed class DisabledEmployeeApplicationAccessRevoker : IEmployeeApplicationAccessRevoker
{
    public Task<Result> RevokeAsync(OrganizationId organizationId, UserId userId, string reason, CancellationToken ct = default)
        => Task.FromResult(Result.Failure(OffboardingErrors.AccessRevocationUnavailable));
}
