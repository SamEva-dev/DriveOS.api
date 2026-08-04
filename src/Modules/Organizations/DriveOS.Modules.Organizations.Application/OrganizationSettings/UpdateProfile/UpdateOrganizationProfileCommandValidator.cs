using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateProfile;

public sealed class UpdateOrganizationProfileCommandValidator
    : AbstractValidator<UpdateOrganizationProfileCommand>
{
    public UpdateOrganizationProfileCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(id => !id.IsEmpty);
        RuleFor(x => x.ExpectedVersion).GreaterThan(0);
        RuleFor(x => x.TradeName).MaximumLength(OrganizationProfile.TradeNameMaximumLength);
        RuleFor(x => x.RegistrationNumber).MaximumLength(OrganizationProfile.RegistrationNumberMaximumLength);
        RuleFor(x => x.TaxNumber).MaximumLength(OrganizationProfile.TaxNumberMaximumLength);
    }
}
