using DriveOS.Modules.Organizations.Domain.Organizations;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.Organizations.CreateOrganization;

public sealed class CreateOrganizationCommandValidator
    : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(command => command.LegalName)
            .NotEmpty()
            .WithErrorCode("Organizations.LegalName.Empty")
            .WithMessage("errors.organizations.legalName.empty");

        RuleFor(command => command.LegalName)
            .MaximumLength(200)
            .WithErrorCode("Organizations.LegalName.TooLong")
            .WithMessage("errors.organizations.legalName.tooLong");

        RuleFor(command => command.CountryCode)
            .NotEmpty()
            .WithErrorCode("Organizations.CountryCode.Empty")
            .WithMessage("errors.organizations.countryCode.empty");

        RuleFor(command => command.CountryCode)
            .Matches("^[A-Za-z]{2}$")
            .When(command => !string.IsNullOrWhiteSpace(command.CountryCode))
            .WithErrorCode("Organizations.CountryCode.Invalid")
            .WithMessage("errors.organizations.countryCode.invalid");

        RuleFor(command => (OrganizationType)command.OrganizationType)
            .IsInEnum()
            .WithErrorCode("Organizations.Type.Invalid")
            .WithMessage("errors.organizations.type.invalid");
    }
}
