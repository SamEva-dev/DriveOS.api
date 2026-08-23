using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Application.WorkingTime;
public sealed record CreateWorkingTimePolicyCommand(OrganizationId OrganizationId,WorkingTimePolicyId PolicyId,EmployeeId EmployeeId,DateOnly EffectiveFrom,DateOnly? EffectiveTo,decimal ContractualWeeklyHours,decimal? ContractualDailyHours,int? MaxWorkingDaysPerWeek,UserId ActorUserId):ICommand<WorkingTimePolicyId>;
public sealed record UpdateWorkingTimePolicyCommand(OrganizationId OrganizationId,WorkingTimePolicyId PolicyId,DateOnly EffectiveFrom,DateOnly? EffectiveTo,decimal ContractualWeeklyHours,decimal? ContractualDailyHours,int? MaxWorkingDaysPerWeek,UserId ActorUserId):ICommand;
public sealed record DeactivateWorkingTimePolicyCommand(OrganizationId OrganizationId,WorkingTimePolicyId PolicyId,UserId ActorUserId):ICommand;
public sealed record GetWorkingTimePoliciesQuery(OrganizationId OrganizationId,EmployeeId EmployeeId):IQuery<IReadOnlyList<WorkingTimePolicyResponse>>;
public sealed record GetWorkingTimeSummaryQuery(OrganizationId OrganizationId,EmployeeId EmployeeId,DateOnly From,DateOnly To):IQuery<WorkingTimeSummaryResponse>;
public sealed record WorkingTimePolicyResponse(Guid Id,Guid EmployeeId,DateOnly EffectiveFrom,DateOnly? EffectiveTo,decimal ContractualWeeklyHours,decimal? ContractualDailyHours,int? MaxWorkingDaysPerWeek,string Status);
public sealed record WorkingTimeSummaryResponse(Guid EmployeeId,DateOnly From,DateOnly To,decimal ContractualHours,decimal PlannedHours,decimal ActualTeachingHours,decimal ApprovedLeaveHours,decimal VarianceToContractHours);
public interface IWorkingTimeProjectionGateway
{
    Task<WorkingTimeProjectionSnapshot> GetAsync(OrganizationId organizationId,EmployeeId employeeId,DateOnly from,DateOnly to,CancellationToken ct=default);
}
public sealed record WorkingTimeProjectionSnapshot(decimal PlannedHours,decimal ActualTeachingHours,decimal ApprovedLeaveHours);
