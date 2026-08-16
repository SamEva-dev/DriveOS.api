using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class EnrollmentTests
{
    [Fact]
    public void CreateDirectDraft_ShouldPersistSourceAndIdempotencyData()
    {
        var result = Enrollment.CreateDirectDraft(
            DraftEnrollmentId.New(),
            OrganizationId.New(),
            PersonId.New(),
            BranchId.New(),
            "PERMIS-B",
            EnrollmentSource.DirectBranch,
            "enroll-request-001",
            "fr",
            "FR",
            true
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Source.Should().Be(EnrollmentSource.DirectBranch);
        result.Value.IdempotencyKey.Should().Be("enroll-request-001");
        result.Value.RegulatoryCountryCode.Should().Be("FR");
        result.Value.RequiredConsentsAccepted.Should().BeTrue();
    }

    [Fact]
    public void CreateDirectDraft_ShouldRejectMissingRequiredConsents()
    {
        var result = Enrollment.CreateDirectDraft(
            DraftEnrollmentId.New(),
            OrganizationId.New(),
            PersonId.New(),
            BranchId.New(),
            "PERMIS-B",
            EnrollmentSource.DirectBranch,
            "enroll-request-002",
            "FR",
            "fr",
            false
        );

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentErrors.RequiredConsentsMissing);
    }

    [Fact]
    public void CreateDraft_CreatesDraftWithTypedIdentifiers()
    {
        var result = Enrollment.CreateDraft(
            DraftEnrollmentId.New(),
            OrganizationId.New(),
            PersonId.New(),
            BranchId.New(),
            LeadId.New(),
            "  B-PERMIT "
        );
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(EnrollmentStatus.Draft);
        result.Value.TrainingCode.Should().Be("B-PERMIT");
    }

    [Fact]
    public void CreateDraft_RejectsMissingTrainingCode()
    {
        var result = Enrollment.CreateDraft(
            DraftEnrollmentId.New(),
            OrganizationId.New(),
            PersonId.New(),
            BranchId.New(),
            null,
            " "
        );
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentErrors.TrainingCodeRequired);
    }
}
