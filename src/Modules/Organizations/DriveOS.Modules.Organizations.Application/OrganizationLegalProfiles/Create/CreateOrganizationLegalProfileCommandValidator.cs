using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Create;

internal sealed class CreateOrganizationLegalProfileCommandValidator
    : AbstractValidator<CreateOrganizationLegalProfileCommand>
{
    public CreateOrganizationLegalProfileCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.LegalForm).IsInEnum();
        RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(80);
        RuleFor(x => x.TaxNumber).MaximumLength(80);
        RuleFor(x => x.TradeName).MaximumLength(200);
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressLine2).MaximumLength(200);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.City).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Region).MaximumLength(120);
        RuleFor(x => x.CountryCode).NotEmpty().Length(2);
    }
}
