using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.Recurrences;

public static class RecurrenceApplicationErrors
{
    public static readonly Error NotFound = Error.NotFound("SchedulingCapacity.Recurrence.NotFound", "scheduling.recurrence.notFound");
    public static readonly Error ResourceNotFound = Error.NotFound("SchedulingCapacity.Recurrence.ResourceNotFound", "scheduling.recurrence.resourceNotFound");
}

public sealed class CreateRecurrenceSeriesCommandHandler(IRecurrenceSeriesRepository repository, ICalendarResourceRepository resources, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<CreateRecurrenceSeriesCommand, RecurrenceSeriesId>
{
    public async Task<Result<RecurrenceSeriesId>> Handle(CreateRecurrenceSeriesCommand c, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(RecurrenceTargetType), c.TargetType) || !Enum.IsDefined(typeof(RecurrenceFrequency), c.Frequency) || !Enum.IsDefined(typeof(ResourceSelectionPolicy), c.ResourceSelectionPolicy))
            return Result.Failure<RecurrenceSeriesId>(RecurrenceSeriesErrors.InvalidRule);

        if ((ResourceSelectionPolicy)c.ResourceSelectionPolicy == ResourceSelectionPolicy.FixedResources && c.Resources.Count == 0)
            return Result.Failure<RecurrenceSeriesId>(RecurrenceSeriesErrors.ResourceRequired);

        foreach (CreateRecurrenceResourceRequest requested in c.Resources)
        {
            if (await resources.GetByIdAsync(new CalendarResourceId(requested.CalendarResourceId), c.OrganizationId, ct) is null)
                return Result.Failure<RecurrenceSeriesId>(RecurrenceApplicationErrors.ResourceNotFound);
        }

        RecurrenceSeriesId id = RecurrenceSeriesId.New();
        Result<RecurrenceSeries> created = RecurrenceSeries.Create(id, c.OrganizationId, c.BranchId, (RecurrenceTargetType)c.TargetType, (RecurrenceFrequency)c.Frequency, c.Interval, c.StartDate, c.EndDate, c.OccurrenceCount, c.DaysOfWeek, c.LocalTime, c.DurationMinutes, c.TimeZoneId, c.Title, (ResourceSelectionPolicy)c.ResourceSelectionPolicy);
        if (created.IsFailure) return Result.Failure<RecurrenceSeriesId>(created.Error);

        foreach (CreateRecurrenceResourceRequest requested in c.Resources)
        {
            Result added = created.Value.AddResource(RecurrenceResourceId.New(), new CalendarResourceId(requested.CalendarResourceId), requested.Quantity);
            if (added.IsFailure) return Result.Failure<RecurrenceSeriesId>(added.Error);
        }

        Result<int> generated = created.Value.GenerateOccurrences();
        if (generated.IsFailure) return Result.Failure<RecurrenceSeriesId>(generated.Error);
        repository.Add(created.Value);
        await uow.CommitAsync(ct);
        return Result.Success(id);
    }
}

public sealed class GenerateRecurrenceOccurrencesCommandHandler(IRecurrenceSeriesRepository repository, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<GenerateRecurrenceOccurrencesCommand, int>
{
    public async Task<Result<int>> Handle(GenerateRecurrenceOccurrencesCommand c, CancellationToken ct)
    {
        RecurrenceSeries? series = await repository.GetByIdForUpdateAsync(c.SeriesId, c.OrganizationId, ct);
        if (series is null) return Result.Failure<int>(RecurrenceApplicationErrors.NotFound);
        Result<int> result = series.GenerateOccurrences();
        if (result.IsFailure) return result;
        await uow.CommitAsync(ct);
        return result;
    }
}

public sealed class CancelRecurrenceOccurrenceCommandHandler(IRecurrenceSeriesRepository repository, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<CancelRecurrenceOccurrenceCommand>
{
    public async Task<Result> Handle(CancelRecurrenceOccurrenceCommand c, CancellationToken ct) { RecurrenceSeries? s=await repository.GetByIdForUpdateAsync(c.SeriesId,c.OrganizationId,ct); if(s is null)return Result.Failure(RecurrenceApplicationErrors.NotFound); Result r=s.CancelOccurrence(c.OccurrenceId,c.Reason); if(r.IsFailure)return r; await uow.CommitAsync(ct); return Result.Success(); }
}
public sealed class RescheduleRecurrenceOccurrenceCommandHandler(IRecurrenceSeriesRepository repository, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<RescheduleRecurrenceOccurrenceCommand>
{
    public async Task<Result> Handle(RescheduleRecurrenceOccurrenceCommand c, CancellationToken ct) { RecurrenceSeries? s=await repository.GetByIdForUpdateAsync(c.SeriesId,c.OrganizationId,ct); if(s is null)return Result.Failure(RecurrenceApplicationErrors.NotFound); Result r=s.RescheduleOccurrence(c.OccurrenceId,c.StartAtUtc,c.EndAtUtc,c.Reason); if(r.IsFailure)return r; await uow.CommitAsync(ct); return Result.Success(); }
}
public sealed class ChangeFutureRecurrenceRuleCommandHandler(IRecurrenceSeriesRepository repository, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<ChangeFutureRecurrenceRuleCommand>
{
    public async Task<Result> Handle(ChangeFutureRecurrenceRuleCommand c, CancellationToken ct) { RecurrenceSeries? s=await repository.GetByIdForUpdateAsync(c.SeriesId,c.OrganizationId,ct); if(s is null)return Result.Failure(RecurrenceApplicationErrors.NotFound); if(!Enum.IsDefined(typeof(RecurrenceFrequency),c.Frequency))return Result.Failure(RecurrenceSeriesErrors.InvalidRule); Result r=s.ChangeFutureRule(c.ApplyFrom,(RecurrenceFrequency)c.Frequency,c.Interval,c.EndDate,c.OccurrenceCount,c.DaysOfWeek,c.LocalTime,c.DurationMinutes); if(r.IsFailure)return r; await uow.CommitAsync(ct); return Result.Success(); }
}
public sealed class CancelRecurrenceSeriesCommandHandler(IRecurrenceSeriesRepository repository, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<CancelRecurrenceSeriesCommand>
{
    public async Task<Result> Handle(CancelRecurrenceSeriesCommand c, CancellationToken ct) { RecurrenceSeries? s=await repository.GetByIdForUpdateAsync(c.SeriesId,c.OrganizationId,ct); if(s is null)return Result.Failure(RecurrenceApplicationErrors.NotFound); Result r=s.CancelSeries(c.Reason); if(r.IsFailure)return r; await uow.CommitAsync(ct); return Result.Success(); }
}
