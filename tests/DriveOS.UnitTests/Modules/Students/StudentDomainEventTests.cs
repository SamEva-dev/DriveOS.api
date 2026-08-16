using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Events;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class StudentDomainEventTests
{
    [Fact]
    public void CreateStudent_RaisesStudentCreated()
    {
        Student student = Student.Create(
            PersonId.New(),
            OrganizationId.New(),
            "Ada",
            "Lovelace",
            "ada@example.test",
            null).Value;

        Assert.Contains(student.DomainEvents, x => x is StudentCreatedDomainEvent);
    }

    [Fact]
    public void EnrollmentLifecycle_RaisesTypedEvents()
    {
        Enrollment enrollment = Enrollment.CreateDraft(
            DraftEnrollmentId.New(),
            OrganizationId.New(),
            PersonId.New(),
            BranchId.New(),
            null,
            "B").Value;

        Assert.Contains(enrollment.DomainEvents, x => x is EnrollmentCreatedDomainEvent);

        enrollment.ClearDomainEvents();
        enrollment.Activate(UserId.New(), DateTimeOffset.UtcNow);

        Assert.Contains(enrollment.DomainEvents, x => x is EnrollmentStatusChangedDomainEvent);
    }
}
