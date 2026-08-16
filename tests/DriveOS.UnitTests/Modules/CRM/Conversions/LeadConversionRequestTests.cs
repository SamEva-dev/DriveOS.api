using DriveOS.Modules.CRM.Domain.Conversions;
using DriveOS.Modules.CRM.Domain.Conversions.Events;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Conversions;

public sealed class LeadConversionRequestTests
{
    [Fact]
    public void Request_KeepsAcceptedOfferAndRaisesIntegrationBoundaryEvent()
    {
        OrganizationId organizationId = OrganizationId.New();
        Lead lead = Lead.Create(
            LeadId.New(),
            organizationId,
            BranchId.New(),
            LeadIdentity.Create("Jane", "Doe", "jane@example.com", null).Value,
            RequestedTraining.Create("B", TransmissionPreference.Manual, null).Value,
            LeadSource.Create(LeadSourceType.Website).Value
        ).Value;
        CommercialOfferId offerId = CommercialOfferId.New();

        LeadConversion conversion = LeadConversion.Request(
            organizationId,
            lead,
            offerId,
            BranchId.New(),
            UserId.New(),
            "B",
            true,
            true,
            true,
            null,
            null,
            "ID_CARD"
        );

        conversion.Status.Should().Be(LeadConversionStatus.Requested);
        conversion.AcceptedOfferId.Should().Be(offerId);
        conversion.StudentPersonId.Should().BeNull();
        conversion.StudentEnrollmentId.Should().BeNull();
        conversion
            .DomainEvents.Should()
            .ContainSingle(x => x is LeadConversionRequestedDomainEvent);
    }

    [Fact]
    public void Complete_IsIdempotentForTheSameStudentAndEnrollment()
    {
        OrganizationId organizationId = OrganizationId.New();
        Lead lead = Lead.Create(
            LeadId.New(),
            organizationId,
            BranchId.New(),
            LeadIdentity.Create("Jane", "Doe", "jane@example.com", null).Value,
            RequestedTraining.Create("B", TransmissionPreference.Manual, null).Value,
            LeadSource.Create(LeadSourceType.Website).Value
        ).Value;
        LeadConversion conversion = LeadConversion.Request(
            organizationId,
            lead,
            CommercialOfferId.New(),
            BranchId.New(),
            UserId.New(),
            "B",
            true,
            true,
            true,
            null,
            null,
            "ID_CARD"
        );
        PersonId personId = PersonId.New();
        DraftEnrollmentId enrollmentId = DraftEnrollmentId.New();
        DateTimeOffset completedAtUtc = DateTimeOffset.UtcNow;

        conversion.Complete(personId, enrollmentId, completedAtUtc).IsSuccess.Should().BeTrue();
        conversion
            .Complete(personId, enrollmentId, completedAtUtc.AddMinutes(1))
            .IsSuccess.Should()
            .BeTrue();

        conversion.Status.Should().Be(LeadConversionStatus.Completed);
        conversion.StudentPersonId.Should().Be(personId);
        conversion.StudentEnrollmentId.Should().Be(enrollmentId);
        conversion
            .DomainEvents.Should()
            .ContainSingle(x => x is LeadConversionCompletedDomainEvent);
    }

    [Fact]
    public void Complete_RejectsDifferentTargetsAfterCompletion()
    {
        OrganizationId organizationId = OrganizationId.New();
        Lead lead = Lead.Create(
            LeadId.New(),
            organizationId,
            null,
            LeadIdentity.Create("Jane", "Doe", null, null).Value,
            RequestedTraining.Create("B", TransmissionPreference.Manual, null).Value,
            LeadSource.Create(LeadSourceType.Website).Value
        ).Value;
        LeadConversion conversion = LeadConversion.Request(
            organizationId,
            lead,
            CommercialOfferId.New(),
            BranchId.New(),
            UserId.New(),
            "B",
            true,
            true,
            true,
            null,
            null,
            null
        );
        conversion.Complete(PersonId.New(), DraftEnrollmentId.New(), DateTimeOffset.UtcNow);

        var result = conversion.Complete(
            PersonId.New(),
            DraftEnrollmentId.New(),
            DateTimeOffset.UtcNow
        );

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LeadConversionErrors.AlreadyCompleted);
    }
}
