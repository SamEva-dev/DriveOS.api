using System.Text.RegularExpressions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSequences;

public sealed record SequencePattern
{
    public const int MaximumLength = 100;

    private static readonly Regex TokenRegex = new(
        "\\{[A-Z]+\\}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private static readonly HashSet<string> SupportedTokens =
    [
        "{CODE}",
        "{YYYY}",
        "{YY}",
        "{MM}",
        "{NUMBER}",
    ];

    private SequencePattern(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<SequencePattern> Create(string? value)
    {
        string normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Result.Failure<SequencePattern>(OrganizationSequenceErrors.EmptyPattern);
        }

        if (normalized.Length > MaximumLength)
        {
            return Result.Failure<SequencePattern>(
                OrganizationSequenceErrors.PatternTooLong(MaximumLength)
            );
        }

        if (!normalized.Contains("{NUMBER}", StringComparison.Ordinal))
        {
            return Result.Failure<SequencePattern>(OrganizationSequenceErrors.NumberTokenRequired);
        }

        foreach (Match match in TokenRegex.Matches(normalized))
        {
            if (!SupportedTokens.Contains(match.Value))
            {
                return Result.Failure<SequencePattern>(
                    OrganizationSequenceErrors.UnsupportedPatternToken
                );
            }
        }

        string withoutSupportedTokens = normalized;
        foreach (string token in SupportedTokens)
        {
            withoutSupportedTokens = withoutSupportedTokens.Replace(
                token,
                string.Empty,
                StringComparison.Ordinal
            );
        }

        if (withoutSupportedTokens.Contains('{') || withoutSupportedTokens.Contains('}'))
        {
            return Result.Failure<SequencePattern>(
                OrganizationSequenceErrors.UnsupportedPatternToken
            );
        }

        return Result.Success(new SequencePattern(normalized));
    }

    public string Format(string code, long number, int padding, DateTimeOffset instantUtc)
    {
        string paddedNumber = number.ToString(
            $"D{padding}",
            System.Globalization.CultureInfo.InvariantCulture
        );

        return Value
            .Replace("{CODE}", code, StringComparison.Ordinal)
            .Replace("{YYYY}", instantUtc.Year.ToString("D4"), StringComparison.Ordinal)
            .Replace("{YY}", (instantUtc.Year % 100).ToString("D2"), StringComparison.Ordinal)
            .Replace("{MM}", instantUtc.Month.ToString("D2"), StringComparison.Ordinal)
            .Replace("{NUMBER}", paddedNumber, StringComparison.Ordinal);
    }

    public override string ToString() => Value;
}
