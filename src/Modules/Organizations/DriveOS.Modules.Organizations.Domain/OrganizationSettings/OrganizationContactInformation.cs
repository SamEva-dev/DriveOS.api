using System.Net.Mail;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSettings;

public sealed record OrganizationContactInformation
{
    public const int EmailMaximumLength = 320;
    public const int PhoneMaximumLength = 40;
    public const int WebsiteMaximumLength = 500;

    private OrganizationContactInformation(
        string? email,
        string? phone,
        string? website)
    {
        Email = email;
        Phone = phone;
        Website = website;
    }

    public string? Email { get; }
    public string? Phone { get; }
    public string? Website { get; }

    public static Result<OrganizationContactInformation> Create(
        string? email,
        string? phone,
        string? website)
    {
        string? normalizedEmail = NormalizeOptional(email)?.ToLowerInvariant();
        string? normalizedPhone = NormalizeOptional(phone);
        string? normalizedWebsite = NormalizeOptional(website);

        if (normalizedEmail is not null &&
            (normalizedEmail.Length > EmailMaximumLength || !IsValidEmail(normalizedEmail)))
        {
            return Result.Failure<OrganizationContactInformation>(
                OrganizationSettingsErrors.InvalidEmail);
        }

        if (normalizedPhone?.Length > PhoneMaximumLength)
        {
            return Result.Failure<OrganizationContactInformation>(
                OrganizationSettingsErrors.InvalidPhone);
        }

        if (normalizedWebsite is not null &&
            (normalizedWebsite.Length > WebsiteMaximumLength ||
             !Uri.TryCreate(normalizedWebsite, UriKind.Absolute, out Uri? uri) ||
             (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
        {
            return Result.Failure<OrganizationContactInformation>(
                OrganizationSettingsErrors.InvalidWebsite);
        }

        return Result.Success(
            new OrganizationContactInformation(
                normalizedEmail,
                normalizedPhone,
                normalizedWebsite));
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            return new MailAddress(email).Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
