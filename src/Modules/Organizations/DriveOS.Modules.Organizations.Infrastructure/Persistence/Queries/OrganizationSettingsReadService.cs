using DriveOS.Modules.Organizations.Application.OrganizationSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.Models;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Queries;

internal sealed class OrganizationSettingsReadService(
    OrganizationsDbContext dbContext)
    : IOrganizationSettingsReadService
{
    public async Task<OrganizationSettingsResponse?> GetByOrganizationIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default)
    {
        var row = await dbContext.OrganizationSettings
            .AsNoTracking()
            .Where(settings => settings.OrganizationId == organizationId)
            .Select(settings => new
            {
                Id = settings.Id.Value,
                OrganizationId = settings.OrganizationId.Value,
                settings.Profile.TradeName,
                settings.Profile.RegistrationNumber,
                settings.Profile.TaxNumber,
                settings.Contact.Email,
                settings.Contact.Phone,
                settings.Contact.Website,
                AddressLine1 = settings.Address.Line1,
                AddressLine2 = settings.Address.Line2,
                settings.Address.PostalCode,
                settings.Address.City,
                settings.Address.Region,
                AddressCountryCode = settings.Address.CountryCode,
                settings.Regional.DefaultLanguage,
                SupportedLanguages = settings.Regional.SupportedLanguages,
                settings.Regional.TimeZoneId,
                settings.Regional.CurrencyCode,
                settings.Regional.DateFormat,
                settings.Regional.TimeFormat,
                FirstDayOfWeek = (int)settings.Regional.FirstDayOfWeek,
                MeasurementSystem = (int)settings.Regional.MeasurementSystem,
                settings.Operational.DefaultSessionDurationMinutes,
                settings.Operational.DefaultBookingLeadTimeMinutes,
                settings.Operational.DefaultCancellationDelayHours,
                settings.Operational.AllowStudentSelfBooking,
                settings.Operational.RequireBranchForOperations,
                DefaultBranchId = settings.Operational.DefaultBranchId == null
                    ? (Guid?)null
                    : settings.Operational.DefaultBranchId.Value.Value,
                settings.Version,
                settings.CreatedAtUtc,
                settings.LastModifiedAtUtc
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new OrganizationSettingsResponse(
            row.Id,
            row.OrganizationId,
            row.TradeName,
            row.RegistrationNumber,
            row.TaxNumber,
            row.Email,
            row.Phone,
            row.Website,
            row.AddressLine1,
            row.AddressLine2,
            row.PostalCode,
            row.City,
            row.Region,
            row.AddressCountryCode,
            row.DefaultLanguage,
            row.SupportedLanguages
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            row.TimeZoneId,
            row.CurrencyCode,
            row.DateFormat,
            row.TimeFormat,
            row.FirstDayOfWeek,
            row.MeasurementSystem,
            row.DefaultSessionDurationMinutes,
            row.DefaultBookingLeadTimeMinutes,
            row.DefaultCancellationDelayHours,
            row.AllowStudentSelfBooking,
            row.RequireBranchForOperations,
            row.DefaultBranchId,
            row.Version,
            row.CreatedAtUtc,
            row.LastModifiedAtUtc);
    }
}
