using DriveOS.Application.Abstractions.Pagination;
using FluentValidation;

namespace DriveOS.Modules.CRM.Application.Leads.GetLeads;

public sealed class GetLeadsQueryValidator : AbstractValidator<GetLeadsQuery>
{
    public GetLeadsQueryValidator()
    {
        RuleFor(query => query.OrganizationId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("Crm.Leads.OrganizationId.Empty")
            .WithMessage("errors.crm.leads.organizationId.empty");

        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("Pagination.PageNumber.Invalid")
            .WithMessage("errors.pagination.pageNumber.invalid");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, PaginationParameters.MaximumPageSize)
            .WithErrorCode("Pagination.PageSize.Invalid")
            .WithMessage("errors.pagination.pageSize.invalid");

        RuleFor(query => query.Search)
            .MaximumLength(200)
            .When(query => !string.IsNullOrWhiteSpace(query.Search))
            .WithErrorCode("Crm.Leads.Search.TooLong")
            .WithMessage("errors.crm.leads.search.tooLong");

        RuleFor(query => query.Status)
            .Must(status => !status.HasValue || Enum.IsDefined(status.Value))
            .WithErrorCode("Crm.Leads.Status.Invalid")
            .WithMessage("errors.crm.leads.status.invalid");

        RuleFor(query => query.SourceType)
            .Must(source => !source.HasValue || Enum.IsDefined(source.Value))
            .WithErrorCode("Crm.Leads.Source.Invalid")
            .WithMessage("errors.crm.leads.source.invalid");

        RuleFor(query => query.SortBy)
            .IsInEnum()
            .WithErrorCode("Crm.Leads.Sort.Invalid")
            .WithMessage("errors.crm.leads.sort.invalid");

        RuleFor(query => query.SortDirection)
            .IsInEnum()
            .WithErrorCode("Crm.Leads.SortDirection.Invalid")
            .WithMessage("errors.crm.leads.sortDirection.invalid");

        RuleFor(query => query)
            .Must(query => !query.UnassignedOnly || query.AssignedAdvisorId is null)
            .WithErrorCode("Crm.Leads.AdvisorFilter.Conflicting")
            .WithMessage("errors.crm.leads.advisorFilter.conflicting");
    }
}
