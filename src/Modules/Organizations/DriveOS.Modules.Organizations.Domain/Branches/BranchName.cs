using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Branches;

public sealed record BranchName
{
    public const int MaximumLength = 150;

    private BranchName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public string NormalizedValue =>
        Value.ToUpperInvariant();

    public static Result<BranchName> Create(string? value)
    {
        string normalized = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Result.Failure<BranchName>(
                BranchErrors.EmptyName);
        }

        if (normalized.Length > MaximumLength)
        {
            return Result.Failure<BranchName>(
                BranchErrors.NameTooLong(MaximumLength));
        }

        return Result.Success(new BranchName(normalized));
    }

    public override string ToString() => Value;
}
