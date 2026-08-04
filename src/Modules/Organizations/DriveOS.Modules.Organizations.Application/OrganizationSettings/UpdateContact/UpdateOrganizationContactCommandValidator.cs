using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateContact;

public sealed class UpdateOrganizationContactCommandValidator
    : AbstractValidator<UpdateOrganizationContactCommand>
{
    public UpdateOrganizationContactCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(id => !id.IsEmpty);
        RuleFor(x => x.ExpectedVersion).GreaterThan(0);
        RuleFor(x => x.Email).MaximumLength(OrganizationContactInformation.EmailMaximumLength);
        RuleFor(x => x.Phone).MaximumLength(OrganizationContactInformation.PhoneMaximumLength);
        RuleFor(x => x.Website).MaximumLength(OrganizationContactInformation.WebsiteMaximumLength);
    }
}
