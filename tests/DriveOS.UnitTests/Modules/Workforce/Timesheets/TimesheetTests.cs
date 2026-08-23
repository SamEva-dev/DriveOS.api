using DriveOS.Modules.Workforce.Domain.Timesheets;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
namespace DriveOS.UnitTests.Modules.Workforce.Timesheets;
public sealed class TimesheetTests
{
    private static readonly OrganizationId Org = OrganizationId.New();
    private static readonly EmployeeId Employee = EmployeeId.New();
    private static readonly UserId Actor = UserId.New();
    [Fact]
    public void Approved_timesheet_is_not_editable()
    {
        var now=DateTimeOffset.UtcNow;
        var sheet=Timesheet.Create(TimesheetId.New(),Org,Employee,new DateOnly(2026,9,1),new DateOnly(2026,9,7),now,Actor).Value;
        sheet.AddEntry(TimesheetEntryId.New(),new DateOnly(2026,9,1),TimesheetActivityType.Teaching,7m,null,TimesheetEntrySource.TrainingDelivery,"session-1",now,Actor).IsSuccess.Should().BeTrue();
        sheet.Submit(now,Actor).IsSuccess.Should().BeTrue();
        sheet.Approve(now,Actor,null).IsSuccess.Should().BeTrue();
        sheet.AddEntry(TimesheetEntryId.New(),new DateOnly(2026,9,2),TimesheetActivityType.Administrative,1m,null,TimesheetEntrySource.Manual,null,now,Actor).IsFailure.Should().BeTrue();
    }
    [Fact]
    public void Rejected_timesheet_can_be_corrected_and_resubmitted()
    {
        var now=DateTimeOffset.UtcNow;
        var sheet=Timesheet.Create(TimesheetId.New(),Org,Employee,new DateOnly(2026,9,1),new DateOnly(2026,9,7),now,Actor).Value;
        sheet.AddEntry(TimesheetEntryId.New(),new DateOnly(2026,9,1),TimesheetActivityType.Administrative,2m,null,TimesheetEntrySource.Manual,null,now,Actor);
        sheet.Submit(now,Actor); sheet.Reject(now,Actor,"Correction requise");
        sheet.AddEntry(TimesheetEntryId.New(),new DateOnly(2026,9,2),TimesheetActivityType.Meeting,1m,null,TimesheetEntrySource.Manual,null,now,Actor).IsSuccess.Should().BeTrue();
        sheet.Submit(now,Actor).IsSuccess.Should().BeTrue();
    }
    [Fact]
    public void Lock_requires_approval()
    {
        var now=DateTimeOffset.UtcNow;
        var sheet=Timesheet.Create(TimesheetId.New(),Org,Employee,new DateOnly(2026,9,1),new DateOnly(2026,9,7),now,Actor).Value;
        sheet.Lock(now,Actor).IsFailure.Should().BeTrue();
    }
}
