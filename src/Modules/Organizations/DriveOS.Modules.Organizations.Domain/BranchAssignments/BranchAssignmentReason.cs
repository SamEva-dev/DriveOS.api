using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.BranchAssignments;

public sealed record BranchAssignmentReason
{
    public const int MaximumLength = 500;

    private BranchAssignmentReason(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<BranchAssignmentReason> Create(string? value)
    {
        string normalizedValue = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            return Result.Failure<BranchAssignmentReason>(BranchUserAssignmentErrors.EmptyReason);
        }

        if (normalizedValue.Length > MaximumLength)
        {
            return Result.Failure<BranchAssignmentReason>(BranchUserAssignmentErrors.ReasonTooLong);
        }

        return Result.Success(new BranchAssignmentReason(normalizedValue));
    }

    public override string ToString() => Value;
}
