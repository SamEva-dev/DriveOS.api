using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.EmploymentContracts;
using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.Modules.Workforce.Domain.BranchAssignments;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using Xunit;

namespace DriveOS.UnitTests.Modules.Workforce.Employees;

public sealed class EmployeeEmploymentPeriodInvariantTests
{
    private static readonly DateOnly EmploymentStart = new(2026, 1, 1);
    private static readonly DateOnly EmploymentEnd = new(2026, 12, 31);

    [Fact]
    public void Branch_assignment_cannot_start_before_employment()
    {
        Employee employee = CreateEmployee();

        var result = employee.AddBranchAssignment(
            EmployeeBranchAssignmentId.New(),
            BranchId.New(),
            EmploymentStart.AddDays(-1),
            EmploymentEnd,
            true,
            new DateOnly(2026, 8, 24),
            DateTimeOffset.UtcNow,
            UserId.New());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(EmployeeBranchAssignmentErrors.PeriodOutsideEmployment.Code);
    }

    [Fact]
    public void Job_position_assignment_cannot_end_after_employment()
    {
        Employee employee = CreateEmployee();

        var result = employee.AddJobPositionAssignment(
            EmployeeJobPositionAssignmentId.New(),
            JobPositionId.New(),
            null,
            EmploymentStart,
            EmploymentEnd.AddDays(1),
            true,
            new DateOnly(2026, 8, 24),
            DateTimeOffset.UtcNow,
            UserId.New());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(EmployeeJobPositionAssignmentErrors.PeriodOutsideEmployment.Code);
    }

    [Fact]
    public void Employment_contract_cannot_extend_beyond_employment()
    {
        Employee employee = CreateEmployee();

        var result = employee.AddEmploymentContract(
            EmploymentContractId.New(),
            EmploymentContractType.FixedTerm,
            EmploymentStart,
            EmploymentEnd.AddDays(1),
            35m,
            null,
            DateTimeOffset.UtcNow,
            UserId.New());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(EmploymentContractErrors.PeriodOutsideEmployment.Code);
    }

    [Fact]
    public void Signed_employment_contract_terms_are_immutable()
    {
        Employee employee = CreateEmployee();
        UserId actor = UserId.New();
        EmploymentContractId contractId = EmploymentContractId.New();
        ContractDocumentId documentId = ContractDocumentId.New();
        SignatureProcessId signatureProcessId = SignatureProcessId.New();

        employee.AddEmploymentContract(
            contractId,
            EmploymentContractType.FixedTerm,
            EmploymentStart,
            EmploymentEnd,
            35m,
            null,
            DateTimeOffset.UtcNow,
            actor).IsSuccess.Should().BeTrue();

        employee.LinkEmploymentContractDocument(
            contractId,
            documentId,
            signatureProcessId,
            DateTimeOffset.UtcNow,
            actor).IsSuccess.Should().BeTrue();

        employee.MarkEmploymentContractSigned(
            contractId,
            signatureProcessId,
            DateTimeOffset.UtcNow,
            actor).IsSuccess.Should().BeTrue();

        var result = employee.UpdateEmploymentContractTerms(
            contractId,
            EmploymentStart,
            EmploymentEnd,
            39m,
            null,
            DateTimeOffset.UtcNow,
            actor);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(EmploymentContractErrors.ImmutableAfterSignatureFlow.Code);
    }

    [Fact]
    public void Rehire_creates_a_distinct_employment_period()
    {
        UserId actor = UserId.New();
        Employee previous = CreateEmployee(endDate: null);

        previous.StartOnboarding(DateTimeOffset.UtcNow, actor).IsSuccess.Should().BeTrue();
        previous.Activate(DateTimeOffset.UtcNow, actor).IsSuccess.Should().BeTrue();
        previous.StartTermination(new DateOnly(2026, 6, 30), "End of contract", DateTimeOffset.UtcNow, actor).IsSuccess.Should().BeTrue();
        previous.EndEmployment(new DateOnly(2026, 6, 30), "End of contract", DateTimeOffset.UtcNow, actor).IsSuccess.Should().BeTrue();

        EmployeeId newId = EmployeeId.New();
        var result = Employee.RehireFrom(
            previous,
            newId,
            previous.UserId,
            "EMP-002",
            new DateOnly(2026, 7, 1),
            null,
            DateTimeOffset.UtcNow,
            actor);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(newId);
        result.Value.Id.Should().NotBe(previous.Id);
        result.Value.RehiredFromEmployeeId.Should().Be(previous.Id);
        previous.Status.Should().Be(EmploymentStatus.Ended);
    }

    private static Employee CreateEmployee(DateOnly? endDate = default)
    {
        DateOnly? effectiveEnd = endDate == default ? EmploymentEnd : endDate;
        var result = Employee.Create(
            EmployeeId.New(),
            OrganizationId.New(),
            PersonId.New(),
            UserId.New(),
            "EMP-001",
            EmploymentStart,
            effectiveEnd,
            DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}
