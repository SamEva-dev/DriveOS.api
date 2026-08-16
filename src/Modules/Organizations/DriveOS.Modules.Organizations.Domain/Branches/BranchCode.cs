using System.Text.RegularExpressions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Branches;

public sealed record BranchCode
{
    public const int MaximumLength = 20;

    private static readonly Regex AllowedCharacters = new(
        "^[A-Z0-9][A-Z0-9_-]*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private BranchCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<BranchCode> Create(string? value)
    {
        string normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Result.Failure<BranchCode>(BranchErrors.EmptyCode);
        }

        if (normalized.Length > MaximumLength)
        {
            return Result.Failure<BranchCode>(BranchErrors.CodeTooLong(MaximumLength));
        }

        if (!AllowedCharacters.IsMatch(normalized))
        {
            return Result.Failure<BranchCode>(BranchErrors.InvalidCode);
        }

        return Result.Success(new BranchCode(normalized));
    }

    public override string ToString() => Value;
}
