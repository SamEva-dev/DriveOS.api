using DriveOS.Application.Abstractions.Pagination;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application
    .Organizations.GetOrganizations;

public sealed class GetOrganizationsQueryValidator
    : AbstractValidator<GetOrganizationsQuery>
{
    public GetOrganizationsQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode(
                "Pagination.PageNumber.Invalid")
            .WithMessage(
                "errors.pagination.pageNumber.invalid");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(
                1,
                PaginationParameters.MaximumPageSize)
            .WithErrorCode(
                "Pagination.PageSize.Invalid")
            .WithMessage(
                "errors.pagination.pageSize.invalid");

        RuleFor(query => query.Search)
            .MaximumLength(200)
            .When(query =>
                !string.IsNullOrWhiteSpace(
                    query.Search))
            .WithErrorCode(
                "Organizations.Search.TooLong")
            .WithMessage(
                "errors.organizations.search.tooLong");

        RuleFor(query => query.SortBy)
            .IsInEnum()
            .WithErrorCode(
                "Organizations.Sort.Invalid")
            .WithMessage(
                "errors.organizations.sort.invalid");

        RuleFor(query => query.SortDirection)
            .IsInEnum()
            .WithErrorCode(
                "Organizations.SortDirection.Invalid")
            .WithMessage(
                "errors.organizations.sortDirection.invalid");
    }
}