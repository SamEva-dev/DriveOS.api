using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.CalendarResources;

public sealed class CreateCalendarResourceCommandHandler(ICalendarResourceRepository repository, ISchedulingCapacityUnitOfWork unitOfWork) : ICommandHandler<CreateCalendarResourceCommand, CalendarResourceId>
{
    public async Task<Result<CalendarResourceId>> Handle(CreateCalendarResourceCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(CalendarResourceType), command.ResourceType))
            return Result.Failure<CalendarResourceId>(CalendarResourceErrors.InvalidType);
        var type = (CalendarResourceType)command.ResourceType;
        if (await repository.ExistsByExternalReferenceAsync(command.OrganizationId, type, command.ExternalResourceId, cancellationToken))
            return Result.Failure<CalendarResourceId>(CalendarResourceApplicationErrors.Duplicate);
        var id = CalendarResourceId.New();
        Result<CalendarResource> created = CalendarResource.Create(id, command.OrganizationId, command.BranchId, type, command.ExternalResourceId, command.DisplayName, command.Capacity, command.TimeZoneId);
        if (created.IsFailure) return Result.Failure<CalendarResourceId>(created.Error);
        repository.Add(created.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(id);
    }
}

public abstract class CalendarResourceMutationHandler(ICalendarResourceRepository repository, ISchedulingCapacityUnitOfWork unitOfWork)
{
    protected async Task<Result> Mutate(OrganizationId organizationId, CalendarResourceId id, Func<CalendarResource, Result> mutation, CancellationToken cancellationToken)
    {
        CalendarResource? resource = await repository.GetByIdForUpdateAsync(id, organizationId, cancellationToken);
        if (resource is null) return Result.Failure(CalendarResourceApplicationErrors.NotFound);
        Result result = mutation(resource);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdateCalendarResourceCommandHandler(ICalendarResourceRepository r, ISchedulingCapacityUnitOfWork u) : CalendarResourceMutationHandler(r,u), ICommandHandler<UpdateCalendarResourceCommand>
{ public Task<Result> Handle(UpdateCalendarResourceCommand c, CancellationToken ct) => Mutate(c.OrganizationId,c.Id,x=>x.UpdateMetadata(c.BranchId,c.DisplayName,c.Capacity,c.TimeZoneId),ct); }
public sealed class RestrictCalendarResourceCommandHandler(ICalendarResourceRepository r, ISchedulingCapacityUnitOfWork u) : CalendarResourceMutationHandler(r,u), ICommandHandler<RestrictCalendarResourceCommand>
{ public Task<Result> Handle(RestrictCalendarResourceCommand c, CancellationToken ct) => Mutate(c.OrganizationId,c.Id,x=>x.Restrict(c.Reason),ct); }
public sealed class MarkCalendarResourceUnavailableCommandHandler(ICalendarResourceRepository r, ISchedulingCapacityUnitOfWork u) : CalendarResourceMutationHandler(r,u), ICommandHandler<MarkCalendarResourceUnavailableCommand>
{ public Task<Result> Handle(MarkCalendarResourceUnavailableCommand c, CancellationToken ct) => Mutate(c.OrganizationId,c.Id,x=>x.MarkUnavailable(c.Reason),ct); }
public sealed class ActivateCalendarResourceCommandHandler(ICalendarResourceRepository r, ISchedulingCapacityUnitOfWork u) : CalendarResourceMutationHandler(r,u), ICommandHandler<ActivateCalendarResourceCommand>
{ public Task<Result> Handle(ActivateCalendarResourceCommand c, CancellationToken ct) => Mutate(c.OrganizationId,c.Id,x=>x.Activate(),ct); }
public sealed class ArchiveCalendarResourceCommandHandler(ICalendarResourceRepository r, ISchedulingCapacityUnitOfWork u) : CalendarResourceMutationHandler(r,u), ICommandHandler<ArchiveCalendarResourceCommand>
{ public Task<Result> Handle(ArchiveCalendarResourceCommand c, CancellationToken ct) => Mutate(c.OrganizationId,c.Id,x=>x.Archive(),ct); }
