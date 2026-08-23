using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
namespace DriveOS.UnitTests.Modules.Workforce;
public sealed class EmployeeTests
{
    [Fact]
    public void Create_should_keep_employee_distinct_from_user_account()
    {
        var result = Employee.Create(EmployeeId.New(), OrganizationId.New(), PersonId.New(), null, " emp-001 ", new DateOnly(2026, 9, 1), null, DateTimeOffset.UtcNow);
        result.IsSuccess.Should().BeTrue(); result.Value.EmployeeNumber.Should().Be("EMP-001"); result.Value.UserId.Should().BeNull(); result.Value.Status.Should().Be(EmploymentStatus.Draft);
    }
    [Fact]
    public void Create_should_reject_end_before_start()
    {
        var result = Employee.Create(EmployeeId.New(), OrganizationId.New(), PersonId.New(), null, "EMP-001", new DateOnly(2026, 9, 2), new DateOnly(2026, 9, 1), DateTimeOffset.UtcNow);
        result.IsFailure.Should().BeTrue(); result.Error.Code.Should().Be("Workforce.Employee.InvalidEmploymentPeriod");
    }
    [Fact]
    public void Rehire_should_create_a_new_draft_employee_linked_to_the_ended_employment()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        UserId actor = UserId.New();
        Employee previous = Employee.Create(EmployeeId.New(), OrganizationId.New(), PersonId.New(), actor, "EMP-001", new DateOnly(2026, 1, 1), null, now).Value;
        previous.StartOnboarding(now, actor).IsSuccess.Should().BeTrue();
        previous.Activate(now, actor).IsSuccess.Should().BeTrue();
        previous.StartTermination(new DateOnly(2026, 6, 30), "Departure", now, actor).IsSuccess.Should().BeTrue();
        previous.EndEmployment(new DateOnly(2026, 6, 30), "Departure", now, actor).IsSuccess.Should().BeTrue();

        EmployeeId newId = EmployeeId.New();
        var result = Employee.RehireFrom(previous, newId, previous.UserId, "EMP-001", new DateOnly(2026, 9, 1), null, now.AddDays(1), actor);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(newId);
        result.Value.Id.Should().NotBe(previous.Id);
        result.Value.PersonId.Should().Be(previous.PersonId);
        result.Value.RehiredFromEmployeeId.Should().Be(previous.Id);
        result.Value.Status.Should().Be(EmploymentStatus.Draft);
        result.Value.BranchAssignments.Should().BeEmpty();
        result.Value.JobPositionAssignments.Should().BeEmpty();
        result.Value.Qualifications.Should().BeEmpty();
        result.Value.InstructorAuthorizations.Should().BeEmpty();
        result.Value.EmploymentContracts.Should().BeEmpty();
    }

    [Fact]
    public void Rehire_should_require_previous_employment_to_be_ended()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        UserId actor = UserId.New();
        Employee previous = Employee.Create(EmployeeId.New(), OrganizationId.New(), PersonId.New(), null, "EMP-001", new DateOnly(2026, 1, 1), null, now).Value;

        var result = Employee.RehireFrom(previous, EmployeeId.New(), null, "EMP-001", new DateOnly(2026, 9, 1), null, now, actor);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workforce.Employee.RehireRequiresEndedEmployment");
    }

    [Fact]
    public void Ended_employment_identity_should_be_immutable()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        UserId actor = UserId.New();
        Employee employee = Employee.Create(EmployeeId.New(), OrganizationId.New(), PersonId.New(), null, "EMP-001", new DateOnly(2026, 1, 1), null, now).Value;
        employee.StartOnboarding(now, actor).IsSuccess.Should().BeTrue();
        employee.Activate(now, actor).IsSuccess.Should().BeTrue();
        employee.StartTermination(new DateOnly(2026, 6, 30), "Departure", now, actor).IsSuccess.Should().BeTrue();
        employee.EndEmployment(new DateOnly(2026, 6, 30), "Departure", now, actor).IsSuccess.Should().BeTrue();

        var result = employee.UpdateIdentity(null, "CHANGED", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), now.AddDays(1), actor);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workforce.Employee.EndedEmploymentImmutable");
    }

}
