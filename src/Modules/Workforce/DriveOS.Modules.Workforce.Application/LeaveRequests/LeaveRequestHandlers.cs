using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Workforce.Application.Persistence;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.LeavePolicies;
using DriveOS.Modules.Workforce.Domain.LeaveRequests;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Application.LeaveRequests;

public sealed class CreateLeaveRequestCommandHandler(ILeaveRequestRepository requests, IEmployeeRepository employees, ILeavePolicyRepository policies, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<CreateLeaveRequestCommand, LeaveRequestId>
{
    public async Task<Result<LeaveRequestId>> Handle(CreateLeaveRequestCommand c, CancellationToken ct)
    {
        var employee = await employees.GetByIdAsync(c.OrganizationId, c.EmployeeId, ct);
        if (employee is null) return Result.Failure<LeaveRequestId>(EmployeeErrors.NotFound);
        if (employee.Status is not (EmploymentStatus.Active or EmploymentStatus.OnLeave)) return Result.Failure<LeaveRequestId>(LeaveRequestErrors.EmployeeNotEligible);
        if (c.StartDate < employee.EmploymentStartDate || (employee.EmploymentEndDate is DateOnly end && c.EndDate > end)) return Result.Failure<LeaveRequestId>(LeaveRequestErrors.PeriodOutsideEmployment);
        var policy = await policies.GetByIdAsync(c.OrganizationId, c.LeavePolicyId, ct);
        if (policy is null) return Result.Failure<LeaveRequestId>(LeavePolicyErrors.NotFound);
        if (policy.Status != LeavePolicyStatus.Active) return Result.Failure<LeaveRequestId>(LeaveRequestErrors.PolicyInactive);
        var now = clock.UtcNow;
        var result = LeaveRequest.Create(c.LeaveRequestId, c.OrganizationId, c.EmployeeId, c.LeavePolicyId, policy.Code, policy.CountryCode, c.StartDate, c.EndDate, c.StartPortion, c.EndPortion, c.Reason, c.EvidenceDocumentId, policy.RequiresApproval, policy.RequiresEvidence, policy.AllowHalfDay, policy.MinimumNoticeDays, policy.MaximumConsecutiveDays, now);
        if (result.IsFailure) return Result.Failure<LeaveRequestId>(result.Error);
        result.Value.SetCreatedAudit(now, c.ActorUserId); requests.Add(result.Value); await uow.CommitAsync(ct); return Result.Success(result.Value.Id);
    }
}
public sealed class UpdateLeaveRequestCommandHandler(ILeaveRequestRepository requests, IEmployeeRepository employees, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<UpdateLeaveRequestCommand>
{
    public async Task<Result> Handle(UpdateLeaveRequestCommand c, CancellationToken ct)
    {
        var request = await requests.GetByIdForUpdateAsync(c.OrganizationId, c.LeaveRequestId, ct); if (request is null) return Result.Failure(LeaveRequestErrors.NotFound);
        var employee = await employees.GetByIdAsync(c.OrganizationId, request.EmployeeId, ct); if (employee is null) return Result.Failure(EmployeeErrors.NotFound);
        if (c.StartDate < employee.EmploymentStartDate || (employee.EmploymentEndDate is DateOnly end && c.EndDate > end)) return Result.Failure(LeaveRequestErrors.PeriodOutsideEmployment);
        var r = request.Update(c.StartDate, c.EndDate, c.StartPortion, c.EndPortion, c.Reason, c.EvidenceDocumentId, clock.UtcNow, c.ActorUserId); if (r.IsFailure) return r; await uow.CommitAsync(ct); return Result.Success();
    }
}
public sealed class SubmitLeaveRequestCommandHandler(ILeaveRequestRepository requests, IEmployeeRepository employees, ILeavePolicyRepository policies, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<SubmitLeaveRequestCommand>
{
    public async Task<Result> Handle(SubmitLeaveRequestCommand c, CancellationToken ct)
    {
        var request = await requests.GetByIdForUpdateAsync(c.OrganizationId, c.LeaveRequestId, ct); if (request is null) return Result.Failure(LeaveRequestErrors.NotFound);
        var employee = await employees.GetByIdAsync(c.OrganizationId, request.EmployeeId, ct); if (employee is null || employee.Status is not (EmploymentStatus.Active or EmploymentStatus.OnLeave)) return Result.Failure(LeaveRequestErrors.EmployeeNotEligible);
        var policy = await policies.GetByIdAsync(c.OrganizationId, request.LeavePolicyId, ct); if (policy is null || policy.Status != LeavePolicyStatus.Active) return Result.Failure(LeaveRequestErrors.PolicyInactive);
        if (await requests.HasOverlappingAsync(c.OrganizationId, request.EmployeeId, request.StartDate, request.EndDate, request.Id, ct)) return Result.Failure(LeaveRequestErrors.OverlappingRequest);
        var now = clock.UtcNow; var r = request.Submit(DateOnly.FromDateTime(now.UtcDateTime), now, c.ActorUserId); if (r.IsFailure) return r; await uow.CommitAsync(ct); return Result.Success();
    }
}
public sealed class ApproveLeaveRequestCommandHandler(ILeaveRequestRepository requests, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<ApproveLeaveRequestCommand>
{ public async Task<Result> Handle(ApproveLeaveRequestCommand c,CancellationToken ct){var x=await requests.GetByIdForUpdateAsync(c.OrganizationId,c.LeaveRequestId,ct);if(x is null)return Result.Failure(LeaveRequestErrors.NotFound);var r=x.Approve(clock.UtcNow,c.ActorUserId,c.Reason);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}}
public sealed class RejectLeaveRequestCommandHandler(ILeaveRequestRepository requests, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<RejectLeaveRequestCommand>
{ public async Task<Result> Handle(RejectLeaveRequestCommand c,CancellationToken ct){var x=await requests.GetByIdForUpdateAsync(c.OrganizationId,c.LeaveRequestId,ct);if(x is null)return Result.Failure(LeaveRequestErrors.NotFound);var r=x.Reject(clock.UtcNow,c.ActorUserId,c.Reason);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}}
public sealed class CancelLeaveRequestCommandHandler(ILeaveRequestRepository requests, IWorkforceUnitOfWork uow, IClock clock) : ICommandHandler<CancelLeaveRequestCommand>
{ public async Task<Result> Handle(CancelLeaveRequestCommand c,CancellationToken ct){var x=await requests.GetByIdForUpdateAsync(c.OrganizationId,c.LeaveRequestId,ct);if(x is null)return Result.Failure(LeaveRequestErrors.NotFound);var now=clock.UtcNow;var r=x.Cancel(DateOnly.FromDateTime(now.UtcDateTime),now,c.ActorUserId,c.Reason);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}}
public sealed class GetLeaveRequestQueryHandler(ILeaveRequestRepository requests) : IQueryHandler<GetLeaveRequestQuery,LeaveRequestResponse>
{ public async Task<Result<LeaveRequestResponse>> Handle(GetLeaveRequestQuery q,CancellationToken ct){var x=await requests.GetByIdAsync(q.OrganizationId,q.LeaveRequestId,ct);return x is null?Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.NotFound):Result.Success(Map(x));} internal static LeaveRequestResponse Map(LeaveRequest x)=>new(x.Id.Value,x.EmployeeId.Value,x.LeavePolicyId.Value,x.PolicyCode,x.CountryCode,x.StartDate,x.EndDate,x.StartPortion.ToString(),x.EndPortion.ToString(),x.Reason,x.EvidenceDocumentId?.Value,x.RequiresApproval,x.RequiresEvidence,x.Status.ToString(),x.SubmittedAtUtc,x.DecidedAtUtc,x.DecidedByUserId?.Value,x.DecisionReason,x.CancelledAtUtc); }
public sealed class GetLeaveRequestsQueryHandler(ILeaveRequestRepository requests) : IQueryHandler<GetLeaveRequestsQuery,IReadOnlyList<LeaveRequestResponse>>
{ public async Task<Result<IReadOnlyList<LeaveRequestResponse>>> Handle(GetLeaveRequestsQuery q,CancellationToken ct)=>Result.Success<IReadOnlyList<LeaveRequestResponse>>((await requests.ListAsync(q.OrganizationId,q.EmployeeId,q.Status,q.From,q.To,ct)).Select(GetLeaveRequestQueryHandler.Map).ToArray()); }
