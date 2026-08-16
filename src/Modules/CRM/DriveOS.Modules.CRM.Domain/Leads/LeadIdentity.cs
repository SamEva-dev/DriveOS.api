using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Leads;

public sealed record LeadIdentity
{
    private LeadIdentity(string firstName, string lastName, string? email, string? phone)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
    }

    private LeadIdentity() { }

    public string FirstName { get; private init; } = string.Empty;

    public string LastName { get; private init; } = string.Empty;

    public string? Email { get; private init; }

    public string? Phone { get; private init; }

    public string DisplayName => $"{FirstName} {LastName}".Trim();

    public static Result<LeadIdentity> Create(
        string firstName,
        string lastName,
        string? email,
        string? phone
    )
    {
        string normalizedFirstName = firstName?.Trim() ?? string.Empty;
        string normalizedLastName = lastName?.Trim() ?? string.Empty;
        string? normalizedEmail = NormalizeOptional(email)?.ToLowerInvariant();
        string? normalizedPhone = NormalizeOptional(phone);

        if (string.IsNullOrWhiteSpace(normalizedFirstName))
        {
            return Result.Failure<LeadIdentity>(LeadErrors.FirstNameRequired);
        }

        if (normalizedFirstName.Length > 100)
        {
            return Result.Failure<LeadIdentity>(LeadErrors.FirstNameTooLong);
        }

        if (string.IsNullOrWhiteSpace(normalizedLastName))
        {
            return Result.Failure<LeadIdentity>(LeadErrors.LastNameRequired);
        }

        if (normalizedLastName.Length > 100)
        {
            return Result.Failure<LeadIdentity>(LeadErrors.LastNameTooLong);
        }

        if (
            normalizedEmail is not null
            && (normalizedEmail.Length > 254 || !normalizedEmail.Contains('@'))
        )
        {
            return Result.Failure<LeadIdentity>(LeadErrors.InvalidEmail);
        }

        if (normalizedPhone?.Length > 40)
        {
            return Result.Failure<LeadIdentity>(LeadErrors.PhoneTooLong);
        }

        return Result.Success(
            new LeadIdentity(
                normalizedFirstName,
                normalizedLastName,
                normalizedEmail,
                normalizedPhone
            )
        );
    }

    private static string? NormalizeOptional(string? value)
    {
        string normalized = value?.Trim() ?? string.Empty;
        return normalized.Length == 0 ? null : normalized;
    }
}
