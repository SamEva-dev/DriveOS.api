using FluentValidation;

namespace DriveOS.Modules.CRM.Application.Leads.QualifyLead;

public sealed class QualifyLeadCommandValidator : AbstractValidator<QualifyLeadCommand>
{
    public QualifyLeadCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.LeadId).Must(x => !x.IsEmpty);
        RuleFor(x => x.Need).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.LicenseCategory).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Availability).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Financing).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
