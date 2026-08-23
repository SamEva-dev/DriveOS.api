using DriveOS.Modules.Workforce.Domain.LeaveRequests;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.UnitTests.Modules.Workforce;
public sealed class LeaveRequestTests
{
    private static LeaveRequest Create(bool requiresApproval = true, bool requiresEvidence = false, bool allowHalfDay = true, int? maxDays = null, DocumentId? evidence = null)
    {
        var result = LeaveRequest.Create(LeaveRequestId.New(), new OrganizationId(Guid.NewGuid()), new EmployeeId(Guid.NewGuid()), LeavePolicyId.New(), "CP", "FR", new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 10), LeaveDayPortion.FullDay, LeaveDayPortion.FullDay, null, evidence, requiresApproval, requiresEvidence, allowHalfDay, null, maxDays, DateTimeOffset.UtcNow);
        Assert.True(result.IsSuccess); return result.Value;
    }
    [Fact] public void Submit_WhenApprovalNotRequired_ShouldAutoApprove(){var r=Create(requiresApproval:false);var result=r.Submit(new DateOnly(2026,9,1),DateTimeOffset.UtcNow,new UserId(Guid.NewGuid()));Assert.True(result.IsSuccess);Assert.Equal(LeaveRequestStatus.Approved,r.Status);}
    [Fact] public void Submit_WhenEvidenceRequiredAndMissing_ShouldFail(){var r=Create(requiresEvidence:true);var result=r.Submit(new DateOnly(2026,9,1),DateTimeOffset.UtcNow,new UserId(Guid.NewGuid()));Assert.True(result.IsFailure);Assert.Equal(LeaveRequestErrors.EvidenceRequired,result.Error);}
    [Fact] public void Create_WhenHalfDayForbidden_ShouldFail(){var result=LeaveRequest.Create(LeaveRequestId.New(),new OrganizationId(Guid.NewGuid()),new EmployeeId(Guid.NewGuid()),LeavePolicyId.New(),"CP","FR",new DateOnly(2026,9,10),new DateOnly(2026,9,10),LeaveDayPortion.Morning,LeaveDayPortion.Morning,null,null,true,false,false,null,null,DateTimeOffset.UtcNow);Assert.True(result.IsFailure);Assert.Equal(LeaveRequestErrors.HalfDayNotAllowed,result.Error);}
    [Fact] public void Create_WhenMaximumDurationExceeded_ShouldFail(){var result=LeaveRequest.Create(LeaveRequestId.New(),new OrganizationId(Guid.NewGuid()),new EmployeeId(Guid.NewGuid()),LeavePolicyId.New(),"CP","FR",new DateOnly(2026,9,10),new DateOnly(2026,9,15),LeaveDayPortion.FullDay,LeaveDayPortion.FullDay,null,null,true,false,true,null,3,DateTimeOffset.UtcNow);Assert.True(result.IsFailure);Assert.Equal(LeaveRequestErrors.MaximumDurationExceeded,result.Error);}
}
