using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using Xunit;
namespace DriveOS.UnitTests.Modules.Workforce;
public sealed class EmployeeJobPositionAssignmentTests
{
    [Fact]
    public void Add_position_scoped_to_branch_requires_branch_coverage()
    {
        var now=DateTimeOffset.UtcNow; var actor=new UserId(Guid.NewGuid());
        Employee employee=Employee.Create(EmployeeId.New(),new OrganizationId(Guid.NewGuid()),new PersonId(Guid.NewGuid()),null,"EMP-1",new DateOnly(2026,1,1),null,now).Value;
        var result=employee.AddJobPositionAssignment(EmployeeJobPositionAssignmentId.New(),JobPositionId.New(),new BranchId(Guid.NewGuid()),new DateOnly(2026,9,1),null,true,new DateOnly(2026,8,22),now,actor);
        result.IsFailure.Should().BeTrue(); result.Error.Code.Should().Be("Workforce.JobPositionAssignment.BranchAssignmentRequired");
    }

    [Fact]
    public void Two_primary_positions_cannot_overlap()
    {
        var now=DateTimeOffset.UtcNow; var actor=new UserId(Guid.NewGuid());
        Employee employee=Employee.Create(EmployeeId.New(),new OrganizationId(Guid.NewGuid()),new PersonId(Guid.NewGuid()),null,"EMP-2",new DateOnly(2026,1,1),null,now).Value;
        employee.AddJobPositionAssignment(EmployeeJobPositionAssignmentId.New(),JobPositionId.New(),null,new DateOnly(2026,1,1),null,true,new DateOnly(2026,8,22),now,actor).IsSuccess.Should().BeTrue();
        var second=employee.AddJobPositionAssignment(EmployeeJobPositionAssignmentId.New(),JobPositionId.New(),null,new DateOnly(2026,2,1),null,true,new DateOnly(2026,8,22),now,actor);
        second.IsFailure.Should().BeTrue(); second.Error.Code.Should().Be("Workforce.JobPositionAssignment.PrimaryPeriodOverlap");
    }
}
