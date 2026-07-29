using DriveOS.SharedKernel.Results;

namespace DriveOS.Application.Abstractions.Pagination;

public static class PaginationErrors
{
    public static readonly Error InvalidPageNumber =
        Error.Validation(
            code: "Pagination.PageNumber.Invalid",
            messageKey: "errors.pagination.pageNumber.invalid");

    public static Error InvalidPageSize(
        int maximumPageSize) =>
        Error.Validation(
            code: "Pagination.PageSize.Invalid",
            messageKey: "errors.pagination.pageSize.invalid",
            parameters: new Dictionary<string, object?>
            {
                ["maximumPageSize"] = maximumPageSize
            });
}