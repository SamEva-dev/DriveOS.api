using DriveOS.Application.Abstractions.Authentication;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Infrastructure.Authentication;

internal sealed class AnonymousCurrentUser : ICurrentUser
{
    public bool IsAuthenticated => false;

    public UserId? UserId => null;

    public string? Email => null;

    public IReadOnlySet<string> Permissions { get; } = new HashSet<string>();

    public bool HasPermission(string permission) => false;
}
