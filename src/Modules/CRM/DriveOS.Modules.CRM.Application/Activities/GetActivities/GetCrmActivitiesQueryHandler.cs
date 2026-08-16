using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Activities.GetActivities;

public sealed class GetLeadActivitiesQueryHandler(ICrmActivityRepository repository)
    : IQueryHandler<GetLeadActivitiesQuery, IReadOnlyList<CrmActivityResponse>>
{
    public async Task<Result<IReadOnlyList<CrmActivityResponse>>> Handle(
        GetLeadActivitiesQuery query,
        CancellationToken ct
    ) =>
        Result.Success<IReadOnlyList<CrmActivityResponse>>(
            Map(
                await repository.GetByLeadAsync(query.OrganizationId, query.LeadId, ct),
                query.Scope
            )
        );

    internal static IReadOnlyList<CrmActivityResponse> Map(
        IEnumerable<CrmActivity> activities,
        CrmActivityReadScope scope
    ) =>
        activities
            .Where(x =>
                x.LeadId.HasValue
                && (scope == CrmActivityReadScope.IncludeInternal || !x.Metadata.IsInternal)
            )
            .Select(x => new CrmActivityResponse(
                x.Id.Value,
                x.LeadId!.Value.Value,
                x.Type.ToString(),
                x.Direction.ToString(),
                x.Subject,
                x.Details,
                x.OccurredAtUtc,
                x.CreatedAtUtc,
                x.CreatedByUserId?.Value
            ))
            .ToArray();
}

public sealed class GetRecentActivitiesQueryHandler(ICrmActivityRepository repository)
    : IQueryHandler<GetRecentActivitiesQuery, IReadOnlyList<CrmActivityResponse>>
{
    public async Task<Result<IReadOnlyList<CrmActivityResponse>>> Handle(
        GetRecentActivitiesQuery query,
        CancellationToken ct
    ) =>
        Result.Success(
            GetLeadActivitiesQueryHandler.Map(
                await repository.GetRecentAsync(
                    query.OrganizationId,
                    Math.Clamp(query.Limit, 1, 200),
                    ct
                ),
                query.Scope
            )
        );
}
