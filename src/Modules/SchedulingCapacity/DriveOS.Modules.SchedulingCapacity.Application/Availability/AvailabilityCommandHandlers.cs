using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Modules.SchedulingCapacity.Domain.Availability;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.Availability;

public sealed class CreateAvailabilityPlanCommandHandler(ICalendarResourceRepository resources, IAvailabilityPlanRepository plans, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<CreateAvailabilityPlanCommand, AvailabilityPlanId>
{
    public async Task<Result<AvailabilityPlanId>> Handle(CreateAvailabilityPlanCommand c, CancellationToken ct)
    {
        if (await resources.GetByIdAsync(c.CalendarResourceId, c.OrganizationId, ct) is null) return Result.Failure<AvailabilityPlanId>(AvailabilityApplicationErrors.ResourceNotFound);
        AvailabilityPlanId id = AvailabilityPlanId.New();
        Result<AvailabilityPlan> created = AvailabilityPlan.Create(id, c.OrganizationId, c.CalendarResourceId, c.EffectiveFrom, c.EffectiveTo);
        if (created.IsFailure) return Result.Failure<AvailabilityPlanId>(created.Error);
        await plans.AddAsync(created.Value, ct);
        await uow.CommitAsync(ct);
        return Result.Success(id);
    }
}

public sealed class AddAvailabilityRuleCommandHandler(IAvailabilityPlanRepository plans, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<AddAvailabilityRuleCommand, AvailabilityRuleId>
{
    public async Task<Result<AvailabilityRuleId>> Handle(AddAvailabilityRuleCommand c, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(AvailabilityRuleType), c.Type)) return Result.Failure<AvailabilityRuleId>(AvailabilityPlanErrors.InvalidRuleType);
        if (!Enum.IsDefined(typeof(AvailabilityExceptionSource), c.Source)) return Result.Failure<AvailabilityRuleId>(AvailabilityPlanErrors.InvalidSource);
        AvailabilityPlan? plan = await plans.GetByIdForUpdateAsync(c.PlanId, c.OrganizationId, ct);
        if (plan is null) return Result.Failure<AvailabilityRuleId>(AvailabilityApplicationErrors.PlanNotFound);
        AvailabilityRuleId id = AvailabilityRuleId.New();
        Result<AvailabilityRuleId> result = plan.AddRecurringRule(
            id,
            c.DayOfWeek,
            c.StartTime,
            c.EndTime,
            c.Capacity,
            (AvailabilityRuleType)c.Type,
            (AvailabilityExceptionSource)c.Source,
            c.Priority,
            c.BranchId,
            c.TrainingCategory,
            c.ServiceArea);
        if (result.IsFailure) return result;
        await uow.CommitAsync(ct);
        return result;
    }
}

