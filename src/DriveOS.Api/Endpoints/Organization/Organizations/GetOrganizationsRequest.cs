namespace DriveOS.Api.Endpoints.Organization.Organizations;

public sealed class GetOrganizationsRequest
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string? Search { get; init; }

    public string? SortBy { get; init; } =
        "legalName";

    public string? SortDirection { get; init; } =
        "asc";
}