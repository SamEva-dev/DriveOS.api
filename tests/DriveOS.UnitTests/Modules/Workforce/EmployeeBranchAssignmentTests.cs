using DriveOS.Modules.Workforce.Domain.BranchAssignments;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using Xunit;

namespace DriveOS.UnitTests.Modules.Workforce;

public sealed class EmployeeBranchAssignmentTests
{
    [Fact]
    public void AddBranchAssignment_rejects_overlap_on_same_branch()
    {
        Employee employee = CreateEmployee();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateOnly today = DateOnly.FromDateTime(now.UtcDateTime);
        BranchId branchId = new(Guid.NewGuid());
        UserId actor = new(Guid.NewGuid());
        employee.AddBranchAssignment(EmployeeBranchAssignmentId.New(), branchId, today, today.AddDays(10), false, today, now, actor).IsSuccess.Should().BeTrue();

        var result = employee.AddBranchAssignment(EmployeeBranchAssignmentId.New(), branchId, today.AddDays(5), today.AddDays(20), false, today, now, actor);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(EmployeeBranchAssignmentErrors.SameBranchPeriodOverlap.Code);
    }

    [Fact]
    public void AddBranchAssignment_rejects_overlapping_primary_assignments()
    {
        Employee employee = CreateEmployee();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateOnly today = DateOnly.FromDateTime(now.UtcDateTime);
        UserId actor = new(Guid.NewGuid());
        employee.AddBranchAssignment(EmployeeBranchAssignmentId.New(), new BranchId(Guid.NewGuid()), today, null, true, today, now, actor).IsSuccess.Should().BeTrue();

        var result = employee.AddBranchAssignment(EmployeeBranchAssignmentId.New(), new BranchId(Guid.NewGuid()), today.AddDays(1), null, true, today, now, actor);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(EmployeeBranchAssignmentErrors.PrimaryPeriodOverlap.Code);
    }

    private static Employee CreateEmployee()
    {
        var result = Employee.Create(EmployeeId.New(), new OrganizationId(Guid.NewGuid()), new PersonId(Guid.NewGuid()), null, "EMP-001", DateOnly.FromDateTime(DateTime.UtcNow), null, DateTimeOffset.UtcNow);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}