public sealed class RemoveAvailabilityRuleCommandHandler(IAvailabilityPlanRepository plans, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<RemoveAvailabilityRuleCommand>
{
    public async Task<Result> Handle(RemoveAvailabilityRuleCommand c, CancellationToken ct)
    {
        AvailabilityPlan? plan = await plans.GetByIdForUpdateAsync(c.PlanId, c.OrganizationId, ct);
        if (plan is null) return Result.Failure(AvailabilityApplicationErrors.PlanNotFound);
        Result result = plan.RemoveRecurringRule(c.RuleId);
        if (result.IsFailure) return result;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class AddAvailabilityExceptionCommandHandler(
    IAvailabilityPlanRepository plans,
    ICalendarResourceRepository resources,
    IAvailabilityImpactAssessmentService impactAssessment,
    IBookingCapacityLock capacityLock,
    ISchedulingCapacityUnitOfWork uow)
    : ICommandHandler<AddAvailabilityExceptionCommand, AddAvailabilityExceptionResult>
{
    public async Task<Result<AddAvailabilityExceptionResult>> Handle(AddAvailabilityExceptionCommand c, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(AvailabilityExceptionType), c.Type))
            return Result.Failure<AddAvailabilityExceptionResult>(AvailabilityPlanErrors.InvalidExceptionType);
        if (c.Source.HasValue && !Enum.IsDefined(typeof(AvailabilityExceptionSource), c.Source.Value))
            return Result.Failure<AddAvailabilityExceptionResult>(AvailabilityPlanErrors.InvalidSource);

        await uow.BeginTransactionAsync(ct);
        try
        {
            AvailabilityPlan? plan = await plans.GetByIdForUpdateAsync(c.PlanId, c.OrganizationId, ct);
            if (plan is null)
            {
                await uow.RollbackTransactionAsync(ct);
                return Result.Failure<AddAvailabilityExceptionResult>(AvailabilityApplicationErrors.PlanNotFound);
            }

            await capacityLock.AcquireAsync(c.OrganizationId, [plan.CalendarResourceId], ct);

            CalendarResource? resource = await resources.GetByIdAsync(plan.CalendarResourceId, c.OrganizationId, ct);
            if (resource is null)
            {
                await uow.RollbackTransactionAsync(ct);
                return Result.Failure<AddAvailabilityExceptionResult>(AvailabilityApplicationErrors.ResourceNotFound);
            }

            AvailabilityExceptionType type = (AvailabilityExceptionType)c.Type;
            AvailabilityExceptionSource? source = c.Source.HasValue ? (AvailabilityExceptionSource)c.Source.Value : null;
            AvailabilityExceptionId id = AvailabilityExceptionId.New();
            Result<AvailabilityExceptionId> result = plan.AddException(
                id,
                c.Date,
                c.StartTime,
                c.EndTime,
                type,
                c.Capacity,
                c.Reason,
                source,
                c.Priority);
            if (result.IsFailure)
            {
                await uow.RollbackTransactionAsync(ct);
                return Result.Failure<AddAvailabilityExceptionResult>(result.Error);
            }

            AvailabilityExceptionSource resolvedSource = source ?? AvailabilityExceptionPolicy.ResolveSource(type);
            IReadOnlyCollection<ImpactedBookingResponse> impactedBookings = AvailabilityExceptionPolicy.IsUnavailable(type)
                ? await impactAssessment.FindImpactedBookingsAsync(
                    c.OrganizationId,
                    plan.CalendarResourceId,
                    c.Date,
                    c.StartTime,
                    c.EndTime,
                    resource.TimeZoneId,
                    ct)
                : [];

            await uow.CommitTransactionAsync(ct);

            return Result.Success(new AddAvailabilityExceptionResult(
                id.Value,
                resolvedSource.ToString(),
                impactedBookings));
        }
        catch
        {
            if (uow.HasActiveTransaction)
                await uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}

public sealed class RemoveAvailabilityExceptionCommandHandler(IAvailabilityPlanRepository plans, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<RemoveAvailabilityExceptionCommand>
{
    public async Task<Result> Handle(RemoveAvailabilityExceptionCommand c, CancellationToken ct)
    {
        AvailabilityPlan? plan = await plans.GetByIdForUpdateAsync(c.PlanId, c.OrganizationId, ct);
        if (plan is null) return Result.Failure(AvailabilityApplicationErrors.PlanNotFound);
        Result result = plan.RemoveException(c.ExceptionId);
        if (result.IsFailure) return result;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class UpdateAvailabilityPreferencesCommandHandler(IAvailabilityPlanRepository plans, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<UpdateAvailabilityPreferencesCommand>
{
    public async Task<Result> Handle(UpdateAvailabilityPreferencesCommand c, CancellationToken ct)
    {
        AvailabilityPlan? plan = await plans.GetByIdForUpdateAsync(c.PlanId, c.OrganizationId, ct);
        if (plan is null) return Result.Failure(AvailabilityApplicationErrors.PlanNotFound);
        Result result = plan.UpdatePreferences(
            c.PreferredMeetingPoint,
            c.MaximumTravelDistanceKm,
            c.MinimumNoticeMinutes,
            c.TrainingFrequencyPerWeek,
            c.PreferredInstructorId,
            c.IntensiveRhythm,
            c.OneTimeGeolocationAllowed);
        if (result.IsFailure) return result;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class ActivateAvailabilityPlanCommandHandler(IAvailabilityPlanRepository plans, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<ActivateAvailabilityPlanCommand>
{
    public async Task<Result> Handle(ActivateAvailabilityPlanCommand c, CancellationToken ct)
    {
        AvailabilityPlan? p = await plans.GetByIdForUpdateAsync(c.PlanId, c.OrganizationId, ct);
        if (p is null) return Result.Failure(AvailabilityApplicationErrors.PlanNotFound);
        Result r = p.Activate();
        if (r.IsFailure) return r;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class ArchiveAvailabilityPlanCommandHandler(IAvailabilityPlanRepository plans, ISchedulingCapacityUnitOfWork uow) : ICommandHandler<ArchiveAvailabilityPlanCommand>
{
    public async Task<Result> Handle(ArchiveAvailabilityPlanCommand c, CancellationToken ct)
    {
        AvailabilityPlan? p = await plans.GetByIdForUpdateAsync(c.PlanId, c.OrganizationId, ct);
        if (p is null) return Result.Failure(AvailabilityApplicationErrors.PlanNotFound);
        Result r = p.Archive();
        if (r.IsFailure) return r;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
