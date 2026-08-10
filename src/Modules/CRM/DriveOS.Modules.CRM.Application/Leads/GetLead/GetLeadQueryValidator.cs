using FluentValidation;

namespace DriveOS.Modules.CRM.Application.Leads.GetLead;

public sealed class GetLeadQueryValidator : AbstractValidator<GetLeadQuery>
{
    public GetLeadQueryValidator()
    {
        RuleFor(query => query.OrganizationId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("Crm.Leads.OrganizationId.Empty")
            .WithMessage("errors.crm.leads.organizationId.empty");

        RuleFor(query => query.LeadId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("Crm.Leads.Id.Empty")
            .WithMessage("errors.crm.leads.id.empty");
    }
}
