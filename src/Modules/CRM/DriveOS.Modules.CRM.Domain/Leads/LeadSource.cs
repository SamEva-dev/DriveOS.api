using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Leads;

public sealed record LeadSource
{
    private LeadSource(LeadSourceType type, string? detail)
    {
        Type = type;
        Detail = detail;
    }

    private LeadSource() { }

    public LeadSourceType Type { get; private init; }

    public string? Detail { get; private init; }

    public static Result<LeadSource> Create(LeadSourceType type, string? detail = null)
    {
        if (!Enum.IsDefined(type))
        {
            return Result.Failure<LeadSource>(LeadErrors.InvalidSourceType);
        }

        string? normalizedDetail = NormalizeOptional(detail);

        if (normalizedDetail?.Length > 250)
        {
            return Result.Failure<LeadSource>(LeadErrors.SourceDetailTooLong);
        }

        if (type == LeadSourceType.Other && normalizedDetail is null)
        {
            return Result.Failure<LeadSource>(LeadErrors.SourceDetailRequired);
        }

        return Result.Success(new LeadSource(type, normalizedDetail));
    }

    private static string? NormalizeOptional(string? value)
    {
        string normalized = value?.Trim() ?? string.Empty;
        return normalized.Length == 0 ? null : normalized;
    }
}
