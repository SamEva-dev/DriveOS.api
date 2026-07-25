using FluentValidation;

namespace DriveOS.Modules.Organizations.Application
    .Organizations.CreateOrganization;

public sealed class CreateOrganizationCommandValidator
    : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(command => command.LegalName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.CountryCode)
            .NotEmpty()
            .Length(2)
            .Matches("^[A-Za-z]{2}$");

        RuleFor(command => command.OrganizationType)
            .IsInEnum();
    }
}