using FluentValidation;

namespace DriveOS.Modules.CRM.Application.Leads.ExportLeads;

public sealed class ExportLeadsQueryValidator : AbstractValidator<ExportLeadsQuery>
{
    public ExportLeadsQueryValidator()
    {
        RuleFor(query => query.OrganizationId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("Crm.Leads.OrganizationId.Empty")
            .WithMessage("errors.crm.leads.organizationId.empty");
        RuleFor(query => query.Search)
            .MaximumLength(200)
            .When(query => !string.IsNullOrWhiteSpace(query.Search))
            .WithErrorCode("Crm.Leads.Search.TooLong")
            .WithMessage("errors.crm.leads.search.tooLong");
        RuleFor(query => query)
            .Must(query => !query.UnassignedOnly || query.AssignedAdvisorId is null)
            .WithErrorCode("Crm.Leads.AdvisorFilter.Conflicting")
            .WithMessage("errors.crm.leads.advisorFilter.conflicting");
    }
}
