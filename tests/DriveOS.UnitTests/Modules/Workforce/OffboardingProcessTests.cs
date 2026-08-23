using DriveOS.Modules.Workforce.Domain.Offboarding;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
namespace DriveOS.UnitTests.Workforce;
public sealed class OffboardingProcessTests
{
    [Fact]
    public void Complete_is_rejected_while_checklist_is_pending()
    {
        var actor=new UserId(Guid.NewGuid());
        var created=OffboardingProcess.Create(OffboardingProcessId.New(),new OrganizationId(Guid.NewGuid()),new EmployeeId(Guid.NewGuid()),new DateOnly(2026,9,30),"Departure",DateTimeOffset.UtcNow,actor);
        created.IsSuccess.Should().BeTrue();
        created.Value.Complete(DateTimeOffset.UtcNow,actor).IsFailure.Should().BeTrue();
    }
    [Fact]
    public void Process_becomes_ready_when_automatic_dependencies_are_clear_and_manual_items_are_completed()
    {
        var actor=new UserId(Guid.NewGuid());var now=DateTimeOffset.UtcNow;
        var x=OffboardingProcess.Create(OffboardingProcessId.New(),new OrganizationId(Guid.NewGuid()),new EmployeeId(Guid.NewGuid()),new DateOnly(2026,9,30),"Departure",now,actor).Value;
        foreach(var item in x.Items.Where(i=>i.IsAutomatic).ToArray()) x.SynchronizeAutomaticItem(item.Kind,0,null,now,actor).IsSuccess.Should().BeTrue();
        foreach(var item in x.Items.Where(i=>!i.IsAutomatic).ToArray()) x.CompleteManualItem(item.Kind,null,now,actor).IsSuccess.Should().BeTrue();
        x.Status.Should().Be(OffboardingStatus.ReadyToComplete);
        x.Complete(now,actor).IsSuccess.Should().BeTrue();
        x.Status.Should().Be(OffboardingStatus.Completed);
    }
}
