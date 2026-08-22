using DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.ExamsCertification;

public sealed class ExamPreparationTests
{
    [Fact]
    public void Preparation_should_be_ready_only_when_authoritative_and_manual_checks_are_ready()
    {
        OrganizationId organizationId = new(Guid.NewGuid());
        ExamRegistrationId registrationId = ExamRegistrationId.New();
        PersonId studentId = new(Guid.NewGuid());
        UserId actor = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ExamPreparation preparation = ExamPreparation.Create(organizationId, registrationId, studentId, actor, now).Value;

        ExamPreparationCheckSnapshot[] checks =
        [
            new("ReadinessDecisionCurrent", true, ExamPreparationCheckStatus.Ready, "x", "ExamsCertification"),
            new("ConvocationConfirmed", true, ExamPreparationCheckStatus.Ready, "x", "ExamsCertification"),
            new("DocumentsAvailable", true, ExamPreparationCheckStatus.Ready, "x", "ExamsCertification"),
            new("ResourcesAssigned", true, ExamPreparationCheckStatus.Ready, "x", "ExamsCertification")
        ];

        preparation.Refresh(1, checks, true, true, true, true, true, true, [7, 2, 1, 0], Guid.NewGuid(), "fingerprint", actor, now);

        preparation.Status.Should().Be(ExamPreparationStatus.Ready);
        preparation.IsConfirmed.Should().BeFalse();
        preparation.ReminderOffsetsDays.Should().Equal(7, 2, 1, 0);
    }

    [Fact]
    public void Preparation_should_not_be_ready_when_vehicle_energy_is_not_confirmed()
    {
        OrganizationId organizationId = new(Guid.NewGuid());
        ExamRegistrationId registrationId = ExamRegistrationId.New();
        PersonId studentId = new(Guid.NewGuid());
        UserId actor = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ExamPreparation preparation = ExamPreparation.Create(organizationId, registrationId, studentId, actor, now).Value;

        ExamPreparationCheckSnapshot[] checks =
        [
            new("ReadinessDecisionCurrent", true, ExamPreparationCheckStatus.Ready, "x", "ExamsCertification"),
            new("ConvocationConfirmed", true, ExamPreparationCheckStatus.Ready, "x", "ExamsCertification")
        ];

        preparation.Refresh(1, checks, true, true, true, false, true, true, [1, 0], Guid.NewGuid(), "fingerprint", actor, now);

        preparation.Status.Should().Be(ExamPreparationStatus.Incomplete);
        preparation.Checks.Single(x => x.Code == "VehicleEnergyConfirmed").Status.Should().Be(ExamPreparationCheckStatus.Pending);
    }

    [Fact]
    public void Confirmation_should_be_bound_to_the_current_revision()
    {
        OrganizationId organizationId = new(Guid.NewGuid());
        ExamRegistrationId registrationId = ExamRegistrationId.New();
        PersonId studentId = new(Guid.NewGuid());
        UserId actor = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ExamPreparation preparation = ExamPreparation.Create(organizationId, registrationId, studentId, actor, now).Value;

        ExamPreparationCheckSnapshot[] checks =
        [
            new("ReadinessDecisionCurrent", true, ExamPreparationCheckStatus.Ready, "x", "ExamsCertification"),
            new("ConvocationConfirmed", true, ExamPreparationCheckStatus.Ready, "x", "ExamsCertification")
        ];

        preparation.Refresh(1, checks, false, false, true, true, true, true, [1, 0], Guid.NewGuid(), "first", actor, now);
        preparation.Confirm(actor, now.AddMinutes(1)).IsSuccess.Should().BeTrue();

        preparation.IsConfirmed.Should().BeTrue();
        preparation.ConfirmedRevision.Should().Be(1);

        preparation.Refresh(1, checks, false, false, true, true, true, true, [1, 0], Guid.NewGuid(), "second", actor, now.AddMinutes(2));

        preparation.Revision.Should().Be(2);
        preparation.IsConfirmed.Should().BeFalse();
        preparation.ConfirmedRevision.Should().Be(1);
    }

    [Fact]
    public void Preparation_cannot_be_confirmed_while_incomplete()
    {
        OrganizationId organizationId = new(Guid.NewGuid());
        ExamRegistrationId registrationId = ExamRegistrationId.New();
        PersonId studentId = new(Guid.NewGuid());
        UserId actor = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ExamPreparation preparation = ExamPreparation.Create(organizationId, registrationId, studentId, actor, now).Value;

        ExamPreparationCheckSnapshot[] checks =
        [
            new("ReadinessDecisionCurrent", true, ExamPreparationCheckStatus.Pending, "x", "ExamsCertification")
        ];

        preparation.Refresh(1, checks, false, false, true, true, true, true, [1, 0], Guid.NewGuid(), "fingerprint", actor, now);

        preparation.Confirm(actor, now.AddMinutes(1)).IsFailure.Should().BeTrue();
        preparation.IsConfirmed.Should().BeFalse();
    }

    [Fact]
    public void Refresh_should_reject_duplicate_authoritative_check_codes()
    {
        OrganizationId organizationId = new(Guid.NewGuid());
        ExamRegistrationId registrationId = ExamRegistrationId.New();
        PersonId studentId = new(Guid.NewGuid());
        UserId actor = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ExamPreparation preparation = ExamPreparation.Create(organizationId, registrationId, studentId, actor, now).Value;

        ExamPreparationCheckSnapshot[] checks =
        [
            new("Same", true, ExamPreparationCheckStatus.Ready, "x", "ExamsCertification"),
            new("Same", true, ExamPreparationCheckStatus.Ready, "x", "ExamsCertification")
        ];

        preparation.Refresh(1, checks, false, false, true, true, true, true, [1, 0], Guid.NewGuid(), "fingerprint", actor, now)
            .IsFailure.Should().BeTrue();
    }
}
