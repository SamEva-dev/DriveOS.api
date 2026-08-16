using DriveOS.Application.Abstractions.Pagination;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.Branches.GetBranches;

public sealed class GetBranchesQueryValidator : AbstractValidator<GetBranchesQuery>
{
    public GetBranchesQueryValidator()
    {
        RuleFor(query => query.OrganizationId).Must(id => !id.IsEmpty);

        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize).InclusiveBetween(1, PaginationParameters.MaximumPageSize);

        RuleFor(query => query.Search)
            .MaximumLength(200)
            .When(query => !string.IsNullOrWhiteSpace(query.Search));

        RuleFor(query => query.SortBy).IsInEnum();
        RuleFor(query => query.SortDirection).IsInEnum();
    }
}
