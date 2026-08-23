using System.Reflection;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.Modules.Workforce.Domain.LeavePolicies;
using DriveOS.Modules.Workforce.Domain.LeaveRequests;
using DriveOS.Modules.Workforce.Domain.WorkingTime;
using DriveOS.Modules.Workforce.Domain.Timesheets;
using DriveOS.Modules.Workforce.Domain.EquipmentAssignments;
using DriveOS.Modules.Workforce.Domain.PerformanceReviews;
using DriveOS.Modules.Workforce.Domain.EmployeeDocuments;
using DriveOS.Modules.Workforce.Domain.ProfessionalRestrictions;
using DriveOS.Modules.Workforce.Domain.Offboarding;
using DriveOS.SharedKernel.Domain;

namespace DriveOS.ArchitectureTests;

public sealed class WorkforceModuleArchitectureTests
{
    [Fact]
    public void Domain_ShouldNotReferenceOuterLayersOrOtherBusinessModules()
    {
        AssertDoesNotReference(
            typeof(Employee).Assembly,
            [
                "DriveOS.Modules.Workforce.Application",
                "DriveOS.Modules.Workforce.Infrastructure",
                "DriveOS.Api",
                "DriveOS.Modules.Organizations",
                "DriveOS.Modules.SchedulingCapacity",
                "DriveOS.Modules.TrainingDelivery",
                "DriveOS.Modules.ExamsCertification",
                "DriveOS.Modules.Contracts",
                "DriveOS.Modules.FleetResources",
                "DriveOS.Modules.Students",
                "DriveOS.Modules.CurriculumPedagogy",
            ]);
    }

    [Fact]
    public void Application_ShouldNotReferenceInfrastructureApiOrOtherBusinessModules()
    {
        AssertDoesNotReference(
            typeof(DriveOS.Modules.Workforce.Application.DependencyInjection).Assembly,
            [
                "DriveOS.Modules.Workforce.Infrastructure",
                "DriveOS.Api",
                "DriveOS.Modules.Organizations",
                "DriveOS.Modules.SchedulingCapacity",
                "DriveOS.Modules.TrainingDelivery",
                "DriveOS.Modules.ExamsCertification",
                "DriveOS.Modules.Contracts",
                "DriveOS.Modules.FleetResources",
                "DriveOS.Modules.Students",
                "DriveOS.Modules.CurriculumPedagogy",
            ]);
    }

    [Theory]
    [MemberData(nameof(AggregateRoots))]
    public void AggregateRoots_ShouldUseStrongIdentifiers(Type aggregateType)
    {
        Type? aggregateRoot = GetAggregateRootBase(aggregateType);

        Assert.NotNull(aggregateRoot);
        Assert.NotEqual(typeof(Guid), aggregateRoot!.GetGenericArguments()[0]);
    }

    [Fact]
    public void Api_ShouldNotExposeLegacyInstructorCredentialWriteEndpoints()
    {
        string[] apiTypes = typeof(global::Program).Assembly
            .GetTypes()
            .Select(type => type.Name)
            .ToArray();

        Assert.DoesNotContain("InstructorRegulatoryCredentialEndpoints", apiTypes);
    }

    public static IEnumerable<object[]> AggregateRoots()
    {
        yield return [typeof(Employee)];
        yield return [typeof(JobPosition)];
        yield return [typeof(LeavePolicy)];
        yield return [typeof(LeaveRequest)];
        yield return [typeof(WorkingTimePolicy)];
        yield return [typeof(Timesheet)];
        yield return [typeof(EquipmentAssignment)];
        yield return [typeof(PerformanceReview)];
        yield return [typeof(EmployeeDocument)];
        yield return [typeof(ProfessionalRestriction)];
        yield return [typeof(OffboardingProcess)];
    }

    private static Type? GetAggregateRootBase(Type type)
    {
        for (Type? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(AggregateRoot<>))
                return current;
        }

        return null;
    }

    private static void AssertDoesNotReference(Assembly assembly, IReadOnlyCollection<string> forbiddenPrefixes)
    {
        string[] references = assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        foreach (string prefix in forbiddenPrefixes)
        {
            Assert.DoesNotContain(
                references,
                reference => reference.StartsWith(prefix, StringComparison.Ordinal));
        }
    }
}
