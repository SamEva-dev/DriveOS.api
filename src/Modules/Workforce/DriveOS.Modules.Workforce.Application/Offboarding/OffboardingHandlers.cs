using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Workforce.Application.Persistence;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.Offboarding;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Application.Offboarding;

internal static class OffboardingMapping
{
    public static OffboardingResponse Map(OffboardingProcess x)=>new(x.Id.Value,x.EmployeeId.Value,x.PlannedEndDate,x.Reason,x.Status.ToString(),x.CompletedAtUtc,x.CompletedByUserId?.Value,x.Items.OrderBy(i=>(int)i.Kind).Select(i=>new OffboardingChecklistItemResponse(i.Id.Value,i.Kind.ToString(),i.IsAutomatic,i.Status.ToString(),i.BlockerCount,i.Note,i.ResolvedAtUtc,i.ResolvedByUserId?.Value,i.WaiverReason,i.LastEvaluatedAtUtc)).ToArray());
}
public sealed class GetEmployeeOffboardingQueryHandler(IOffboardingProcessRepository repository):IQueryHandler<GetEmployeeOffboardingQuery,OffboardingResponse>
{
    public async Task<Result<OffboardingResponse>> Handle(GetEmployeeOffboardingQuery q,CancellationToken ct){var x=await repository.FindCurrentByEmployeeAsync(q.OrganizationId,q.EmployeeId,false,ct);return x is null?Result.Failure<OffboardingResponse>(OffboardingErrors.NotFound):Result.Success(OffboardingMapping.Map(x));}
}
public sealed class RefreshOffboardingCommandHandler(IOffboardingProcessRepository repository,IOffboardingDependencyReadService dependencies,IWorkforceUnitOfWork uow,IClock clock):ICommandHandler<RefreshOffboardingCommand>
{
    public async Task<Result> Handle(RefreshOffboardingCommand c,CancellationToken ct)
    {
        var x=await repository.FindCurrentByEmployeeAsync(c.OrganizationId,c.EmployeeId,true,ct);if(x is null)return Result.Failure(OffboardingErrors.NotFound);
        var d=await dependencies.GetAsync(c.OrganizationId,c.EmployeeId,x.PlannedEndDate,ct);var now=clock.UtcNow;
        foreach(var (kind,count,note) in new[]{
            (OffboardingChecklistItemKind.BranchAssignmentsClosed,d.BranchAssignments,"offboarding.blockers.branchAssignments"),
            (OffboardingChecklistItemKind.JobPositionsClosed,d.JobPositions,"offboarding.blockers.jobPositions"),
            (OffboardingChecklistItemKind.EmploymentContractsClosed,d.EmploymentContracts,"offboarding.blockers.employmentContracts"),
            (OffboardingChecklistItemKind.EquipmentReturned,d.EquipmentAssignments,"offboarding.blockers.equipmentAssignments"),
            (OffboardingChecklistItemKind.TimesheetsFinalized,d.Timesheets,"offboarding.blockers.timesheets"),
            (OffboardingChecklistItemKind.ProfessionalRestrictionsReviewed,d.ProfessionalRestrictions,"offboarding.blockers.professionalRestrictions")})
        {var r=x.SynchronizeAutomaticItem(kind,count,count==0?null:note,now,c.ActorUserId);if(r.IsFailure)return r;}
        await uow.CommitAsync(ct);return Result.Success();
    }
}
public sealed class CompleteOffboardingChecklistItemCommandHandler(IOffboardingProcessRepository repository,IWorkforceUnitOfWork uow,IClock clock):ICommandHandler<CompleteOffboardingChecklistItemCommand>
{
    public async Task<Result> Handle(CompleteOffboardingChecklistItemCommand c,CancellationToken ct){var x=await repository.FindCurrentByEmployeeAsync(c.OrganizationId,c.EmployeeId,true,ct);if(x is null)return Result.Failure(OffboardingErrors.NotFound);var r=x.CompleteManualItem(c.Kind,c.Note,clock.UtcNow,c.ActorUserId);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}
}
public sealed class WaiveOffboardingChecklistItemCommandHandler(IOffboardingProcessRepository repository,IWorkforceUnitOfWork uow,IClock clock):ICommandHandler<WaiveOffboardingChecklistItemCommand>
{
    public async Task<Result> Handle(WaiveOffboardingChecklistItemCommand c,CancellationToken ct){var x=await repository.FindCurrentByEmployeeAsync(c.OrganizationId,c.EmployeeId,true,ct);if(x is null)return Result.Failure(OffboardingErrors.NotFound);var r=x.WaiveItem(c.Kind,c.Reason,clock.UtcNow,c.ActorUserId);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}
}
public sealed class CompleteOffboardingCommandHandler(IOffboardingProcessRepository repository,IEmployeeRepository employees,IWorkforceUnitOfWork uow,IClock clock):ICommandHandler<CompleteOffboardingCommand>
{
    public async Task<Result> Handle(CompleteOffboardingCommand c,CancellationToken ct)
    {
        var process=await repository.FindCurrentByEmployeeAsync(c.OrganizationId,c.EmployeeId,true,ct);if(process is null)return Result.Failure(OffboardingErrors.NotFound);
        var employee=await employees.GetByIdForUpdateAsync(c.OrganizationId,c.EmployeeId,ct);if(employee is null)return Result.Failure(EmployeeErrors.NotFound);
        var now=clock.UtcNow;var end=employee.EndEmployment(process.PlannedEndDate,c.CompletionReason,now,c.ActorUserId);if(end.IsFailure)return end;var done=process.Complete(now,c.ActorUserId,employee.UserId);if(done.IsFailure)return done;await uow.CommitAsync(ct);return Result.Success();
    }
}
