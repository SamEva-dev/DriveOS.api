using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Workforce.Application.Persistence;
using DriveOS.Modules.Workforce.Domain.BranchAssignments;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Workforce.Application.BranchAssignments;

public sealed class AddEmployeeBranchAssignmentCommandHandler(IEmployeeRepository repository, IWorkforceBranchDirectory branchDirectory, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<AddEmployeeBranchAssignmentCommand, EmployeeBranchAssignmentId>
{
    public async Task<Result<EmployeeBranchAssignmentId>> Handle(AddEmployeeBranchAssignmentCommand command, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null) return Result.Failure<EmployeeBranchAssignmentId>(EmployeeErrors.NotFound);
        WorkforceBranchSnapshot? branch = await branchDirectory.GetAsync(command.OrganizationId, command.BranchId, cancellationToken);
        if (branch is null) return Result.Failure<EmployeeBranchAssignmentId>(EmployeeBranchAssignmentErrors.BranchNotFound);
        if (string.Equals(branch.Status, "Closed", StringComparison.OrdinalIgnoreCase)) return Result.Failure<EmployeeBranchAssignmentId>(EmployeeBranchAssignmentErrors.BranchClosed);

        DateTimeOffset now = clock.UtcNow;
        Result<EmployeeBranchAssignmentId> result = employee.AddBranchAssignment(command.AssignmentId, command.BranchId, command.StartDate, command.EndDate, command.IsPrimary, DateOnly.FromDateTime(now.UtcDateTime), now, command.ActorUserId);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return result;
    }
}

public sealed class UpdateEmployeeBranchAssignmentCommandHandler(IEmployeeRepository repository, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<UpdateEmployeeBranchAssignmentCommand>
{
    public async Task<Result> Handle(UpdateEmployeeBranchAssignmentCommand command, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null) return Result.Failure(EmployeeErrors.NotFound);
        DateTimeOffset now = clock.UtcNow;
        Result result = employee.UpdateBranchAssignment(command.AssignmentId, command.StartDate, command.EndDate, command.IsPrimary, DateOnly.FromDateTime(now.UtcDateTime), now, command.ActorUserId);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class EndEmployeeBranchAssignmentCommandHandler(IEmployeeRepository repository, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<EndEmployeeBranchAssignmentCommand>
{
    public async Task<Result> Handle(EndEmployeeBranchAssignmentCommand command, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null) return Result.Failure(EmployeeErrors.NotFound);
        DateTimeOffset now = clock.UtcNow;
        Result result = employee.EndBranchAssignment(command.AssignmentId, command.EndDate, DateOnly.FromDateTime(now.UtcDateTime), now, command.ActorUserId);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class CancelEmployeeBranchAssignmentCommandHandler(IEmployeeRepository repository, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<CancelEmployeeBranchAssignmentCommand>
{
    public async Task<Result> Handle(CancelEmployeeBranchAssignmentCommand command, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null) return Result.Failure(EmployeeErrors.NotFound);
        Result result = employee.CancelBranchAssignment(command.AssignmentId, clock.UtcNow, command.ActorUserId);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class GetEmployeeBranchAssignmentsQueryHandler(IEmployeeRepository repository) : IQueryHandler<GetEmployeeBranchAssignmentsQuery, IReadOnlyList<EmployeeBranchAssignmentResponse>>
{
    public async Task<Result<IReadOnlyList<EmployeeBranchAssignmentResponse>>> Handle(GetEmployeeBranchAssignmentsQuery query, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdAsync(query.OrganizationId, query.EmployeeId, cancellationToken);
        if (employee is null) return Result.Failure<IReadOnlyList<EmployeeBranchAssignmentResponse>>(EmployeeErrors.NotFound);
        return Result.Success<IReadOnlyList<EmployeeBranchAssignmentResponse>>(employee.BranchAssignments.OrderByDescending(x => x.StartDate).Select(x => new EmployeeBranchAssignmentResponse(x.Id.Value, x.BranchId.Value, x.StartDate, x.EndDate, x.IsPrimary, x.Status.ToString(), x.CreatedAtUtc, x.LastModifiedAtUtc)).ToArray());
    }
}
