using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    UserId? UserId { get; }

    string? Email { get; }

    IReadOnlySet<string> Permissions { get; }

    bool HasPermission(string permission);
}