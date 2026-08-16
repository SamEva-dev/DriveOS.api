using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class StudentTests
{
    [Fact]
    public void UpdateIdentity_ShouldAuditAndResetVerificationToDeclared()
    {
        Student student = Student
            .Create(
                PersonId.New(),
                OrganizationId.New(),
                "Ada",
                "Lovelace",
                "ada@example.test",
                null
            )
            .Value;
        UserId actor = UserId.New();
        student.VerifyIdentity(
            IdentityVerificationStatus.DocumentVerified,
            "Passport checked",
            actor,
            DateTimeOffset.UtcNow
        );
        var data = new StudentIdentityData(
            "Augusta",
            "Lovelace",
            "Ada",
            null,
            null,
            null,
            "ada@example.test",
            null,
            null,
            null,
            null,
            null,
            "FR",
            "fr",
            "Europe/Paris"
        );

        var result = student.UpdateIdentity(
            data,
            "Legal name corrected from document",
            actor,
            DateTimeOffset.UtcNow
        );

        result.IsSuccess.Should().BeTrue();
        student.IdentityVerificationStatus.Should().Be(IdentityVerificationStatus.Declared);
        student.IdentityAuditEntries.Should().HaveCount(2);
    }

    [Fact]
    public void UpdateIdentity_ShouldRejectUnjustifiedChangeOfVerifiedIdentity()
    {
        Student student = Student
            .Create(PersonId.New(), OrganizationId.New(), "Ada", "Lovelace", null, "0600000000")
            .Value;
        UserId actor = UserId.New();
        student.VerifyIdentity(
            IdentityVerificationStatus.ExternallyVerified,
            "Government identity match",
            actor,
            DateTimeOffset.UtcNow
        );
        var data = new StudentIdentityData(
            "Ada",
            "Byron",
            null,
            null,
            null,
            null,
            null,
            "0600000000",
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        var result = student.UpdateIdentity(data, null, actor, DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(StudentErrors.VerifiedIdentityJustificationRequired);
    }

    [Fact]
    public void UpdateSelfServiceContact_ShouldNeverChangeLegalIdentityOrVerification()
    {
        Student student = Student
            .Create(PersonId.New(), OrganizationId.New(), "Ada", "Lovelace", null, null)
            .Value;
        UserId actor = UserId.New();
        student.VerifyIdentity(
            IdentityVerificationStatus.DocumentVerified,
            "Passport checked",
            actor,
            DateTimeOffset.UtcNow
        );

        var result = student.UpdateSelfServiceContact(
            "new@example.test",
            "0600000000",
            "1 rue du Test",
            null,
            "06000",
            "Nice",
            "fr",
            "FR",
            "Europe/Paris",
            true,
            true,
            false,
            actor,
            DateTimeOffset.UtcNow
        );

        result.IsSuccess.Should().BeTrue();
        student.FirstName.Should().Be("Ada");
        student.LastName.Should().Be("Lovelace");
        student.IdentityVerificationStatus.Should().Be(IdentityVerificationStatus.DocumentVerified);
        student.Email.Should().Be("new@example.test");
    }

    [Fact]
    public void Create_NormalizesIdentityAndCreatesActiveStudent()
    {
        var result = Student.Create(
            PersonId.New(),
            OrganizationId.New(),
            "  Ada ",
            " Lovelace  ",
            " ada@example.test ",
            null
        );
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("Ada");
        result.Value.LastName.Should().Be("Lovelace");
        result.Value.Email.Should().Be("ada@example.test");
        result.Value.Status.Should().Be(StudentStatus.Active);
    }

    [Fact]
    public void Create_RejectsMissingFirstName()
    {
        var result = Student.Create(
            PersonId.New(),
            OrganizationId.New(),
            " ",
            "Lovelace",
            null,
            null
        );
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(StudentErrors.FirstNameRequired);
    }
}
