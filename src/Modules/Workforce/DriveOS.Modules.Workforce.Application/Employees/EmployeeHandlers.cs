using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Workforce.Application.Persistence;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.Offboarding;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Application.Employees;
public sealed class CreateEmployeeCommandHandler(IEmployeeRepository repository, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<CreateEmployeeCommand, EmployeeId>
{
    public async Task<Result<EmployeeId>> Handle(CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        if (await repository.FindByEmployeeNumberAsync(command.EmployerOrganizationId, command.EmployeeNumber, cancellationToken) is not null) return Result.Failure<EmployeeId>(EmployeeErrors.DuplicateEmployeeNumber);
        if (await repository.FindCurrentByPersonAsync(command.EmployerOrganizationId, command.PersonId, cancellationToken) is not null) return Result.Failure<EmployeeId>(EmployeeErrors.ExistingEmployment);
        if (command.UserId is { } userId && await repository.FindCurrentByUserAsync(command.EmployerOrganizationId, userId, cancellationToken) is not null) return Result.Failure<EmployeeId>(EmployeeErrors.UserAlreadyLinked);
        DateTimeOffset now = clock.UtcNow;
        Result<Employee> created = Employee.Create(command.EmployeeId, command.EmployerOrganizationId, command.PersonId, command.UserId, command.EmployeeNumber, command.EmploymentStartDate, command.EmploymentEndDate, now);
        if (created.IsFailure) return Result.Failure<EmployeeId>(created.Error);
        created.Value.SetCreatedAudit(now, command.ActorUserId);
        repository.Add(created.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }
}
public sealed class RehireEmployeeCommandHandler(IEmployeeRepository repository, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<RehireEmployeeCommand, EmployeeId>
{
    public async Task<Result<EmployeeId>> Handle(RehireEmployeeCommand command, CancellationToken cancellationToken)
    {
        Employee? previous = await repository.GetByIdAsync(command.EmployerOrganizationId, command.PreviousEmployeeId, cancellationToken);
        if (previous is null) return Result.Failure<EmployeeId>(EmployeeErrors.NotFound);
        if (previous.Status != EmploymentStatus.Ended) return Result.Failure<EmployeeId>(EmployeeErrors.RehireRequiresEndedEmployment);

        Employee? latest = await repository.FindLatestByPersonAsync(command.EmployerOrganizationId, previous.PersonId, cancellationToken);
        if (latest is null || latest.Id != previous.Id) return Result.Failure<EmployeeId>(EmployeeErrors.RehireSourceMustBeLatestEmployment);
        if (await repository.FindCurrentByPersonAsync(command.EmployerOrganizationId, previous.PersonId, cancellationToken) is not null) return Result.Failure<EmployeeId>(EmployeeErrors.ExistingEmployment);
        if (await repository.FindByEmployeeNumberAsync(command.EmployerOrganizationId, command.EmployeeNumber, cancellationToken) is not null) return Result.Failure<EmployeeId>(EmployeeErrors.DuplicateEmployeeNumber);
        if (command.ReusePreviousUserLink && command.UserId.HasValue) return Result.Failure<EmployeeId>(EmployeeErrors.InvalidRehireUserSelection);

        UserId? linkedUser = command.ReusePreviousUserLink ? previous.UserId : command.UserId;
        if (linkedUser is { } userId && await repository.FindCurrentByUserAsync(command.EmployerOrganizationId, userId, cancellationToken) is not null)
            return Result.Failure<EmployeeId>(EmployeeErrors.UserAlreadyLinked);

        DateTimeOffset now = clock.UtcNow;
        Result<Employee> created = Employee.RehireFrom(previous, command.NewEmployeeId, linkedUser, command.EmployeeNumber, command.EmploymentStartDate, command.EmploymentEndDate, now, command.ActorUserId);
        if (created.IsFailure) return Result.Failure<EmployeeId>(created.Error);
        created.Value.SetCreatedAudit(now, command.ActorUserId);
        repository.Add(created.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }
}

public sealed class UpdateEmployeeIdentityCommandHandler(IEmployeeRepository repository, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<UpdateEmployeeIdentityCommand>
{
    public async Task<Result> Handle(UpdateEmployeeIdentityCommand command, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdForUpdateAsync(command.EmployerOrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null) return Result.Failure(EmployeeErrors.NotFound);
        Employee? byNumber = await repository.FindByEmployeeNumberAsync(command.EmployerOrganizationId, command.EmployeeNumber, cancellationToken);
        if (byNumber is not null && byNumber.Id != command.EmployeeId) return Result.Failure(EmployeeErrors.DuplicateEmployeeNumber);
        if (command.UserId is { } userId)
        {
            Employee? byUser = await repository.FindCurrentByUserAsync(command.EmployerOrganizationId, userId, cancellationToken);
            if (byUser is not null && byUser.Id != command.EmployeeId) return Result.Failure(EmployeeErrors.UserAlreadyLinked);
        }
        Result updated = employee.UpdateIdentity(command.UserId, command.EmployeeNumber, command.EmploymentStartDate, command.EmploymentEndDate, clock.UtcNow, command.ActorUserId);
        if (updated.IsFailure) return updated;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
public sealed class GetEmployeeQueryHandler(IEmployeeRepository repository) : IQueryHandler<GetEmployeeQuery, EmployeeResponse>
{
    public async Task<Result<EmployeeResponse>> Handle(GetEmployeeQuery query, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdAsync(query.EmployerOrganizationId, query.EmployeeId, cancellationToken);
        return employee is null ? Result.Failure<EmployeeResponse>(EmployeeErrors.NotFound) : Result.Success(Map(employee));
    }
    internal static EmployeeResponse Map(Employee x) => new(x.Id.Value, x.EmployerOrganizationId.Value, x.PersonId.Value, x.UserId?.Value, x.EmployeeNumber, x.EmploymentStartDate, x.EmploymentEndDate, x.Status.ToString(), x.RehiredFromEmployeeId?.Value, x.CreatedAtUtc, x.LastModifiedAtUtc);
}
public sealed class GetEmployeeEmploymentHistoryQueryHandler(IEmployeeRepository repository) : IQueryHandler<GetEmployeeEmploymentHistoryQuery, IReadOnlyList<EmployeeResponse>>
{
    public async Task<Result<IReadOnlyList<EmployeeResponse>>> Handle(GetEmployeeEmploymentHistoryQuery query, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdAsync(query.EmployerOrganizationId, query.EmployeeId, cancellationToken);
        if (employee is null) return Result.Failure<IReadOnlyList<EmployeeResponse>>(EmployeeErrors.NotFound);
        IReadOnlyList<Employee> history = await repository.ListByPersonAsync(query.EmployerOrganizationId, employee.PersonId, cancellationToken);
        return Result.Success<IReadOnlyList<EmployeeResponse>>(history.Select(GetEmployeeQueryHandler.Map).ToArray());
    }
}

public sealed class GetEmployeesQueryHandler(IEmployeeRepository repository) : IQueryHandler<GetEmployeesQuery, IReadOnlyList<EmployeeResponse>>
{
    public async Task<Result<IReadOnlyList<EmployeeResponse>>> Handle(GetEmployeesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<Employee> employees = await repository.ListAsync(query.EmployerOrganizationId, query.Status, cancellationToken);
        return Result.Success<IReadOnlyList<EmployeeResponse>>(employees.Select(GetEmployeeQueryHandler.Map).ToArray());
    }
}

public sealed class StartEmployeeOnboardingCommandHandler(IEmployeeRepository repository, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<StartEmployeeOnboardingCommand>
{
    public async Task<Result> Handle(StartEmployeeOnboardingCommand command, CancellationToken cancellationToken)
        => await Update(command.EmployerOrganizationId, command.EmployeeId, x => x.StartOnboarding(clock.UtcNow, command.ActorUserId), repository, unitOfWork, cancellationToken);

    private static async Task<Result> Update(OrganizationId organizationId, EmployeeId employeeId, Func<Employee, Result> action, IEmployeeRepository repository, IWorkforceUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdForUpdateAsync(organizationId, employeeId, cancellationToken);
        if (employee is null) return Result.Failure(EmployeeErrors.NotFound);
        Result result = action(employee);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ActivateEmployeeCommandHandler(IEmployeeRepository repository, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<ActivateEmployeeCommand>
{
    public async Task<Result> Handle(ActivateEmployeeCommand command, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdForUpdateAsync(command.EmployerOrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null) return Result.Failure(EmployeeErrors.NotFound);
        Result result = employee.Activate(clock.UtcNow, command.ActorUserId);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class SuspendEmployeeCommandHandler(IEmployeeRepository repository, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<SuspendEmployeeCommand>
{
    public async Task<Result> Handle(SuspendEmployeeCommand command, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdForUpdateAsync(command.EmployerOrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null) return Result.Failure(EmployeeErrors.NotFound);
        Result result = employee.Suspend(command.Reason, clock.UtcNow, command.ActorUserId);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ReactivateEmployeeCommandHandler(IEmployeeRepository repository, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<ReactivateEmployeeCommand>
{
    public async Task<Result> Handle(ReactivateEmployeeCommand command, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdForUpdateAsync(command.EmployerOrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null) return Result.Failure(EmployeeErrors.NotFound);
        Result result = employee.Reactivate(clock.UtcNow, command.ActorUserId);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class StartEmploymentTerminationCommandHandler(IEmployeeRepository repository, IOffboardingProcessRepository offboardingRepository, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<StartEmploymentTerminationCommand>
{
    public async Task<Result> Handle(StartEmploymentTerminationCommand command, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdForUpdateAsync(command.EmployerOrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null) return Result.Failure(EmployeeErrors.NotFound);
        if (await offboardingRepository.FindCurrentByEmployeeAsync(command.EmployerOrganizationId, command.EmployeeId, false, cancellationToken) is not null) return Result.Failure(OffboardingErrors.ExistingProcess);
        DateTimeOffset now = clock.UtcNow;
        Result result = employee.StartTermination(command.PlannedEndDate, command.Reason, now, command.ActorUserId);
        if (result.IsFailure) return result;
        Result<OffboardingProcess> created = OffboardingProcess.Create(OffboardingProcessId.New(), command.EmployerOrganizationId, command.EmployeeId, command.PlannedEndDate, command.Reason, now, command.ActorUserId, employee.UserId);
        if (created.IsFailure) return Result.Failure(created.Error);
        offboardingRepository.Add(created.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

/// <summary>Backward-compatible completion path. WFR-017 makes offboarding readiness mandatory.</summary>
public sealed class EndEmploymentCommandHandler(IEmployeeRepository repository, IOffboardingProcessRepository offboardingRepository, IWorkforceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<EndEmploymentCommand>
{
    public async Task<Result> Handle(EndEmploymentCommand command, CancellationToken cancellationToken)
    {
        Employee? employee = await repository.GetByIdForUpdateAsync(command.EmployerOrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null) return Result.Failure(EmployeeErrors.NotFound);
        OffboardingProcess? process = await offboardingRepository.FindCurrentByEmployeeAsync(command.EmployerOrganizationId, command.EmployeeId, true, cancellationToken);
        if (process is null) return Result.Failure(OffboardingErrors.NotFound);
        if (process.Status != OffboardingStatus.ReadyToComplete) return Result.Failure(OffboardingErrors.ChecklistIncomplete);
        if (command.EndDate != process.PlannedEndDate) return Result.Failure(EmployeeErrors.InvalidTerminationDate);
        DateTimeOffset now = clock.UtcNow;
        Result result = employee.EndEmployment(command.EndDate, command.Reason, now, command.ActorUserId);
        if (result.IsFailure) return result;
        Result completed = process.Complete(now, command.ActorUserId, employee.UserId);
        if (completed.IsFailure) return completed;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
