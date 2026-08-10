using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Leads.Events;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Leads;

public sealed class LeadTests
{
    [Fact]
    public void Create_WithValidData_CreatesNewLeadAndRaisesEvent()
    {
        LeadIdentity identity = CreateIdentity();
        RequestedTraining training = CreateTraining();
        LeadSource source = LeadSource.Create(LeadSourceType.Website).Value;

        var result = Lead.Create(
            LeadId.New(),
            OrganizationId.New(),
            null,
            identity,
            training,
            source);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(LeadStatus.New);
        result.Value.Identity.Email.Should().Be("john.doe@example.com");
        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<LeadCreatedDomainEvent>();
    }

    [Fact]
    public void Create_WithEmptyOrganizationId_Fails()
    {
        var result = Lead.Create(
            LeadId.New(),
            OrganizationId.Empty,
            null,
            CreateIdentity(),
            CreateTraining(),
            LeadSource.Create(LeadSourceType.Referral).Value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Crm.Leads.OrganizationId.Empty");
    }

    [Fact]
    public void LeadIdentity_NormalizesEmailAndNames()
    {
        var result = LeadIdentity.Create(
            "  John ",
            " Doe  ",
            " JOHN.DOE@EXAMPLE.COM ",
            " +33 6 00 00 00 00 ");

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void LeadSource_OtherWithoutDetail_Fails()
    {
        var result = LeadSource.Create(LeadSourceType.Other);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Crm.Leads.Source.DetailRequired");
    }

    [Fact]
    public void RequestedTraining_NormalizesLicenseCategory()
    {
        var result = RequestedTraining.Create(
            " b ",
            TransmissionPreference.Manual,
            " Nice Centre ");

        result.IsSuccess.Should().BeTrue();
        result.Value.LicenseCategory.Should().Be("B");
        result.Value.PreferredLocation.Should().Be("Nice Centre");
    }

    private static LeadIdentity CreateIdentity() =>
        LeadIdentity.Create(
            "John",
            "Doe",
            "john.doe@example.com",
            "+33600000000").Value;

    private static RequestedTraining CreateTraining() =>
        RequestedTraining.Create(
            "B",
            TransmissionPreference.Manual,
            "Nice").Value;
}
