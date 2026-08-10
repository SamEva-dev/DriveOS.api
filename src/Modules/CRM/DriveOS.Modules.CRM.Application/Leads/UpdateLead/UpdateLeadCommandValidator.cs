using DriveOS.Modules.CRM.Domain.Leads;
using FluentValidation;

namespace DriveOS.Modules.CRM.Application.Leads.UpdateLead;

public sealed class UpdateLeadCommandValidator : AbstractValidator<UpdateLeadCommand>
{
    public UpdateLeadCommandValidator()
    {
        RuleFor(command => command.OrganizationId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("Crm.Leads.OrganizationId.Empty")
            .WithMessage("errors.crm.leads.organizationId.empty");

        RuleFor(command => command.LeadId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("Crm.Leads.Id.Empty")
            .WithMessage("errors.crm.leads.id.empty");

        RuleFor(command => command.BranchId)
            .Must(id => id is null || !id.Value.IsEmpty)
            .WithErrorCode("Crm.Leads.BranchId.Empty")
            .WithMessage("errors.crm.leads.branchId.empty");

        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Email).EmailAddress().MaximumLength(254)
            .When(command => !string.IsNullOrWhiteSpace(command.Email));
        RuleFor(command => command.Phone).MaximumLength(40);
        RuleFor(command => command.LicenseCategory).NotEmpty().MaximumLength(30);
        RuleFor(command => command.Transmission).IsInEnum();
        RuleFor(command => command.PreferredLocation).MaximumLength(200);
        RuleFor(command => command.SourceType).IsInEnum();
        RuleFor(command => command.SourceDetail).MaximumLength(250);
        RuleFor(command => command.SourceDetail).NotEmpty()
            .When(command => command.SourceType == LeadSourceType.Other);
    }
}
