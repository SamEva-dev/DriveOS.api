using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Tasks;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Tasks.GetLeadTasks;

public sealed class GetLeadTasksQueryHandler(ICrmTaskRepository repository)
    : IQueryHandler<GetLeadTasksQuery, IReadOnlyList<CrmTaskResponse>>
{
    public async Task<Result<IReadOnlyList<CrmTaskResponse>>> Handle(
        GetLeadTasksQuery query,
        CancellationToken ct
    )
    {
        IReadOnlyList<CrmTask> tasks = await repository.GetByLeadAsync(
            query.OrganizationId,
            query.LeadId,
            ct
        );
        return Result.Success<IReadOnlyList<CrmTaskResponse>>(
            tasks
                .Select(x => new CrmTaskResponse(
                    x.Id.Value,
                    x.LeadId.Value,
                    x.Type.ToString(),
                    x.Title,
                    x.Notes,
                    x.DueAtUtc,
                    x.AssignedToUserId?.Value,
                    x.Status.ToString(),
                    x.ClosedAtUtc,
                    x.CreatedAtUtc
                ))
                .ToArray()
        );
    }
}

public sealed class GetPendingTasksQueryHandler(ICrmTaskRepository repository)
    : IQueryHandler<GetPendingTasksQuery, IReadOnlyList<CrmTaskResponse>>
{
    public async Task<Result<IReadOnlyList<CrmTaskResponse>>> Handle(
        GetPendingTasksQuery query,
        CancellationToken ct
    )
    {
        IReadOnlyList<CrmTask> tasks = await repository.GetPendingAsync(query.OrganizationId, ct);
        return Result.Success<IReadOnlyList<CrmTaskResponse>>(
            tasks
                .Select(x => new CrmTaskResponse(
                    x.Id.Value,
                    x.LeadId.Value,
                    x.Type.ToString(),
                    x.Title,
                    x.Notes,
                    x.DueAtUtc,
                    x.AssignedToUserId?.Value,
                    x.Status.ToString(),
                    x.ClosedAtUtc,
                    x.CreatedAtUtc
                ))
                .ToArray()
        );
    }
}
