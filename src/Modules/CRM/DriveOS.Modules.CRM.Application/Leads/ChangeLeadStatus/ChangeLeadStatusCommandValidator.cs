using DriveOS.Modules.CRM.Domain.Leads;
using FluentValidation;

namespace DriveOS.Modules.CRM.Application.Leads.ChangeLeadStatus;

public sealed class ChangeLeadStatusCommandValidator : AbstractValidator<ChangeLeadStatusCommand>
{
    public ChangeLeadStatusCommandValidator()
    {
        RuleFor(command => command.OrganizationId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("Crm.Leads.OrganizationId.Empty")
            .WithMessage("errors.crm.leads.organizationId.empty");

        RuleFor(command => command.LeadId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("Crm.Leads.Id.Empty")
            .WithMessage("errors.crm.leads.id.empty");

        RuleFor(command => command.TargetStatus)
            .IsInEnum()
            .WithErrorCode("Crm.Leads.Status.Invalid")
            .WithMessage("errors.crm.leads.status.invalid");

        RuleFor(command => command.Reason)
            .NotEmpty()
            .When(command => command.TargetStatus == LeadStatus.Lost)
            .WithErrorCode("Crm.Leads.LossReason.Required")
            .WithMessage("errors.crm.leads.lossReason.required");

        RuleFor(command => command.Reason)
            .MaximumLength(500)
            .WithErrorCode("Crm.Leads.StatusReason.TooLong")
            .WithMessage("errors.crm.leads.statusReason.tooLong");
    }
}
