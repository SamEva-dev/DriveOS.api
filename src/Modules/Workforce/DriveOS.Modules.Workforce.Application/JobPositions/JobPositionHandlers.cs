using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Workforce.Application.Persistence;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Application.JobPositions;

public sealed class CreateJobPositionCommandHandler(IJobPositionRepository positions, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<CreateJobPositionCommand, JobPositionId>
{
    public async Task<Result<JobPositionId>> Handle(CreateJobPositionCommand command, CancellationToken ct)
    {
        if (await positions.FindByCodeAsync(command.OrganizationId, command.Code, ct) is not null) return Result.Failure<JobPositionId>(JobPositionErrors.DuplicateCode);
        DateTimeOffset now = clock.UtcNow;
        Result<JobPosition> created = JobPosition.Create(command.JobPositionId, command.OrganizationId, command.Code, command.Name, command.Description, command.ProfessionalFunction, now);
        if (created.IsFailure) return Result.Failure<JobPositionId>(created.Error);
        created.Value.SetCreatedAudit(now, command.ActorUserId);
        positions.Add(created.Value); await unitOfWork.CommitAsync(ct); return Result.Success(created.Value.Id);
    }
}
public sealed class UpdateJobPositionCommandHandler(IJobPositionRepository positions, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<UpdateJobPositionCommand>
{
    public async Task<Result> Handle(UpdateJobPositionCommand command, CancellationToken ct)
    {
        JobPosition? position = await positions.GetByIdForUpdateAsync(command.OrganizationId, command.JobPositionId, ct); if (position is null) return Result.Failure(JobPositionErrors.NotFound);
        JobPosition? byCode = await positions.FindByCodeAsync(command.OrganizationId, command.Code, ct); if (byCode is not null && byCode.Id != command.JobPositionId) return Result.Failure(JobPositionErrors.DuplicateCode);
        Result r = position.Update(command.Code, command.Name, command.Description, command.ProfessionalFunction, clock.UtcNow, command.ActorUserId); if (r.IsFailure) return r;
        await uow.CommitAsync(ct); return Result.Success();
    }
}
public sealed class DeactivateJobPositionCommandHandler(IJobPositionRepository positions, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<DeactivateJobPositionCommand>
{
    public async Task<Result> Handle(DeactivateJobPositionCommand c, CancellationToken ct) { JobPosition? p = await positions.GetByIdForUpdateAsync(c.OrganizationId, c.JobPositionId, ct); if (p is null) return Result.Failure(JobPositionErrors.NotFound); Result r=p.Deactivate(clock.UtcNow,c.ActorUserId); if(r.IsFailure)return r; await uow.CommitAsync(ct); return Result.Success(); }
}
public sealed class ReactivateJobPositionCommandHandler(IJobPositionRepository positions, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<ReactivateJobPositionCommand>
{
    public async Task<Result> Handle(ReactivateJobPositionCommand c, CancellationToken ct) { JobPosition? p = await positions.GetByIdForUpdateAsync(c.OrganizationId, c.JobPositionId, ct); if (p is null) return Result.Failure(JobPositionErrors.NotFound); Result r=p.Reactivate(clock.UtcNow,c.ActorUserId); if(r.IsFailure)return r; await uow.CommitAsync(ct); return Result.Success(); }
}
public sealed class GetJobPositionQueryHandler(IJobPositionRepository positions) : IQueryHandler<GetJobPositionQuery, JobPositionResponse>
{
    public async Task<Result<JobPositionResponse>> Handle(GetJobPositionQuery q, CancellationToken ct) { JobPosition? p=await positions.GetByIdAsync(q.OrganizationId,q.JobPositionId,ct); return p is null?Result.Failure<JobPositionResponse>(JobPositionErrors.NotFound):Result.Success(Map(p)); }
    internal static JobPositionResponse Map(JobPosition p)=>new(p.Id.Value,p.Code,p.Name,p.Description,p.ProfessionalFunction.ToString(),p.Status.ToString(),p.CreatedAtUtc,p.LastModifiedAtUtc);
}
public sealed class GetJobPositionsQueryHandler(IJobPositionRepository positions) : IQueryHandler<GetJobPositionsQuery, IReadOnlyList<JobPositionResponse>>
{
    public async Task<Result<IReadOnlyList<JobPositionResponse>>> Handle(GetJobPositionsQuery q, CancellationToken ct)=>Result.Success<IReadOnlyList<JobPositionResponse>>((await positions.ListAsync(q.OrganizationId,q.Status,ct)).Select(GetJobPositionQueryHandler.Map).ToArray());
}

public sealed class AddEmployeeJobPositionAssignmentCommandHandler(IEmployeeRepository employees, IJobPositionRepository positions, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<AddEmployeeJobPositionAssignmentCommand, EmployeeJobPositionAssignmentId>
{
    public async Task<Result<EmployeeJobPositionAssignmentId>> Handle(AddEmployeeJobPositionAssignmentCommand c, CancellationToken ct)
    {
        Employee? employee=await employees.GetByIdForUpdateAsync(c.OrganizationId,c.EmployeeId,ct); if(employee is null)return Result.Failure<EmployeeJobPositionAssignmentId>(EmployeeErrors.NotFound);
        JobPosition? position=await positions.GetByIdAsync(c.OrganizationId,c.JobPositionId,ct); if(position is null)return Result.Failure<EmployeeJobPositionAssignmentId>(JobPositionErrors.NotFound); if(position.Status!=JobPositionStatus.Active)return Result.Failure<EmployeeJobPositionAssignmentId>(JobPositionErrors.Inactive);
        DateTimeOffset now=clock.UtcNow; Result<EmployeeJobPositionAssignmentId> r=employee.AddJobPositionAssignment(c.AssignmentId,c.JobPositionId,c.BranchId,c.StartDate,c.EndDate,c.IsPrimary,DateOnly.FromDateTime(now.UtcDateTime),now,c.ActorUserId); if(r.IsFailure)return r; await uow.CommitAsync(ct); return r;
    }
}
public sealed class UpdateEmployeeJobPositionAssignmentCommandHandler(IEmployeeRepository employees, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<UpdateEmployeeJobPositionAssignmentCommand>
{
    public async Task<Result> Handle(UpdateEmployeeJobPositionAssignmentCommand c, CancellationToken ct){Employee? e=await employees.GetByIdForUpdateAsync(c.OrganizationId,c.EmployeeId,ct);if(e is null)return Result.Failure(EmployeeErrors.NotFound);DateTimeOffset now=clock.UtcNow;Result r=e.UpdateJobPositionAssignment(c.AssignmentId,c.StartDate,c.EndDate,c.IsPrimary,DateOnly.FromDateTime(now.UtcDateTime),now,c.ActorUserId);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}
}
public sealed class EndEmployeeJobPositionAssignmentCommandHandler(IEmployeeRepository employees, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<EndEmployeeJobPositionAssignmentCommand>
{
    public async Task<Result> Handle(EndEmployeeJobPositionAssignmentCommand c,CancellationToken ct){Employee? e=await employees.GetByIdForUpdateAsync(c.OrganizationId,c.EmployeeId,ct);if(e is null)return Result.Failure(EmployeeErrors.NotFound);Result r=e.EndJobPositionAssignment(c.AssignmentId,c.EndDate,clock.UtcNow,c.ActorUserId);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}
}
public sealed class CancelEmployeeJobPositionAssignmentCommandHandler(IEmployeeRepository employees, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<CancelEmployeeJobPositionAssignmentCommand>
{
    public async Task<Result> Handle(CancelEmployeeJobPositionAssignmentCommand c,CancellationToken ct){Employee? e=await employees.GetByIdForUpdateAsync(c.OrganizationId,c.EmployeeId,ct);if(e is null)return Result.Failure(EmployeeErrors.NotFound);Result r=e.CancelJobPositionAssignment(c.AssignmentId,clock.UtcNow,c.ActorUserId);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}
}
public sealed class GetEmployeeJobPositionAssignmentsQueryHandler(IEmployeeRepository employees) : IQueryHandler<GetEmployeeJobPositionAssignmentsQuery, IReadOnlyList<EmployeeJobPositionAssignmentResponse>>
{
    public async Task<Result<IReadOnlyList<EmployeeJobPositionAssignmentResponse>>> Handle(GetEmployeeJobPositionAssignmentsQuery q,CancellationToken ct){Employee? e=await employees.GetByIdAsync(q.OrganizationId,q.EmployeeId,ct);if(e is null)return Result.Failure<IReadOnlyList<EmployeeJobPositionAssignmentResponse>>(EmployeeErrors.NotFound);return Result.Success<IReadOnlyList<EmployeeJobPositionAssignmentResponse>>(e.JobPositionAssignments.OrderByDescending(x=>x.StartDate).Select(x=>new EmployeeJobPositionAssignmentResponse(x.Id.Value,x.JobPositionId.Value,x.BranchId?.Value,x.StartDate,x.EndDate,x.IsPrimary,x.Status.ToString())).ToArray());}
}
