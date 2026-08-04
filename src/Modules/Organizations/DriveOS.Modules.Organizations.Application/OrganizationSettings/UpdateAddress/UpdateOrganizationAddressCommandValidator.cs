using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateAddress;

public sealed class UpdateOrganizationAddressCommandValidator
    : AbstractValidator<UpdateOrganizationAddressCommand>
{
    public UpdateOrganizationAddressCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(id => !id.IsEmpty);
        RuleFor(x => x.ExpectedVersion).GreaterThan(0);
        RuleFor(x => x.AddressCountryCode).NotEmpty().Length(2);
        RuleFor(x => x.AddressLine1).MaximumLength(OrganizationAddress.AddressLineMaximumLength);
        RuleFor(x => x.AddressLine2).MaximumLength(OrganizationAddress.AddressLineMaximumLength);
        RuleFor(x => x.PostalCode).MaximumLength(OrganizationAddress.PostalCodeMaximumLength);
        RuleFor(x => x.City).MaximumLength(OrganizationAddress.CityMaximumLength);
        RuleFor(x => x.Region).MaximumLength(OrganizationAddress.RegionMaximumLength);
    }
}
