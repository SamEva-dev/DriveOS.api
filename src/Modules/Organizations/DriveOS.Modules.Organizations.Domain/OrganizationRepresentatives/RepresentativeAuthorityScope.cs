using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;

public sealed record RepresentativeAuthorityScope
{
    public const int MaximumLength = 2000;

    private RepresentativeAuthorityScope(string value) => Value = value;

    public string Value { get; }

    public static Result<RepresentativeAuthorityScope> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<RepresentativeAuthorityScope>(
                OrganizationRepresentativeErrors.AuthorityScopeRequired
            );
        }

        string normalized = value.Trim();
        if (normalized.Length > MaximumLength)
        {
            return Result.Failure<RepresentativeAuthorityScope>(
                OrganizationRepresentativeErrors.AuthorityScopeTooLong(MaximumLength)
            );
        }

        return Result.Success(new RepresentativeAuthorityScope(normalized));
    }
}
